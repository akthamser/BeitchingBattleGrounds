using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "New Spell", order = 0)]
public class SpellData : ScriptableObject
{
    // Info
    public string spellName = "New Spell";      // Name of the spell.
    public SpellCastType castType = SpellCastType.Projectile;       // Type of spell to cast.
    public Sprite icon;                         // UI icon.
    public float cooldown;                      // Minimum time between casts (seconds).
    public AudioClip castSfx;                   // SFX to play when the spell is cast.

    // Projectile
    public GameObject projectilePrefab;         // Prefab spawned upon cast. 
    public float projectileSpeed;               // Velocity of the projectile.
    public float projectileDrop;                // Bullet drop applied over time.

    public SpellVelocityAffectorType projectileVelocityAffectorType;    // Effect applied to the Y velocity of the projectile.
    public float projectileVelocityAffectorIntensity;                   // Intensity of the effect applied to the Y velocity.

    // On Self Cast
    public SpellOnSelfCast onSelfCast;

    // On Hit Target
    public SpellOnHit onHitTarget;

    // On Hit Wall
}

// Type of spell to cast.
public enum SpellCastType
{
    Projectile,
    Self
}

// Type of effect applied to the Y velocity of the projectile.
public enum SpellVelocityAffectorType
{
    None,
    SinWave,
}

// When the player casts a spell on themselves, what type of teleportation occurs?
public enum SpellTeleportType
{
    RandomPosition,
    RandomPlayer,
    NearestPlayer,
    FurthestPlayer,
    TopOfMap
}