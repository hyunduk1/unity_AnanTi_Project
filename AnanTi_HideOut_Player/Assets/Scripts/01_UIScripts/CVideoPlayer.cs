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

    ErrorCode error;

    public bool bAutoPlay; 
    void Start()
    {
        _VideoWindow.Events.AddListener(OnVideoEvent);
    }

    public void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.F9))
        {
            StopVideo();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            PlayVideo(true);
        }
    }

    public void InitiallizeContents(string strFolder, string strMovieName, bool bIsAutoStart = true, bool bLoop = true, bool bMode = true)
    {
        _VideoWindow.m_AutoStart = bIsAutoStart;
        _VideoWindow.m_Loop = bLoop;
        _VideoWindow.m_VideoPath = strMovieName;

        _VideoWindow.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, strFolder + strMovieName, bIsAutoStart);
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
                Debug.Log(et);
                break;
        }
    }



    public void GetSoundVolum(float Volume)
    {
        _VideoWindow.Control.SetVolume(Volume);
    }
    public void GetDurationTime()
    {
        _VideoWindow.Info.GetDurationMs();
    }
    public float GetCurrentTime()
    {
        return _VideoWindow.Control.GetCurrentTimeMs();
    }

    public void PlayVideo(bool IsRestart)
    {
        _VideoWindow.Rewind(IsRestart);
        _VideoWindow.Play();
    }
    public void StopVideo()
    {
        _VideoWindow.Pause();
    }
}