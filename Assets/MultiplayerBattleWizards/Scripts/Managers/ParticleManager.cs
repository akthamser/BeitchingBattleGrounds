using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("Player")]
    public ParticleSystem playerSpawn;
    public ParticleSystem playerDeath;
    public ParticleSystem playerHeal;
    public ParticleSystem playerTeleport;

    [Header("Environment")]
    public ParticleSystem pickupSpawn;

    // Instance
    public static ParticleManager inst;

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

    // Instantiates a given particle at a certain position.
    // Destroys the particle after it's been played.
    public void Create (ParticleSystem particle, Vector3 position)
    {
        GameObject particleObj = Instantiate(particle.gameObject, position, Quaternion.identity);
        Destroy(particleObj, particle.main.duration);
    }

    // Instantiates a given particle at a certain position inside of a parent.
    // Destroys the particle after it's been played.
    public void Create (ParticleSystem particle, Vector3 position, Transform parent)
    {
        GameObject particleObj = Instantiate(particle.gameObject, position, Quaternion.identity, parent);
        Destroy(particleObj, particle.main.duration);
    }
}