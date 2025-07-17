using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Image loadingBar;
    public GameObject processBar;
    public Text progress;
    public Text progressTest;
    

    float m_Max;

    public void InitProgress(float max, string desc)
    {
        m_Max = max;
        processBar.SetActive(true);
        progress.gameObject.SetActive(true);
        progress.text = desc;
        loadingBar.fillAmount = max > 0 ? 0 : 100;
        progressTest.gameObject.SetActive(max > 0);
    }

    public void UpdateProgress(float progress)
    {
        loadingBar.fillAmount = progress / m_Max;
        progressTest.text = string.Format("{0:0}%", loadingBar.fillAmount * 100);
    }
}
