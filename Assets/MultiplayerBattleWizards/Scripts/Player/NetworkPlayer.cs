using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains info for each player.
/// </summary>
public class NetworkPlayer
{
    public int networkID;               // Unique identifier.
    public Player player;               // In-game player class reference.
    public PhotonPlayer photonPlayer;   // PhotonPlayer object relating to the player.
}