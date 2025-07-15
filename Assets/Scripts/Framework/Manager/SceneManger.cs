using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManger : MonoBehaviour
{
    private string m_LogicName = "[SceneLogic]";

    private void Awake()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    /// <summary>
    /// 场景切换回调
    /// </summary>
    /// <param name="hideScene"></param>
    /// <param name="activeScene"></param>
    private void OnActiveSceneChanged(Scene hideScene, Scene activeScene)
    {
        if (!hideScene.isLoaded || !activeScene.isLoaded)
            return;

        SceneLogic logic1 = GetSceneLogic(hideScene);
        SceneLogic logic2 = GetSceneLogic(activeScene);

        logic1?.OnInActive();
        logic2?.OnActive();
    }

    /// <summary>
    /// 激活场景
    /// </summary>
    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }


    /// <summary>
    /// 叠加加载场景
    /// </summary>
    public void LoadScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// 切换加载场景
    /// </summary>
    public void ChangeScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }


    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        if (IsLoadedScene(sceneName))
            yield break;
        // 异步加载场景
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        // 允许场景激活(如果不设置为true，场景会加载不完整)
        async.allowSceneActivation = true;
        yield return async;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        // 把挂载场景逻辑的GameObject挂载到目标场景
        GameObject go = new GameObject(m_LogicName);
        SceneManager.MoveGameObjectToScene(go, scene);

        SceneLogic logic = go.AddComponent<SceneLogic>();
        logic.SceneName = sceneName;
        logic.Init(luaName);
        logic.OnEnter();
    }

    IEnumerator UnloadSceneOptions(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogError("场景未加载");
            yield break;
        }
        // 异步卸载场景
        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();
        AsyncOperation async = SceneManager.UnloadSceneAsync(scene);
        yield return async;
    }

    private SceneLogic GetSceneLogic(Scene scene)
    {
        GameObject[] gameObjects = scene.GetRootGameObjects();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.name.CompareTo(m_LogicName) == 0)
            {
                SceneLogic logic = gameObject.GetComponent<SceneLogic>();
                return logic;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查场景是否已经加载
    /// </summary>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
