using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the camera movement in-game.
/// </summary>
public class CameraController : MonoBehaviour
{
    public bool doTrack;                // Does the camera track the players?
    public float trackSmooth;           // Smoothness applied to the camera track.

    [Range(0.0f, 1.0f)]
    public float influence;             // How much does the camera track the players?

    public float maxDistFromCenter;     // Max distance the camera can move from the center.

    private Vector3 originPos;          // Starting position for the camera.

    void Start ()
    {
        originPos = transform.position;
    }

    void LateUpdate ()
    {
        // Only track if we're playing the game.
        if(doTrack && GameManager.inst.gameState != GameState.Initiation)
        {
            Vector3 total = Vector3.zero;

            // Loop through each player.
            foreach(NetworkPlayer player in PlayerManager.inst.players)
            {
                // If the player doesn't exist, don't track them.
                if(!player.player)
                    continue;

                // If the player is dead, don't track them.
                if(player.player.curState == PlayerState.Dead)
                    continue;

                total += player.player.transform.position;
            }

            // Get the average position for all players.
            total /= PlayerManager.inst.players.Length;
            total.z = originPos.z;

            Vector3 dir = total.normalized;

            // Camera can't go past the max distance.
            if(total.magnitude > maxDistFromCenter)
                total = dir * maxDistFromCenter;

            // Move the camera smoothly to this new position.
            if(total.magnitude != 0)
                transform.position = Vector3.Lerp(transform.position, new Vector3(total.x, total.y, originPos.z) * influence, trackSmooth * Time.deltaTime);
        }
    }
}