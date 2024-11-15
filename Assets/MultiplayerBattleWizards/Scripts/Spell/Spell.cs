using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for a spell.
/// </summary>
abstract public class Spell : MonoBehaviour
{
    [HideInInspector] public SpellData spellData;   // The spell we're casting.
    [HideInInspector] public int casterID;          // ID of the player who cast the spell.
    [HideInInspector] public bool isMine;           // Is this instance of the spell the original one that was shot by the caster? Used for checking hit detection.
}