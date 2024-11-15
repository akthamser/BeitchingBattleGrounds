using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenLoader: MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName; // The name of the scene to load.

    // Call this method to load the specified scene.
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Check if the scene is in the build settings.
            if (IsSceneInBuildSettings(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                Debug.Log("Loading scene: " + sceneName);
            }
            else
            {
                Debug.LogError("Scene '" + sceneName + "' is not found in the build settings.");
            }
        }
        else
        {
            Debug.LogError("Scene name is not set.");
        }
    }

    // Checks if a scene is listed in the build settings.
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (scene.Equals(sceneName))
            {
                return true;
            }
        }
        return false;
    }
}
