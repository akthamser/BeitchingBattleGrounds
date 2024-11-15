using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all the spells in the game.
/// </summary>
public class SpellManager : MonoBehaviour
{
    public List<SpellData> spells = new List<SpellData>();          // List of all available spells.

    // Instance
    public static SpellManager inst;

    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    // Returns a spell with the given name.
    public SpellData GetSpell (string spellName)
    {
        return spells.Find(x => x.spellName == spellName);
    }

    // Returns a random spell.
    public SpellData GetRandomSpell ()
    {
        return spells[Random.Range(0, spells.Count)];
    }

    // Returns a random spell, excluding the one sent.
    public SpellData GetRandomSpell (SpellData excluding)
    {
        SpellData curSpell = excluding;

        while(curSpell == excluding)
            curSpell = GetRandomSpell();

        return curSpell;
    }

    // Returns a random spell index.
    public int GetRandomSpellIndex ()
    {
        return spells.IndexOf(GetRandomSpell());
    }

    // Returns a random spell index, excluding the one sent.
    public int GetRandomSpellIndex (SpellData excluding)
    {
        return spells.IndexOf(GetRandomSpell(excluding));
    }
}