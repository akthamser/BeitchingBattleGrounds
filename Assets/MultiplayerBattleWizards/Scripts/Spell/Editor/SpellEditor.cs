using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor extension of the SpellData scriptable object.
/// </summary>
[CustomEditor(typeof(SpellData))]
public class SpellEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        SpellData spell = (SpellData)target;

        EditorUtility.SetDirty(spell);

        // Setting up header font.
        GUIStyle headerStyle = new GUIStyle();
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 15;

        // ---------------------------------
        //      I N F O
        // ---------------------------------

        EditorGUILayout.LabelField("Info", headerStyle);
        EditorGUILayout.Space();

        spell.spellName = EditorGUILayout.TextField("Name", spell.spellName);
        spell.castType = (SpellCastType)EditorGUILayout.EnumPopup("Cast Type", spell.castType);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("UI Icon");
        spell.icon = (Sprite)EditorGUILayout.ObjectField(spell.icon, typeof(Sprite), false);
        EditorGUILayout.EndHorizontal();
        spell.cooldown = EditorGUILayout.FloatField("Cooldown", spell.cooldown);
        spell.castSfx = (AudioClip)EditorGUILayout.ObjectField("Cast SFX", spell.castSfx, typeof(AudioClip), false);

        // ---------------------------------
        //      P R O J E C T I L E
        // ---------------------------------

        if(spell.castType == SpellCastType.Projectile)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Projectile", headerStyle);
            EditorGUILayout.Space();

            spell.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", spell.projectilePrefab, typeof(GameObject), false);
            spell.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", spell.projectileSpeed);

            spell.projectileDrop = EditorGUILayout.Slider("Projectile Drop", spell.projectileDrop, 0.0f, 1.0f);

            spell.projectileVelocityAffectorType = (SpellVelocityAffectorType)EditorGUILayout.EnumPopup("Velocity Affector", spell.projectileVelocityAffectorType);

            if(spell.projectileVelocityAffectorType != SpellVelocityAffectorType.None)
            {
                ++EditorGUI.indentLevel;
                spell.projectileVelocityAffectorIntensity = EditorGUILayout.FloatField("Intensity", spell.projectileVelocityAffectorIntensity);
                --EditorGUI.indentLevel;
            }

            // ---------------------------------
            //      O N   H I T   T A R G E T
            // ---------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("On Hit Target", headerStyle);
            EditorGUILayout.Space();

            // Damage
            spell.onHitTarget.doDamage = EditorGUILayout.Toggle("Do Damage", spell.onHitTarget.doDamage);

            if(spell.onHitTarget.doDamage)
            {
                ++EditorGUI.indentLevel;
                spell.onHitTarget.damage = EditorGUILayout.IntField("Damage", spell.onHitTarget.damage);
                --EditorGUI.indentLevel;
            }

            // Stun
            spell.onHitTarget.doStun = EditorGUILayout.Toggle("Do Stun", spell.onHitTarget.doStun);

            if(spell.onHitTarget.doStun)
            {
                ++EditorGUI.indentLevel;
                spell.onHitTarget.stunDuration = EditorGUILayout.FloatField("Stun Duration", spell.onHitTarget.stunDuration);
                --EditorGUI.indentLevel;
            }

            // Spawn
            spell.onHitTarget.doSpawnObject = EditorGUILayout.Toggle("Do Spawn Object", spell.onHitTarget.doSpawnObject);

            if(spell.onHitTarget.doSpawnObject)
            {
                ++EditorGUI.indentLevel;
                spell.onHitTarget.objectToSpawn = (GameObject)EditorGUILayout.ObjectField("Object to Spawn", spell.onHitTarget.objectToSpawn, typeof(GameObject), false);
                spell.onHitTarget.objectToSpawnDestroyTime = EditorGUILayout.FloatField("Destroy Time", spell.onHitTarget.objectToSpawnDestroyTime);
                --EditorGUI.indentLevel;
            }

            // Heal Caster
            spell.onHitTarget.doHealCaster = EditorGUILayout.Toggle("Do Heal Caster", spell.onHitTarget.doHealCaster);

            if(spell.onHitTarget.doHealCaster)
            {
                ++EditorGUI.indentLevel;
                spell.onHitTarget.healthToCaster = EditorGUILayout.IntField("Health to Caster", spell.onHitTarget.healthToCaster);
                --EditorGUI.indentLevel;
            }

            // Go through tiles.
            spell.onHitTarget.goThroughTiles = EditorGUILayout.Toggle("Go Through Tiles", spell.onHitTarget.goThroughTiles);
        }
        else if(spell.castType == SpellCastType.Self)
        {
            // ---------------------------------
            //      O N   S E L F   C A S T
            // ---------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("On Self Cast", headerStyle);
            EditorGUILayout.Space();

            // Heal Caster
            spell.onSelfCast.doHealCaster = EditorGUILayout.Toggle("Do Heal Caster", spell.onSelfCast.doHealCaster);

            if(spell.onSelfCast.doHealCaster)
            {
                ++EditorGUI.indentLevel;
                spell.onSelfCast.healthToCaster = EditorGUILayout.IntField("Health to Caster", spell.onSelfCast.healthToCaster);
                --EditorGUI.indentLevel;
            }

            // Nearby Damage
            spell.onSelfCast.doDamageNearby = EditorGUILayout.Toggle("Do Damage Nearby", spell.onSelfCast.doDamageNearby);

            if(spell.onSelfCast.doDamageNearby)
            {
                ++EditorGUI.indentLevel;
                spell.onSelfCast.nearbyDamageRange = EditorGUILayout.FloatField("Range", spell.onSelfCast.nearbyDamageRange);
                spell.onSelfCast.nearbyDamage = EditorGUILayout.IntField("Damage", spell.onSelfCast.nearbyDamage);
                --EditorGUI.indentLevel;
            }

            // Nearby Stun
            spell.onSelfCast.doStunNearby = EditorGUILayout.Toggle("Do Stun Nearby", spell.onSelfCast.doStunNearby);

            if(spell.onSelfCast.doStunNearby)
            {
                ++EditorGUI.indentLevel;
                spell.onSelfCast.nearbyStunRange = EditorGUILayout.FloatField("Range", spell.onSelfCast.nearbyStunRange);
                spell.onSelfCast.nearbyStunDuration = EditorGUILayout.FloatField("Stun Duration", spell.onSelfCast.nearbyStunDuration);
                --EditorGUI.indentLevel;
            }

            // Teleport
            spell.onSelfCast.doTeleport = EditorGUILayout.Toggle("Do Teleport", spell.onSelfCast.doTeleport);

            if(spell.onSelfCast.doTeleport)
            {
                ++EditorGUI.indentLevel;
                spell.onSelfCast.teleportType = (SpellTeleportType)EditorGUILayout.EnumPopup("Teleport Type", spell.onSelfCast.teleportType);
                --EditorGUI.indentLevel;
            }

            // Spawn
            spell.onSelfCast.doSpawnObject = EditorGUILayout.Toggle("Do Spawn Object", spell.onSelfCast.doSpawnObject);

            if(spell.onSelfCast.doSpawnObject)
            {
                ++EditorGUI.indentLevel;
                spell.onSelfCast.objectToSpawn = (GameObject)EditorGUILayout.ObjectField("Object to Spawn", spell.onSelfCast.objectToSpawn, typeof(GameObject), false);
                spell.onSelfCast.objectToSpawnDestroyTime = EditorGUILayout.FloatField("Destroy Time", spell.onSelfCast.objectToSpawnDestroyTime);
                --EditorGUI.indentLevel;
            }
        }

        // Error Messages
        EditorGUILayout.Space();

        if(spell.spellName.Length == 0)
            EditorGUILayout.HelpBox("Spell needs a name!", MessageType.Error);

        if(!SpellHasProperties())
            EditorGUILayout.HelpBox("Spell currently doesn't do anything!", MessageType.Info);
    }

    // Does the spell do anything?
    bool SpellHasProperties ()
    {
        SpellData spell = (SpellData)target;

        if(spell.castType == SpellCastType.Projectile)
        {
            if(spell.onHitTarget.doDamage)
                return true;
            else if(spell.onHitTarget.doHealCaster)
                return true;
            else if(spell.onHitTarget.doSpawnObject)
                return true;
            else if(spell.onHitTarget.doStun)
                return true;
        }
        else if(spell.castType == SpellCastType.Self)
        {
            if(spell.onSelfCast.doDamageNearby)
                return true;
            else if(spell.onSelfCast.doHealCaster)
                return true;
            else if(spell.onSelfCast.doSpawnObject)
                return true;
            else if(spell.onSelfCast.doStunNearby)
                return true;
            else if(spell.onSelfCast.doTeleport)
                return true;
        }

        return false;
    }
}