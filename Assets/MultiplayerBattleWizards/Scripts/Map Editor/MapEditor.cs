#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

public class MapEditor : MonoBehaviour
{
    public string resourceFolderPath;       // Path to the "Resources" folder.
    public string customMapsFolderPath;     // Path to the "Resources/CustomMaps" folder for player-made maps.

    public List<Sprite> allSprites = new List<Sprite>();        // List of all the available sprites to use (for loading in maps).

    [HideInInspector]
    public List<GameObject> tiles = new List<GameObject>();     // List of all tiles currently in our map.

    public GameObject tilePrefab;           // Prefab we instantiate when we place a tile.
    public Sprite defaultTile;              // The default sprite at runtime.
    public Sprite spawnPointTile;           // Sprite to represent the player spawn points.

    private Sprite curTile;                 // Our currently selected tile sprite.

    private int minSpawnPoints = 4;         // Minimum amount of spawn points required.

    [Header("UI")]
    public InputField mapNameInput;         // Input field to write name of map to save or load.
    public Toggle isCustomMapToggle;        // Toggle to choose if the map is a custom map.
    public Text spawnPointsText;            // Text displaying current amount of spawn points.
    public Image currSelectImage;           // Image showing our current tile.

    void Awake()
    {
        SetCurTile(defaultTile);

        // Set default paths if they are not already assigned in the Inspector.
        if (string.IsNullOrEmpty(resourceFolderPath))
            resourceFolderPath = "Assets/Resources";

        if (string.IsNullOrEmpty(customMapsFolderPath))
            customMapsFolderPath = "Assets/MultiplayerBattleWizards/Resources/CustomMaps";
    }

    void Update()
    {
        // Place tile.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlaceTile(new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0));
        }
        // Remove tile.
        else if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryRemoveTile(new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0));
        }
    }

    void PlaceTile(Vector3 pos)
    {
        GameObject existingTile = TileAtPosition(pos);

        if (existingTile != null)
        {
            if (!existingTile.name.Equals(curTile.name))
                RemoveTile(existingTile);
            else
                return;
        }

        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        tile.name = curTile.name;
        tile.GetComponent<SpriteRenderer>().sprite = curTile;
        tiles.Add(tile);

        if (tile.name == spawnPointTile.name)
            spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";
    }

    void TryRemoveTile(Vector3 pos)
    {
        GameObject existingTile = TileAtPosition(pos);

        if (existingTile == null)
            return;

        if (existingTile.name == spawnPointTile.name)
            spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";

        RemoveTile(existingTile);
    }

    void RemoveTile(GameObject tile)
    {
        tiles.Remove(tile);
        Destroy(tile);
    }

    GameObject TileAtPosition(Vector3 pos)
    {
        return tiles.Find(x => x.transform.position == pos);
    }

    public void SetCurTile(Sprite tile)
    {
        curTile = tile;
        currSelectImage.sprite = tile;
    }

    int AmountOfSpawnPoints()
    {
        return tiles.FindAll(x => x.name == spawnPointTile.name).Count;
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(resourceFolderPath))
        {
            Debug.LogError("The resource folder path is not set. Please provide a valid path.");
            return;
        }
        SaveMap(resourceFolderPath + "/Maps");
    }

    public void SaveCustom()
    {
        if (string.IsNullOrEmpty(customMapsFolderPath))
        {
            Debug.LogError("The custom maps folder path is not set. Please provide a valid path.");
            return;
        }
        SaveMap(customMapsFolderPath);
    }

    private void SaveMap(string folderPath)
    {
        if (!CanSave())
            return;

        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("The folder path is not valid.");
            return;
        }

        MapData data = new MapData(mapNameInput.text);
        List<GameObject> spawnPointObjects = tiles.FindAll(x => x.name == spawnPointTile.name);
        data.spawnPoints = new Vector3[spawnPointObjects.Count];

        for (int x = 0; x < spawnPointObjects.Count; ++x)
            data.spawnPoints[x] = spawnPointObjects[x].transform.position;

        List<MapTileData> tileData = new List<MapTileData>();

        for (int x = 0; x < tiles.Count; ++x)
        {
            if (tiles[x].name != spawnPointTile.name)
                tileData.Add(new MapTileData(tiles[x].name, tiles[x].transform.position));
        }

        data.tiles = tileData.ToArray();

        Directory.CreateDirectory(folderPath);
        string path = folderPath + "/" + mapNameInput.text + ".json";
        string rawData = JsonUtility.ToJson(data);

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(rawData);
            }
        }

        UnityEditor.AssetDatabase.Refresh();

        Debug.Log("<b>" + mapNameInput.text + "</b> saved to: " + path);
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(resourceFolderPath))
        {
            Debug.LogError("The resource folder path is not set. Please provide a valid path.");
            return;
        }
        LoadMap(resourceFolderPath + "/Maps");
    }

    public void LoadCustom()
    {
        if (string.IsNullOrEmpty(customMapsFolderPath))
        {
            Debug.LogError("The custom maps folder path is not set. Please provide a valid path.");
            return;
        }
        LoadMap(customMapsFolderPath);
    }

    private void LoadMap(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("The folder path is not valid.");
            return;
        }

        TextAsset mapFile = (TextAsset)AssetDatabase.LoadAssetAtPath(folderPath + "/" + mapNameInput.text + ".json", typeof(TextAsset));

        if (mapFile == null)
        {
            Debug.LogError("Map with the name of '" + mapNameInput.text + "' not found.");
            return;
        }

        for (int x = 0; x < tiles.Count; ++x)
            Destroy(tiles[x]);

        tiles.Clear();

        MapData data = JsonUtility.FromJson<MapData>(mapFile.text);

        foreach (MapTileData tile in data.tiles)
        {
            GameObject tileObj = Instantiate(tilePrefab, tile.position, Quaternion.identity, transform);
            tileObj.name = tile.tileName;
            tileObj.GetComponent<SpriteRenderer>().sprite = allSprites.Find(x => x.name == tile.tileName);

            tiles.Add(tileObj);
        }

        foreach (Vector3 sp in data.spawnPoints)
        {
            GameObject tileObj = Instantiate(tilePrefab, sp, Quaternion.identity, transform);
            tileObj.name = spawnPointTile.name;
            tileObj.GetComponent<SpriteRenderer>().sprite = spawnPointTile;

            tiles.Add(tileObj);
        }

        spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";
    }

    bool CanSave()
    {
        bool can = false;

        List<GameObject> spawnPoints = tiles.FindAll(x => x.name == spawnPointTile.name);

        if (spawnPoints.Count >= minSpawnPoints)
            can = true;
        else
        {
            Debug.LogError("A map requires a minimum of <b>" + minSpawnPoints + "</b> spawn points");
            return false;
        }

        if (mapNameInput.text.Length > 0)
            can = true;
        else
        {
            Debug.LogError("The map needs a name.", mapNameInput);
            return false;
        }

        return can;
    }

    public void OnFileButton()
    {
        string folderPath = isCustomMapToggle.isOn ? customMapsFolderPath : resourceFolderPath + "/Maps";
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("The folder path is not valid.");
            return;
        }

        Object obj = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
}
#endif
