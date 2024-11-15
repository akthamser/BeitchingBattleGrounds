using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds info for when the player casts a spell on themselves.
/// </summary>
[System.Serializable]
public class SpellOnSelfCast
{
    public bool doHealCaster;                   // Give health to the player who casted the spell?
    public int healthToCaster;                  // Health to give to the caster of the spell.

    public bool doDamageNearby;                 // Damage nearby players.
    public float nearbyDamageRange;             // Range of dealing damage nearby.
    public int nearbyDamage;                    // Damage to deal nearby.

    public bool doStunNearby;                   // Stun nearby players.
    public float nearbyStunRange;               // Range of stunning nearby players.
    public float nearbyStunDuration;            // Duration of the stun.

    public bool doTeleport;                     // Teleport the player.
    public SpellTeleportType teleportType;      // Type of teleportation applied to the player.

    public bool doSpawnObject;                  // Do we instantiate an object?
    public GameObject objectToSpawn;            // Object to instantiate.
    public float objectToSpawnDestroyTime;      // Time to destroy the object after spawning it. 0 = don't destroy.
}