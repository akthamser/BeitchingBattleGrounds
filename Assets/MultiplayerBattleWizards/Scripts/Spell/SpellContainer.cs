using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds usable data for a spell. Cooldown timer, usability, etc.
/// </summary>
[System.Serializable]
public class SpellContainer
{
    public SpellData spell;             // SpellData scriptable object.

    [HideInInspector]
    public float lastCastTime;          // Last time the spell was casted (used for cooldown).

    // Basic constructor.
    public SpellContainer (SpellData spell)
    {
        this.spell = spell;
        lastCastTime = 0;
    }

    // Can we cast the spell? Based on cooldown time.
    public bool CanCast()
    {
        if(Time.time - lastCastTime >= spell.cooldown)
            return true;
        else
            return false;
    }

    // Returns the cooldown progress (0.0 - 1.0)
    public float GetCooldownProgress ()
    {
        return (Time.time - lastCastTime) / spell.cooldown;
    }

    // Called when the spell gets casted.
    public void OnCast()
    {
        lastCastTime = Time.time;
    }
}