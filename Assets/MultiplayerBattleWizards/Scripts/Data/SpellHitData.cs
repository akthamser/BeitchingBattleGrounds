using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data sent to a player hit by a spell.
/// </summary>
[System.Serializable]
public class SpellHitData
{
    public string spellName;                // Spell that hit the target.
    public int attackerID;                  // ID of the player who casted the spell.
    public float hitDirX;                   // Direction of spell from target (normalized) X axis.
    public float hitDirY;                   // Direction of spell from target (normalized) Y axis.
}