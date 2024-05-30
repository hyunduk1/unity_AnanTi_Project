using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIPanel : MonoBehaviour
{
    private CanvasGroup m_CanvasGroup;
    public bool _bAnalyzingPanel=false;
    public bool _bCheckAutoIdleMode = true;
    //private GameObject _EventMotion;

    public GameObject[] _PositionObjs;

    public CVideoPlayer[] _VideoPlayer;
    // Start is called before the first frame update

    private void Awake()
    {
       
        
    }
    void Start()
    {
        m_CanvasGroup = transform.GetComponent<CanvasGroup>();
        
        for (int i = 0; i < _PositionObjs.Length; i++)
        {
            _PositionObjs[i].GetComponent<RectTransform>();
            if (_PositionObjs[i] != null)
            {
                SaveData loadData = SaveSystem.Load("SAVE");
                SavePosition loadPos = SaveSystem.LoadScale("SAVE_SCALE");
                _PositionObjs[i].transform.localPosition = loadData.Content_Pos[i];
                _PositionObjs[i].transform.localScale = loadPos.Content_Scale[i];
            }
        }
    }


    void Update()
    {
        MoveVideoPanel();
    }
    private void MoveVideoPanel()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[0].transform.position += new Vector3(CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[0].transform.position += new Vector3(-CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[0].transform.position += new Vector3(0.0f, CConfigMng.Instance._nMovingSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[0].transform.position += new Vector3(0.0f, -CConfigMng.Instance._nMovingSpeed);
            }
        }
        if (Input.GetKey(KeyCode.F2))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[1].transform.position += new Vector3(CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[1].transform.position += new Vector3(-CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[1].transform.position += new Vector3(0.0f, CConfigMng.Instance._nMovingSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[1].transform.position += new Vector3(0.0f, -CConfigMng.Instance._nMovingSpeed);
            }
        }
        if (Input.GetKey(KeyCode.F3))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[2].transform.position += new Vector3(CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[2].transform.position += new Vector3(-CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[2].transform.position += new Vector3(0.0f, CConfigMng.Instance._nMovingSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[2].transform.position += new Vector3(0.0f, -CConfigMng.Instance._nMovingSpeed);
            }
        }
        if (Input.GetKey(KeyCode.F4))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[3].transform.position += new Vector3(CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[3].transform.position += new Vector3(-CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[3].transform.position += new Vector3(0.0f, CConfigMng.Instance._nMovingSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[3].transform.position += new Vector3(0.0f, -CConfigMng.Instance._nMovingSpeed);
            }
        }
        if (Input.GetKey(KeyCode.F5))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[4].transform.position += new Vector3(CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[4].transform.position += new Vector3(-CConfigMng.Instance._nMovingSpeed, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[4].transform.position += new Vector3(0.0f, CConfigMng.Instance._nMovingSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[4].transform.position += new Vector3(0.0f, -CConfigMng.Instance._nMovingSpeed);
            }
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[0].transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[0].transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[0].transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[0].transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[1].transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[1].transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[1].transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[1].transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[2].transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[2].transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[2].transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[2].transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[3].transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[3].transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[3].transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[3].transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _PositionObjs[4].transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _PositionObjs[4].transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _PositionObjs[4].transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _PositionObjs[4].transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }


        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("¿˙¿Â");
            SaveData saveData = new SaveData(_PositionObjs[0].transform.localPosition, _PositionObjs[1].transform.localPosition, _PositionObjs[2].transform.localPosition, _PositionObjs[3].transform.localPosition, _PositionObjs[4].transform.localPosition);
            SavePosition savepos = new SavePosition(_PositionObjs[0].transform.localScale, _PositionObjs[1].transform.localScale, _PositionObjs[2].transform.localScale, _PositionObjs[3].transform.localScale, _PositionObjs[4].transform.localScale);
            SaveSystem.Save(saveData, "SAVE");
            SaveSystem.SaveScale(savepos, "SAVE_SCALE");
        }
        
    }

    public void VideoPlayer_Idel_Panel(bool AutoStart, bool Loop)
    {
        for (int i = 0; i < 5; i++)
            _VideoPlayer[i].InitiallizeContents(CConfigMng.Instance._strHide_Out_Idel_Folder, CConfigMng.Instance._strHide_Out_IDLE_00 + i.ToString(), AutoStart, Loop);
    }

    public void VideoPlayer_Content_Panel(bool AutoStart, bool Loop)
    {
        for(int i = 0; i < 5; i++)
            _VideoPlayer[i].InitiallizeContents(CConfigMng.Instance._strHide_Out_Play_Folder, CConfigMng.Instance._strHide_Out_Content_00 + i.ToString(), AutoStart, Loop);
    }

    public void FadeInWindow()
    {
        for (int i = 0; i < _VideoPlayer.Length; i++)
            _VideoPlayer[i].PlayVideo(true);

        ItweenEventStart("EventMoveUpdate", "FadeInComplete", 0.0f, 1.0f, CConfigMng.Instance._fVideoTransition, 0.0f, iTween.EaseType.easeOutExpo);
        ItweenEventStart("EventSoundUpdate", "FadeInComplete", 0.0f, CNetWorkMng.Instance._fContentSoundVolume, CConfigMng.Instance._fVideoTransition, 0.0f, iTween.EaseType.easeOutExpo);
    }

    public void FadeOutWindow()
    {
        ItweenEventStart("EventMoveUpdate", "FadeOutComplete", 1.0f, 0.0f, CConfigMng.Instance._fVideoTransition, 0.0f, iTween.EaseType.easeOutExpo);
        ItweenEventStart("EventSoundUpdate", "FadeOutComplete", CNetWorkMng.Instance._fContentSoundVolume, 0.0f, CConfigMng.Instance._fVideoTransition, 0.0f, iTween.EaseType.easeOutExpo);
    }
    public void EventSoundUpdate(float fValue)
    {
        for (int i = 0; i < _VideoPlayer.Length; i++)
        {
            _VideoPlayer[i].GetSoundVolum(fValue);
        }
    }
    public void EventMoveUpdate(float fValue)
    {
        m_CanvasGroup.alpha = fValue;
    }
    public void FadeInComplete()
    {
        for (int i = 0; i < _VideoPlayer.Length; i++)
        {
            _VideoPlayer[i].GetSoundVolum(CNetWorkMng.Instance._fContentSoundVolume);
        }
        
    }

    public void FadeOutComplete()
    {
        //Destroy(gameObject);
    }


    public void ItweenEventStart(string strUpdetName, string strCompleteName, float fValueA, float fValueB, float fSpeed, float fDelay, iTween.EaseType easyType)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", fValueA, "to", fValueB, "time", fSpeed, "delay", fDelay, "easetype", easyType.ToString(),
        "onUpdate", strUpdetName, "oncomplete", strCompleteName));
    }

}
