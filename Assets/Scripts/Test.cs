using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    IEnumerator Start()
    {
        // 加载prefab的ab包
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;
        // 加载prefab使用到的资源的ab包
        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/res/login/button_150.png.ab");
        yield return request1;
        // 加载prefab使用到的资源的ab包
        AssetBundleCreateRequest request2 = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/res/login/background.png.ab");
        yield return request2;
        // 加载prefab
        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync("Assets/BuildResources/UI/Prefabs/TestUI.prefab");
        yield return bundleRequest;

        GameObject go = Instantiate(bundleRequest.asset) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
