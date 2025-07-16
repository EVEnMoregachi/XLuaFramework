using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource m_MusicAudio;
    AudioSource m_SoundAudio;
    // ������Χ 1-10000
    const int MaxVolume = 10000;

    private float SoundVolume
    {
        get { return PlayerPrefs.GetFloat("SoundVolume", 10000.0f); }
        set
        {
            m_SoundAudio.volume = value;
            PlayerPrefs.SetFloat("SoundVolume", value);
        }
    }

    private float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("MusicVolume", 10000.0f); }
        set
        {
            m_MusicAudio.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    private void Awake()
    {
        m_MusicAudio = this.gameObject.AddComponent<AudioSource>();
        m_MusicAudio.playOnAwake = false;
        m_MusicAudio.loop = true;

        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void PlayMusic(string name)
    { 
        string oldName = "";
        if (m_MusicAudio.clip != null)
            oldName = m_MusicAudio.clip.name;
        // ��ͬ�����ֲ��ظ�����
        if (oldName == name)
            return;

        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            Debug.Log("�������֣�" + name);
            if (obj == null)
            {
                Debug.LogError("��������ʧ�ܣ�" + name);
                return;
            }
            m_MusicAudio.clip = obj as AudioClip;
            m_MusicAudio.Play();
        });
    }

    /// <summary>
    /// ��ͣ����
    /// </summary>
    public void PauseMusic()
    {
        m_MusicAudio.Pause();
    }

    /// <summary>
    /// �ָ�����
    /// </summary>
    public void UnPauseMusic()
    {
        m_MusicAudio.UnPause();
    }

    /// <summary>
    /// ֹͣ����
    /// </summary>
    public void StopMusic()
    {
        m_MusicAudio.Stop();
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    public void PlaySound(string name)
    {
        Manager.Resource.LoadSound(name, (UnityEngine.Object obj) =>
        {
            m_SoundAudio.PlayOneShot(obj as AudioClip);
        });
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        this.MusicVolume = SetVolume(volume);
    }

    /// <summary>
    /// ������Ч����
    /// </summary>
    public void SetSoundVolume(float volume)
    {
        this.SoundVolume = SetVolume(volume);
    }

    /// <summary>
    /// ��������
    /// </summary>
    private float SetVolume(float value)
    {
        value = Mathf.Clamp(value, 1f, 10000f);
        float dB = Mathf.Log10(value / 10000f) * 20f; // ���� dB
        return Mathf.Pow(10f, dB / 20f); // dB �� ��������
    }
}
