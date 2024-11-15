using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spell projectile shot by a player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SpellProjectile : Spell
{
    [Header("Components")]
    public PhotonView photonView;
    public Rigidbody2D rig;

    void Awake ()
    {
        // Get missing components.
        if(!rig) rig = GetComponent<Rigidbody2D>();
        if(!photonView) photonView = GetComponent<PhotonView>();
    }

    // Called when projectile is created.
    // Sets up some initial data.
    public void Initiate (SpellProjectileData data)
    {
        casterID = data.casterID;
        isMine = data.isMine;
        spellData = data.spell;
        rig.velocity = data.velocity;
        rig.gravityScale = spellData.projectileDrop;

        // If the spell never hits anything and goes outside of the map, we'll destroy it in 10 seconds.
        Destroy(gameObject, 10.0f);
    }

    void Update ()
    {
        // Rotate projectile to face its velocity.
        transform.right = rig.velocity.normalized * (rig.velocity.x > 0 ? 1 : -1);

        if(spellData.projectileVelocityAffectorType != SpellVelocityAffectorType.None)
            VelocityAffector();
    }

    // Apply an affector to the Y velocity. Sin wave, etc.
    void VelocityAffector ()
    {
        float yVel = 0.0f;

        if(spellData.projectileVelocityAffectorType == SpellVelocityAffectorType.SinWave)
            yVel = Mathf.Sin(Time.time * (spellData.projectileVelocityAffectorIntensity * 2)) * spellData.projectileVelocityAffectorIntensity;

        rig.velocity = new Vector3(rig.velocity.x, yVel - rig.gravityScale);
    }

    // Called when the projectile hits a player or ground tile.
    void HitDestroy ()
    {
        GetComponent<Collider2D>().enabled = false;
        rig.velocity = Vector2.zero;
        rig.isKinematic = true;

        // If we spawn an object on hit - instantiate it.
        if(spellData.onHitTarget.doSpawnObject)
        {
            GameObject obj = Instantiate(spellData.onHitTarget.objectToSpawn, transform.position, Quaternion.identity);

            // If the destroy time is greater than 0, set the object to be destroyed in that time.
            if(spellData.onHitTarget.objectToSpawnDestroyTime > 0.0f)
                Destroy(obj, spellData.onHitTarget.objectToSpawnDestroyTime);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D (Collider2D col)
    {
        // Only do stuff if we hit a ground tile or player.
        if(!col.CompareTag("Ground") && !col.CompareTag("Player"))
            return;

        // If we're able to go through tiles, check that.
        if(spellData.onHitTarget.goThroughTiles)
            if(col.CompareTag("Ground")) return;

        // The caster of the spell manages the hit detection - client side.
        if(!isMine)
        {
            HitDestroy();
            return;
        }

        // Did we enter the trigger of a PLAYER?
        if(col.gameObject.CompareTag("Player"))
        {
            // Did we hit an enemy?
            if(PlayerManager.inst.GetPlayer(col.gameObject).networkID != casterID)
            {
                // Get the enemy we hit.
                Player hitPlayer = col.gameObject.GetComponent<Player>();
                SpellHitData hitData = new SpellHitData();

                // Create the hit data.
                hitData.spellName = spellData.spellName;
                hitData.attackerID = casterID;

                Vector3 hitDir = (transform.position - col.gameObject.transform.position).normalized;

                hitData.hitDirX = hitDir.x;
                hitData.hitDirY = hitDir.y;
                
                byte[] serializedHitData = Serializer.Serialize(hitData);

                // Hit the enemy with an RPC - sending the hit data.
                hitPlayer.photonView.RPC("Hit", PhotonTargets.All, serializedHitData);

                // Destroy the projectile.
                HitDestroy();
            }
        }

        HitDestroy();
    }
}