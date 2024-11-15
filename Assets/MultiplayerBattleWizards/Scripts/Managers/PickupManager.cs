using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    private Vector3[] spawnPositions;       // Array of all available pickup spawn points.

    public List<GameObject> curPickups = new List<GameObject>();   // List of all current pickups.

    [Header("Values")]
    public int maxPickups;                  // Maximum amount of pickups allowed on the map at once.
    public float startSpawnTime;            // Time that we're going to start spawning pickups.
    public float minSpawnTime;              // Minimum time allowed between pickup spawns.
    public float maxSpawnTime;              // Maximum time allowed between pickup spawns.

    private float nextSpawnTime;            // Next time we're going to spawn a pickup.

    [Header("Pickups")]
    public GameObject[] spellPickups;       // Array of all spell pickups.
    public GameObject[] healthPickups;      // Array of all health pickups.

    [Header("Components")]
    public PhotonView photonView;

    // Instance
    public static PickupManager inst;

    #region Subscribe to Events

    void OnEnable ()
    {
        // Subscribe to events.
        GameManager.inst.onMapLoaded.AddListener(OnMapLoaded);
    }

    void OnDisable ()
    {
        // Un-subscribe from events.
        GameManager.inst.onMapLoaded.RemoveListener(OnMapLoaded);
    }

    #endregion

    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    void Start ()
    {
        // Calculate the initial spawn time.
        nextSpawnTime = GetNextSpawnTime();
    }

    // Called when the map has finished loading in.
    // We want to get all available pickup spawn positions on it.
    void OnMapLoaded ()
    {
        spawnPositions = MapLoader.inst.GetSurfacePositions();
    }

    void Update ()
    {
        // The master client (host) takes care of the timing and spawning of pickups.
        if(!PhotonNetwork.isMasterClient)
            return;

        // If we're ready to spawn the pickup, do that.
        if(nextSpawnTime <= Time.time)
        {
            nextSpawnTime = GetNextSpawnTime();

            // Make sure we're not at our max pickups.
            if(curPickups.Count < maxPickups)
                SpawnPickup();
        }
    }

    // Returns the next time a pickup will be spawned.
    float GetNextSpawnTime ()
    {
        // Is this the first time we're spawning a pickup?
        if(nextSpawnTime == 0.0f)
            return startSpawnTime + Random.Range(minSpawnTime, maxSpawnTime);
        else
            return Time.time + Random.Range(minSpawnTime, maxSpawnTime);
    }

    // Called when the host needs to spawn a new pickup.
    // Calculate a position, pickup, etc.
    // Called locally for the master client (host).
    public void SpawnPickup ()
    {
        // Get the spawn point and type of pickup.
        Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
        int type = Random.Range(0, System.Enum.GetValues(typeof(PickupType)).Length);
        GameObject pickupPrefab = null;
        
        switch((PickupType)type)
        {
            case PickupType.Health: pickupPrefab = healthPickups[Random.Range(0, healthPickups.Length)]; break;
            case PickupType.Spell: pickupPrefab = spellPickups[Random.Range(0, spellPickups.Length)]; break;
        }

        // Spawn it for all players.
        GameObject pickupObj = PhotonNetwork.Instantiate("Pickups/" + pickupPrefab.name, spawnPos, Quaternion.identity, 0);
        curPickups.Add(pickupObj);
    }
}