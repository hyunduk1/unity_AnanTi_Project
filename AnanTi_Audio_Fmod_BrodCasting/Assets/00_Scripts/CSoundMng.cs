using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;


public class CSoundMng : MonoBehaviour
{
    private static CSoundMng _instance;
    public static CSoundMng Instance { get { return _instance; } }

    public AudioSource[] _AudioSources;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            //DontDestroyOnLoad(gameObject);
            if (Application.isPlaying) 
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            StartCoroutine(LoadAudioClipFromFile());
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    IEnumerator LoadAudioClipFromFile()
    {
        for(int i = 0; i < 2; i ++)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(CConfigMng.Instance._strAudioFolderName + CConfigMng.Instance._strAudio_L_FileName + i.ToString(), AudioType.WAV))
            {
                yield return www.SendWebRequest();
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                _AudioSources[i].clip = audioClip;
                _AudioSources[i].panStereo = CConfigMng.Instance._nAudioL;
                _AudioSources[i].Play();
                _AudioSources[i].loop = CConfigMng.Instance._bAudioLoop;
            }
        }
    }
}
