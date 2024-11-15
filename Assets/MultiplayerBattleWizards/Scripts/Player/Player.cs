using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for a player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonView))]
public class Player : MonoBehaviour
{
    public PlayerState curState;            // Current state of the player.

    [Header("Stats")]
    public int curHp;                       // Current health points.
    public int maxHp;                       // Maximum health points.
    public int score;                       // Player's current score (kills).
    [HideInInspector]
    public Player curAttacker;              // Player who's currently attacking us.

    [Header("Player Components")]
    public PlayerMove move;                 // Player's 'PlayerMove' component.
    public PlayerAttack attack;             // Player's 'PlayerAttack' component.
    public PlayerInput input;               // Player's 'PlayerInput' component.
    [HideInInspector]
    public PlayerUI ui;                     // Player's 'PlayerUI' component (in the canvas).

    [HideInInspector]
    public NetworkPlayer networkPlayer;     // Player's 'NetworkPlayer' class.

    [Header("Components")]
    public Rigidbody2D rig;                 // Player's 'Rigidbody2D' component.
    public Animator anim;                   // Player's 'Animator' component.
    public SpriteRenderer sr;               // Player's 'SpriteRenderer' component.
    public PhotonView photonView;           // Player's 'PhotonView' component.
    public AudioSource audioSource;         // Player's 'AudioSource' component.

    // Events
    [HideInInspector]
    public UnityEvent onCastSpell;          // Called when the player casts a spell.

    // Private Values
    private bool flashingSpriteColor;       // Are we currently flashing the sprite color?
    private float respawnTime;              // Time to re-spawn the player.
    private Color curColor = Color.white;   // Players current color.

    // Stun Info
    private float stunEndTime;

    #region Subscribing to Events

    void OnEnable ()
    {
        // Subscribing to events.
        GameManager.inst.onGameStart.AddListener(OnGameStart);
        GameManager.inst.onGameWin += OnGameWin;
    }

    void OnDisable ()
    {
        // Un-subscribing from events.
        GameManager.inst.onGameStart.RemoveListener(OnGameStart);
        GameManager.inst.onGameWin -= OnGameWin;
    }

    #endregion

    void Awake ()
    {
        // Get missing components.
        if(!move) move = GetComponent<PlayerMove>();
        if(!attack) attack = GetComponent<PlayerAttack>();
        if(!input) input = GetComponent<PlayerInput>();
        if(!rig) rig = GetComponent<Rigidbody2D>();
        if(!anim) anim = GetComponent<Animator>();
        if(!sr) sr = GetComponent<SpriteRenderer>();
        if(!audioSource) audioSource = GetComponent<AudioSource>();

        // Tell movement, attack and input class' to get components.
        move.SetComponents(this);
        attack.SetComponents(this);
        input.SetComponents(this);
    }

    void Start ()
    {
        // Get and set the animator controller.
        // Used to get each player their unique color.
        anim.runtimeAnimatorController = PlayerManager.inst.GetAnimatorController(networkPlayer.networkID);

        // If this isn't our player, make it kinematic.
        if(!photonView.isMine)
            rig.bodyType = RigidbodyType2D.Kinematic;

        // Begin by giving the player a random spell.
        if(photonView.isMine)
            photonView.RPC("GiveSpell", PhotonTargets.All, SpellManager.inst.GetRandomSpell().spellName);
    }

    void Update ()
    {
        // Don't call the Update function if this isn't our player.
        if(!photonView.isMine)
            return;

        SetState();
        CheckYPos();

        // If we're dead, check if we're able to respawn.
        if(curState == PlayerState.Dead)
        {
            if(Time.time >= respawnTime)
                Spawn(GameManager.inst.spawnPoints[Random.Range(0, GameManager.inst.spawnPoints.Length)]);
        }

        // If we're stunned, check if we're ready to go back to normal.
        else if(curState == PlayerState.Stunned)
        {
            if(Time.time >= stunEndTime)
                photonView.RPC("RemoveStun", PhotonTargets.All);
        }
    }

    // Updates the player's current state
    void SetState ()
    {
        // Don't change states automatically if we're dead or stunned.
        if(curState != PlayerState.Dead && curState != PlayerState.Stunned)
        {
            // Idle - if we're standing still.
            if(rig.velocity.magnitude == 0)
                curState = PlayerState.Idle;
            // Moving - if we're moving and grounded.
            else if(Input.GetAxis("Horizontal") != 0 && move.grounded)
                curState = PlayerState.Moving;
            // InAir - if we're moving and not grounded.
            else if(rig.velocity.magnitude != 0 && !move.grounded)
                curState = PlayerState.InAir;
        }

        // Change the state also in the animator.
        anim.SetInteger("State", (int)curState);
    }

    // Called when the player gets hit by a spell.
    // Damages / applies spell properties to player.
    [PunRPC]
    public void Hit (byte[] rawData)
    {
        // Deserialize the byte array to a SpellHitData class.
        SpellHitData data = (SpellHitData)Serializer.Deserialize(rawData);
        SpellData spell = SpellManager.inst.GetSpell(data.spellName);

        curAttacker = PlayerManager.inst.GetPlayer(data.attackerID).player;

        // Does the spell stun the player?
        if(spell.onHitTarget.doStun)
            Stun(spell.onHitTarget.stunDuration);

        // Does the spell heal the caster?
        if(spell.onHitTarget.doHealCaster)
            PlayerManager.inst.GetPlayer(data.attackerID).player.Heal(spell.onHitTarget.healthToCaster);

        // Does the spell spawn an object?
        if(spell.onHitTarget.doSpawnObject)
            Instantiate(spell.onHitTarget.objectToSpawn, transform.position, Quaternion.identity);

        // Does the spell do damage?
        if(spell.onHitTarget.doDamage || spell.onSelfCast.doDamageNearby)
        {
            int damage = spell.onHitTarget.doDamage ? spell.onHitTarget.damage : spell.onSelfCast.nearbyDamage;

            if(curHp - damage <= 0 && photonView.isMine)
                Die();
            else
                curHp -= damage;

            // Update player's UI.
            ui.UpdateHealth();

            // Flash the player red quickly.
            StartCoroutine(ColorFlash(Color.red));
        }

        // Play sound effect.
        AudioManager.inst.Play(audioSource, AudioManager.inst.playerHit);
    }

    // Called when the player dies.
    [PunRPC]
    public void Die ()
    {
        curHp = 0;

        curState = PlayerState.Dead;

        // Create a particle effect.
        ParticleManager.inst.Create(ParticleManager.inst.playerDeath, transform.position);

        // Shake the camera.
        Shake.inst.ShakeObject(Camera.main.gameObject, 0.2f, 0.1f, 5.0f);

        // If we're stunned, remove the stun.
        RemoveStun();

        if(photonView.isMine)
        {
            // Disable physics.
            rig.velocity = Vector3.zero;
            rig.bodyType = RigidbodyType2D.Kinematic;

            // Hide the player off-screen.
            transform.position = new Vector3(0, 100, 0);

            respawnTime = Time.time + GameManager.inst.deadDuration;

            // Call this again but for all other clients.
            photonView.RPC("Die", PhotonTargets.Others);

            if(curAttacker != null)
            {
                // Tell the player who killed us.
                curAttacker.photonView.RPC("KillPlayer", PhotonTargets.All, networkPlayer.networkID);
            }
        }

        ui.UpdateHealth();

        // Play sound effect.
        AudioManager.inst.Play(audioSource, AudioManager.inst.playerDeath);
    }

    // Respawns the player on the map.
    [PunRPC]
    public void Spawn (Vector3 spawnPoint)
    {
        // Set stats.
        curHp = maxHp;
        curState = PlayerState.Idle;
        curAttacker = null;

        // Create particle effect.
        ParticleManager.inst.Create(ParticleManager.inst.playerSpawn, transform.position, transform);

        // Update the UI.
        ui.UpdateHealth();

        // Enable physics if this is our player.
        if(photonView.isMine)
        {
            rig.bodyType = RigidbodyType2D.Dynamic;

            // Set spawn point.
            transform.position = spawnPoint;

            // Call this function for all other clients.
            photonView.RPC("Spawn", PhotonTargets.Others, spawnPoint);

            // If we get a random spell when we spawn, do so.
            if(GameManager.inst.spellDistribution == SpellDistributionType.RandomOnSpawn)
                photonView.RPC("GiveSpell", PhotonTargets.All, SpellManager.inst.GetRandomSpell());
        }
    }

    // Heals the player a certain amount of health.
    [PunRPC]
    public void Heal (int healthToGive)
    {
        // Make sure our health doesn't exceed our maximum.
        if(curHp + healthToGive > maxHp)
            curHp = maxHp;
        else
            curHp += healthToGive;

        // Spawn particle effect.
        ParticleManager.inst.Create(ParticleManager.inst.playerHeal, transform.position, transform);

        // Update the UI.
        ui.UpdateHealth();
    }

    // Called when we kill another player.
    [PunRPC]
    public void KillPlayer (int victimID)
    {
        ++score;

        // Host checks the win condition.
        if(PhotonNetwork.isMasterClient)
            GameManager.inst.CheckWinCondition();

        // Update UI score.
        ui.UpdateScore();
    }

    // Called when the player gets stunned.
    // Prevents them from moving and attacking for a set duration.
    [PunRPC]
    void Stun (float duration)
    {
        curState = PlayerState.Stunned;
        stunEndTime = Time.time + duration;

        // Set blue color.
        curColor = Color.blue;
        sr.color = curColor;

        // Set stats.
        move.canMove = false;
        attack.canAttack = false;
        rig.velocity = new Vector2(0, rig.velocity.y);
    }

    // Called when the player's stun duration has ran out.
    // Returns the player to normal.
    [PunRPC]
    void RemoveStun ()
    {
        if(curState != PlayerState.Dead)
            curState = PlayerState.Idle;

        curColor = Color.white;
        sr.color = curColor;

        move.canMove = true;
        attack.canAttack = true;
    }

    // Teleports the player to a specific position.
    [PunRPC]
    public void Teleport (Vector3 tpPos)
    {
        // Create a particle effect.
        ParticleManager.inst.Create(ParticleManager.inst.playerTeleport, transform.position);

        transform.position = tpPos;
    }

    // Flashes the player's sprite color.
    IEnumerator ColorFlash (Color color, float duration = 0.1f)
    {
        sr.color = color;
        yield return new WaitForSeconds(duration);
        sr.color = curColor;
    }

    // Checks if the player has fallen off the map - if so, kill them.
    void CheckYPos()
    {
        // Has the player fallen on the map and they aren't dead? If so, kill them.
        if(transform.position.y <= GameManager.inst.minYKillPos && curState != PlayerState.Dead)
            Die();
    }

    // GameManager.onGameStart event.
    // Called when the game begins and the players can start fighting.
    void OnGameStart ()
    {
        move.canMove = true;
        attack.canAttack = true;
    }

    // GameManager.onGameWin event.
    // Called when a player has won the game.
    void OnGameWin (int winningPlayer)
    {
        attack.canAttack = false;
    }
}

public enum PlayerState
{
    Idle,       // 0
    Moving,     // 1
    InAir,      // 2
    Attack,     // 3
    Stunned,    // 4
    Dead        // 5
}