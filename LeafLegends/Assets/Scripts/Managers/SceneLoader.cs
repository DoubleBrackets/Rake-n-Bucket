using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private Material material;

    private string currentLevel;

    private void Awake()
    {
#if UNITY_EDITOR
        currentLevel = SceneManager.GetSceneAt(1).name;
#endif
    }

    public async UniTask LoadLevel(string sceneName)
    {
        if (SceneManager.GetSceneByName(currentLevel).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(currentLevel);
        }

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
}