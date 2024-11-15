using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsScreen : MonoBehaviour
{
    [Header("Maps")]
    public GameObject mapButtonPrefab;
    public RectTransform mapContainer;            // Container for default maps
    public RectTransform customMapContainer;      // Container for custom maps

    private MenuUI menu;

    void Awake()
    {
        menu = GetComponent<MenuUI>();
    }

    // Called when the screen is activated.
    public void OnSetScreen()
    {
        // Optionally refresh the map buttons if needed.
    }

    void Start()
    {
        LoadMaps();
        LoadCustomMaps();
    }

    // Loads in the default map buttons.
    void LoadMaps()
    {
        // Get array of all default maps.
        TextAsset[] mapNames = Resources.LoadAll<TextAsset>("Maps");
        Debug.Log("Default Maps Found: " + mapNames.Length);

        // Create buttons and set text and onClick event for each map.
        foreach (TextAsset map in mapNames)
        {
            Debug.Log("Loading Default Map: " + map.name);
            GameObject mapObj = Instantiate(mapButtonPrefab, mapContainer.transform);
            mapObj.GetComponentInChildren<Text>().text = map.name;

            mapObj.GetComponent<Button>().onClick.AddListener(() => { OnSelectMap(map.name); });
        }
    }

    // Loads in the custom map buttons.
    void LoadCustomMaps()
    {
        // Get array of all custom maps.
        TextAsset[] customMapNames = Resources.LoadAll<TextAsset>("CustomMaps");
        Debug.Log("Custom Maps Found: " + customMapNames.Length);


        // Create buttons and set text and onClick event for each custom map.
        foreach (TextAsset customMap in customMapNames)
        {
            Debug.Log("Loading Custom Map: " + customMap.name);
            GameObject customMapObj = Instantiate(mapButtonPrefab, customMapContainer.transform);
            customMapObj.GetComponentInChildren<Text>().text = customMap.name;

            customMapObj.GetComponent<Button>().onClick.AddListener(() => { OnSelectMap(customMap.name); });
        }
    }

    // Called when a map button is pressed - sends over a map name.
    // Sets the map to the room properties, then go back to the lobby screen.
    public void OnSelectMap(string mapName)
    {
        Debug.Log("Map selected: " + mapName);
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("map", mapName);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }

    // Called when a "_ Kills" button is pressed to change game mode.
    // Sends over the amount of kills needed.
    public void OnGameModeKills(int kills)
    {
        Debug.Log("Game mode set to Kills: " + kills);
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("gamemode", (int)GameModeType.ScoreBased);
        hash.Add("gamemodeprop", kills);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }

    // Called when a "_ Mins" button is pressed to change game mode.
    // Sends over the time in seconds.
    public void OnGameModeTime(int time)
    {
        Debug.Log("Game mode set to Time: " + time);
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("gamemode", (int)GameModeType.TimeBased);
        hash.Add("gamemodeprop", time);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }
}
