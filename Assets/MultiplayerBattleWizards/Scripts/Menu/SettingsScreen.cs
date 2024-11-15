using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    public InputField playerNameInput;
    public Slider volumeSlider;

    private MenuUI menu;

    void Awake ()
    {
        menu = GetComponent<MenuUI>();
    }

    // Called when the screen is activated.
    public void OnSetScreen ()
    {
        playerNameInput.text = PhotonNetwork.playerName;

        // Do we have a 'Volume' player pref?
        if(PlayerPrefs.HasKey("Volume"))
        {
            // Set the slider to be the volume value.
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            // Set and create the volume player pref.
            volumeSlider.value = AudioListener.volume;
            PlayerPrefs.SetFloat("Volume", AudioListener.volume);
        }
    }

    // Called when the player changes the value of the player name input.
    public void UpdatePlayerName ()
    {
        // Name has to be something.
        if(playerNameInput.text.Length > 0)
        {
            PhotonNetwork.playerName = playerNameInput.text;
            PlayerPrefs.SetString("PlayerName", playerNameInput.text);

            // If we're in a lobby, let's refresh everyone's lobby UI.
            if(PhotonNetwork.inRoom)
                menu.photonView.RPC("UpdateUI", PhotonTargets.All);
        }
    }

    // Called when the volume slider is updated.
    public void UpdateVolume ()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        AudioListener.volume = volumeSlider.value;
    }
}