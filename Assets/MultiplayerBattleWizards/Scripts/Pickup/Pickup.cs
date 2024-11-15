using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Pickup : MonoBehaviour
{
    public PickupType type;         // The type of pickup.

    public float bobSpeed;          // Speed at which the pickup moves up and down.
    public float bobHeight;         // Height the pickup bobs up and down to.

    private Vector3 originPos;      // Starting position of the pickup.
    private Vector3 targetPos;      // Target pos we're bobbing to.

    [Header("Health")]
    public int healthToGive;        // Health to give to player (if type is Health).

    [Header("Spell")]
    public bool randomSpell;        // Do we give the player a random spell?
    public int spellToGive;         // Spell to give to player (if type is Spell).

    [Header("Components")]
    public PhotonView photonView;

    void Start ()
    {
        originPos = transform.position;
        targetPos = transform.position;

        // Create a spawn particle.
        ParticleManager.inst.Create(ParticleManager.inst.pickupSpawn, transform.position);
    }

    void Update ()
    {
        // Bob up and down.
        transform.position = Vector3.MoveTowards(transform.position, targetPos, bobSpeed * Time.deltaTime);

        if(transform.position == targetPos)
            targetPos = originPos + (Vector3.up * (transform.position.y > originPos.y ? -bobHeight / 2 : bobHeight / 2));
    }

    // Called when a player enters the trigger of the pickup.
    [PunRPC]
    void ActivatedByPlayer (int playerID, int randomSpellIndex = 0)
    {
        // Get the player.
        Player player = PlayerManager.inst.GetPlayer(playerID).player;

        if(type == PickupType.Health)
            player.Heal(healthToGive);
        else if(type == PickupType.Spell)
        {
            int spellIndex = randomSpell ? randomSpellIndex : spellToGive;

            SpellData spell = SpellManager.inst.spells[spellIndex];
            player.attack.GiveSpell(spell);
        }

        // Destroy the pickup.
        PickupManager.inst.curPickups.Remove(gameObject);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        // Did a player run into us?
        if(other.gameObject.CompareTag("Player"))
        {
            // Get that player.
            Player player = PlayerManager.inst.GetPlayer(other.gameObject).player;
            
            // Make sure that picking up pickups is a server side operation.
            if(PhotonNetwork.isMasterClient)
            {
                photonView.RPC("ActivatedByPlayer", PhotonTargets.All, player.networkPlayer.networkID, SpellManager.inst.GetRandomSpellIndex(player.attack.curSpell.spell));
            }
        }
    }
}

public enum PickupType
{
    Health,
    Spell
}