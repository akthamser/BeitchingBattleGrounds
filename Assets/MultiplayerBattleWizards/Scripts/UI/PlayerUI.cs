using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's UI health and score.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    public Image[] hearts;      // Array of all heart images.
    public Text scoreText;      // Text showing the player's score.
    public Image spellIcon;     // Player's current spell icon.
    public Image spellCooldown; // Spell cooldown image.
    public Color playerColor;   // Color of the player's UI elements.

    public int id;              // ID relating to the player.
    private Player player;      // Connected player.

    private bool spellCooldownActive;

    #region Subscribing to Events

    void OnEnable ()
    {
        // Subscribing to events.
        PlayerManager.inst.onPlayerInitialized += OnPlayerInitialized;
    }

    void OnDisable ()
    {
        // Un-subscribing from events.
        PlayerManager.inst.onPlayerInitialized -= OnPlayerInitialized;
    }

    #endregion

    void Update ()
    {
        // If the player's spell is currently cooling down - show the cooldown bar going down.
        if(spellCooldownActive)
        {
            spellCooldown.fillAmount = 1.0f - player.attack.curSpell.GetCooldownProgress();

            if(spellCooldown.fillAmount == 0.0f)
                spellCooldownActive = false;
        }
    }

    // Subscribed to the PlayerManager.onPlayerInitialized event.
    // Connects the player UI to the player.
    void OnPlayerInitialized (int playerID)
    {
        if(playerID == id)
        {
            player = PlayerManager.inst.GetPlayer(playerID).player;
            player.ui = this;

            scoreText.text = "0";
        }
    }

    // Called when the player takes damage or gains health.
    // Updates the heart images to represent their health.
    [PunRPC]
    public void UpdateHealth ()
    {
        int prevHp = hearts.Length;
        int newHp = player.curHp;

        // If there aren't enough hearts, log an error then return.
        if(player.maxHp > hearts.Length)
        {
            Debug.LogError("Player's max hp exceeds the amount of UI hearts they have! Add more.");
            return;
        }
        
        // Enable / disable hearts so they reflect the player's current health.
        for(int x = 0; x < hearts.Length; ++x)
        {
            hearts[x].gameObject.SetActive(x < player.curHp ? true : false);
        }

        // If we're losing health, apply some shake.
        if(newHp < prevHp)           
            Shake.inst.ShakeObject(hearts[0].transform.parent.gameObject, 0.3f, 5.0f, 150.0f);
    }

    // Called when the player's score (kills) changes.
    // Updates the score text to reflect the player's current score.
    [PunRPC]
    public void UpdateScore ()
    {
        scoreText.text = player.score.ToString();
    }

    // Called when the player's cur spell changes.
    // Updates the spell icon.
    [PunRPC]
    public void SetSpellIcon (SpellData spellData)
    {
        spellIcon.sprite = spellData.icon;
    }

    // Called when the player casts a spell and is now cooling down.
    public void SetCooldown ()
    {
        spellCooldownActive = true;
    }
}