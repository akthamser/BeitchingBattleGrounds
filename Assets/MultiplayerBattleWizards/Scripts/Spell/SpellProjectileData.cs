using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container that holds initiation data for the projectile.
/// Used for setting up the projectile once spawned.
/// </summary>
public class SpellProjectileData
{
    public int casterID;            // ID of the player casting the spell.
    public bool isMine;             // Is this the original / the one the client shot?
    public SpellData spell;         // Name of the spell (can't send over SpellData so name will do).
    public Vector2 velocity;        // Initial velocity for the projectile to move at.

    public SpellProjectileData (int casterID, bool isMine, SpellData spell, Vector2 velocity)
    {
        this.casterID = casterID;
        this.isMine = isMine;
        this.spell = spell;
        this.velocity = velocity;
    }
}