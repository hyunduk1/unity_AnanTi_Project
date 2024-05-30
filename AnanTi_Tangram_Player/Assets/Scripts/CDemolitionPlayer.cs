using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DemolitionStudios.DemolitionMedia;

public class CDemolitionPlayer : MonoBehaviour
{

    public Media m_MediaPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if (m_MediaPlayer != null)
        {
            m_MediaPlayer.Events.AddListener(OnMediaPlayerEvent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMediaPlayerEvent(Media source, MediaEvent.Type type, MediaError error)
    {


        switch (type)
        {
            case MediaEvent.Type.Closed: break;
            case MediaEvent.Type.OpeningStarted: break;
            case MediaEvent.Type.PreloadingToMemoryStarted: break;
            case MediaEvent.Type.PreloadingToMemoryFinished: break;
            case MediaEvent.Type.Opened: break;
            case MediaEvent.Type.OpenFailed: break;
            case MediaEvent.Type.VideoRenderTextureCreated: break;
            case MediaEvent.Type.PlaybackStarted: break;
            case MediaEvent.Type.PlaybackStopped: break;
            case MediaEvent.Type.PlaybackEndReached: break;
            case MediaEvent.Type.PlaybackSuspended: break;
            case MediaEvent.Type.PlaybackResumed: break;
            case MediaEvent.Type.PlaybackNewLoop: break;
            case MediaEvent.Type.PlaybackErrorOccured: break;
        }
    }

     public void MediaPlayer(string FolderName , string FileName)
    {
        m_MediaPlayer.urlType = Media.UrlType.RelativeToDataPath;
        m_MediaPlayer.mediaUrl = FolderName + FileName;
        m_MediaPlayer.Loops = 1;
        m_MediaPlayer.openOnStart = true;
        m_MediaPlayer.playOnOpen = true;
        m_MediaPlayer.preloadToMemory = true;
    }
}
