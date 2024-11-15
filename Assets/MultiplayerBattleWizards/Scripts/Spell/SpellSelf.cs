using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spell casted by the player unto themself.
/// </summary>
public class SpellSelf : Spell
{
    // Called when projectile is created.
    // Sets up some initial data.
    public void Initiate (int casterID, bool isMine, SpellData spellData)
    {
        this.casterID = casterID;
        this.isMine = isMine;
        this.spellData = spellData;

        Cast();
    }
    
    // Casts the spell.
    void Cast ()
    {
        SpellOnSelfCast cast = spellData.onSelfCast;

        // Does the spell heal the caster?
        if(cast.doHealCaster)
            PlayerManager.inst.GetPlayer(casterID).player.Heal(cast.healthToCaster);

        // Does the spell spawn an object?
        if(cast.doSpawnObject)
        {
            GameObject obj = Instantiate(cast.objectToSpawn, transform.position, Quaternion.identity);

            // If the destroy time is greater than 0, set it to be destroyed in that time.
            if(cast.objectToSpawnDestroyTime > 0.0f)
                Destroy(obj, cast.objectToSpawnDestroyTime);
        }

        if(isMine)
        {
            // Does the spell stun nearby players?
            if(cast.doStunNearby)
            {
                Player[] playersToStun = GetNearbyPlayers(cast.nearbyStunRange);

                foreach(Player p in playersToStun)
                    p.photonView.RPC("Stun", PhotonTargets.All, cast.nearbyStunDuration);
            }

            // Does the spell deal damage to nearby players?
            if(cast.doDamageNearby)
            {
                Player[] playersToDamage = GetNearbyPlayers(cast.nearbyDamageRange);

                // Create the hit data.
                SpellHitData hitData = new SpellHitData();

                hitData.attackerID = casterID;
                hitData.spellName = spellData.spellName;

                byte[] rawData = Serializer.Serialize(hitData);

                foreach(Player p in playersToDamage)
                    p.photonView.RPC("Hit", PhotonTargets.All, rawData);
            }

            // Does the spell teleport the caster?
            if(cast.doTeleport)
            {
                TeleportPlayer(cast.teleportType);
            }
        }

        // We've finished casting the spell, so let's destroy this object.
        Destroy(gameObject);
    }

    // Returns an array of players within the range of the caster.
    Player[] GetNearbyPlayers (float range)
    {
        List<Player> players = new List<Player>();
        Player caster = PlayerManager.inst.GetPlayer(casterID).player;

        // Loop through all players and add them if they're in range.
        foreach(NetworkPlayer p in PlayerManager.inst.players)
        {
            // Skip if this is us.
            if(p.player == caster) continue;

            if(Vector3.Distance(caster.transform.position, p.player.transform.position) <= range)
                players.Add(p.player);
        }

        return players.ToArray();
    }

    // Teleport the player based on the teleport type.
    void TeleportPlayer (SpellTeleportType tpType)
    {
        Vector3 tpPos = Vector3.zero;

        switch(tpType)
        {
            // Teleport to the furthest player in the game.
            case SpellTeleportType.FurthestPlayer:
            {
                    // Get the other players.
                    NetworkPlayer[] others = PlayerManager.inst.GetOtherPlayers(casterID);
                    Player caster = PlayerManager.inst.GetPlayer(casterID).player;

                    float furthestDist = 0;

                    // Find the one furthest away.
                    foreach(NetworkPlayer p in others)
                    {
                        float dist = Vector3.Distance(caster.transform.position, p.player.transform.position);

                        if(dist > furthestDist)
                        {
                            tpPos = p.player.transform.position;
                            furthestDist = dist;
                        }
                    }

                    break;
            }
            // Teleport to the nearest player in the game.
            case SpellTeleportType.NearestPlayer:
            {
                    // Get the other players.
                    NetworkPlayer[] others = PlayerManager.inst.GetOtherPlayers(casterID);
                    Player caster = PlayerManager.inst.GetPlayer(casterID).player;

                    float nearestDist = 0;

                    // Find the one furthest away.
                    foreach(NetworkPlayer p in others)
                    {
                        float dist = Vector3.Distance(caster.transform.position, p.player.transform.position);

                        if(dist < nearestDist)
                        {
                            tpPos = p.player.transform.position;
                            nearestDist = dist;
                        }
                    }

                    break;
            }
            // Teleport to a random player.
            case SpellTeleportType.RandomPlayer:
            {
                NetworkPlayer[] others = PlayerManager.inst.GetOtherPlayers(casterID);
                tpPos = others[Random.Range(0, others.Length)].player.transform.position;
                break;
            }
            // Teleport to a random surface position.
            case SpellTeleportType.RandomPosition:
            {
                Vector3[] surfacePositions = MapLoader.inst.GetSurfacePositions();
                tpPos = surfacePositions[Random.Range(0, surfacePositions.Length)];
                break;
            }
            // Teleport to the top of the map.
            case SpellTeleportType.TopOfMap:
            {
                Vector3[] surfacePositions = MapLoader.inst.GetSurfacePositions();

                foreach(Vector3 pos in surfacePositions)
                    if(pos.y > tpPos.y)
                        tpPos = pos;

                break;
            }
        }

        if(tpPos != Vector3.zero)
        {
            PlayerManager.inst.GetPlayer(casterID).player.photonView.RPC("Teleport", PhotonTargets.All, tpPos);
        }
    }
}