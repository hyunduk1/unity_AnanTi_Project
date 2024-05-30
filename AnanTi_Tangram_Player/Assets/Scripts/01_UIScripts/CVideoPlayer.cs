using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;


public class CVideoPlayer : MonoBehaviour
{
    public MediaPlayer _VideoWindow;

    public string nextPanelName;

    public string _VideoName;
    public bool _bLoop = false;
    public bool _bAuto = false;

    public bool bIsfinished;

    public bool bAutoPlay;

    public int m_nCurrentAutoPlay = 0;
    void Start()
    {
        _VideoWindow.Events.AddListener(OnVideoEvent);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            StopVideo();
        }

        if(Input.GetKeyDown(KeyCode.F12))
        {
            PlayVideo();
        }
    }
    public void InitiallizeContents(string strFolder, string strMovieName, bool bIsAutoStart = true, bool bLoop = true, bool bMode = true)
    {
        _VideoWindow.m_AutoStart = bIsAutoStart;
        _VideoWindow.m_Loop = bLoop;
        _VideoWindow.m_VideoPath = strMovieName;

        _VideoWindow.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, strFolder + strMovieName, bIsAutoStart);
        _VideoWindow.Play();
    }

    
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                Debug.Log(et);
                break;

            case MediaPlayerEvent.EventType.Started:
                Debug.Log(et);
                break;

            case MediaPlayerEvent.EventType.FirstFrameReady:
                Debug.Log(et);
                break;

            case MediaPlayerEvent.EventType.FinishedPlaying:

                if (CNetWorkMng.Instance._IsContent00 == true)
                {
                    CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_00_0, false);
                    CNetWorkMng.Instance._IsContent00 = false;
                }

                if (CNetWorkMng.Instance._IsContent01 == true)
                {
                    CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_01_1, false);
                    CNetWorkMng.Instance._IsContent01 = false;
                }

                if (CNetWorkMng.Instance._IsContent02 == true)
                {
                    CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_02_2, false);
                    CNetWorkMng.Instance._IsContent02 = false;
                }
                    

                if (CNetWorkMng.Instance._IsContent03 == true)
                {
                    CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_01_1, false);
                    CNetWorkMng.Instance._IsContent03 = false;
                }
                    

                if(CNetWorkMng.Instance._IsLooping == true)
                {
                    CPanelMng.Instance._nCurrentAutoPlay++;
                    Debug.Log(CPanelMng.Instance._nCurrentAutoPlay);
                    CPanelMng.Instance.insertdisplay_moviepanel("A2_MT-3-" + CPanelMng.Instance._nCurrentAutoPlay +".mp4", false);

                    if (CPanelMng.Instance._nCurrentAutoPlay == 7)
                    {
                        CPanelMng.Instance._nCurrentAutoPlay = 0;
                    }

                }
                
                Debug.Log(et);
                break;
        }
    }




    public void GetDurationTime()
    {
        _VideoWindow.Info.GetDurationMs();
    }
    public float GetCurrentTime()
    {
        return _VideoWindow.Control.GetCurrentTimeMs();
    }

    public void PlayVideo()
    {
        _VideoWindow.Rewind(false);
    }
    public void StopVideo()
    {
        _VideoWindow.Pause();
    }
}