using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileButton : MonoBehaviour
{
    public MobileControl control;       // What the button maps to.
    private Player localPlayer;         // The local player to send commands / events to.

    private bool buttonDown;            // Is the button currently held down?

    #region Subscribe to Events

    void OnEnable ()
    {
        // Subscribe to events.
        PlayerManager.inst.onPlayerInitialized += OnPlayerInitialized;
    }

    void OnDisable ()
    {
        // Un-subscribe from events.
        PlayerManager.inst.onPlayerInitialized -= OnPlayerInitialized;
    }

    #endregion

    // Called when a player has been initialized. Check if it's the local player.
    void OnPlayerInitialized (int playerID)
    {
        NetworkPlayer player = PlayerManager.inst.GetPlayer(playerID);

        // If it's the local player, get it.
        if(player.photonPlayer.IsLocal)
            localPlayer = player.player;
    }

    void Update ()
    {
        if(buttonDown)
        {
            if(control == MobileControl.MoveLeft)
                localPlayer.input.mobileMovementAxis = -1.0f;
            else if(control == MobileControl.MoveRight)
                localPlayer.input.mobileMovementAxis = 1.0f;

            if(control == MobileControl.Jump)
                localPlayer.input.onJumpHold.Invoke();
        }
    }

    // Called when the button is pressed down.
    public void OnButtonDown ()
    {
        buttonDown = true;

        if(control == MobileControl.Attack)
            localPlayer.input.onAttackDown.Invoke();
        else if(control == MobileControl.Jump)
            localPlayer.input.onJumpDown.Invoke();
    }

    // Called when the button is released.
    public void OnButtonUp ()
    {
        buttonDown = false;

        if(control == MobileControl.Jump)
            localPlayer.input.onJumpUp.Invoke();
    }
}

// What the button maps to.
public enum MobileControl
{
    MoveLeft,
    MoveRight,
    Jump,
    Attack
}