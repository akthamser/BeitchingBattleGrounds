using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectScreen : MonoBehaviour
{
    public InputField roomCodeInput;    // Input field for the room code.
    public Text errorText;              // Red text which displays connection errors.

    private MenuUI menu;

    void Awake ()
    {
        menu = GetComponent<MenuUI>();
    }

    void Start ()
    {
        roomCodeInput.characterLimit = NetworkManager.inst.roomCodeLength;
    }

    // Called when the screen is activated.
    public void OnSetScreen ()
    {

    }

    // Called when the input field has been changed.
    // Used to capitalise the input as that is how the rooms are named.
    public void OnRoomCodeInputChanged ()
    {
        roomCodeInput.text = roomCodeInput.text.ToUpper();
    }

    // Called when the "Connect" button gets pressed.
    // Attempts to join the room with the entered room code.
    public void OnConnectButton ()
    {
        if(roomCodeInput.text.Length == NetworkManager.inst.roomCodeLength)
            NetworkManager.inst.AttemptJoinRoom(roomCodeInput.text);
        else
            SetErrorText("Room code needs to be " + NetworkManager.inst.roomCodeLength + " characters long!");
    }

    // Called after the player presses the "Connect" button and connects to the room.
    // Switches the screen to the lobby.
    public void OnJoinedRoom ()
    {
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }

    // Called when the player fails to connect to a room.
    public void OnPhotonJoinRoomFailed (object[] codeAndMsg)
    {
        SetErrorText("Failed to connect to game!");
    }

    // Sets the error text to display.
    // Called when an error/problem occurs when the "Connect" button is pressed.
    void SetErrorText (string textToDisplay)
    {
        errorText.text = textToDisplay;

        CancelInvoke("DisableErrorText");
        Invoke("DisableErrorText", 3.0f);
    }

    // Disables the error text after a certain amount of time.
    void DisableErrorText ()
    {
        errorText.text = "";
    }
}