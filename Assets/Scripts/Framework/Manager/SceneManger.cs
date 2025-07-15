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
    /// �����л��ص�
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
    /// �����
    /// </summary>
    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }


    /// <summary>
    /// ���Ӽ��س���
    /// </summary>
    public void LoadScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// �л����س���
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
        // �첽���س���
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        // ����������(���������Ϊtrue����������ز�����)
        async.allowSceneActivation = true;
        yield return async;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        // �ѹ��س����߼���GameObject���ص�Ŀ�곡��
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
            Debug.LogError("����δ����");
            yield break;
        }
        // �첽ж�س���
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
    /// ��鳡���Ƿ��Ѿ�����
    /// </summary>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
