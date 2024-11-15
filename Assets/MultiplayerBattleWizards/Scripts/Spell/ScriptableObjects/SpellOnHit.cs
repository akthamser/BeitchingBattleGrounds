using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellOnHit
{
    public bool doDamage;                       // Deal damage to the hit target?
    public int damage;                          // Damage to deal.

    public bool doStun;                         // Stun the hit target?
    public float stunDuration;                  // Duration of the stun.

    public bool doSpawnObject;                  // Do we instantiate an object?
    public GameObject objectToSpawn;            // Object to instantiate.
    public float objectToSpawnDestroyTime;      // Time to destroy the object after spawning it. 0 = don't destroy.

    public bool doHealCaster;                   // Give health to the player who casted the spell?
    public int healthToCaster;                  // Health to give to the caster of the spell.

    public bool goThroughTiles;                 // Will the projectile go through tiles?
}