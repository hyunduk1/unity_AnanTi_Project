using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;

public class CUIPanel : MonoBehaviour
{
    private CanvasGroup m_CanvasGroup;
  //  public MediaPlayer _VideoWindow;
    // Start is called before the first frame update
    void Start()
    {
        m_CanvasGroup = transform.GetComponent<CanvasGroup>();
        
    }

    void Update()
    {
    }

    public void FadeInWindow()
    {
        ItweenEventStart("EventMoveUpdate", "FadeInComplete", 0.0f, 1.0f, 1.0f, 0.0f, iTween.EaseType.easeOutExpo);
    }

    public void FadeOutWindow()
    {
        ItweenEventStart("EventMoveUpdate", "FadeOutComplete", 1.0f, 0.0f, 1.0f, 0.0f, iTween.EaseType.easeOutExpo);
    }

    public void EventMoveUpdate(float fValue)
    {
        m_CanvasGroup.alpha = fValue;
        //_VideoWindow.Control.SetVolume(fValue);
    }
    public void FadeInComplete()
    {
        //_VideoWindow.Control.SetVolume(CNetWorkMng.Instance._fContentVolume);
    }

    public void FadeOutComplete()
    {
        Destroy(gameObject);
    }


    public void ItweenEventStart(string strUpdetName, string strCompleteName, float fValueA, float fValueB, float fSpeed, float fDelay, iTween.EaseType easyType)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", fValueA, "to", fValueB, "time", fSpeed, "delay", fDelay, "easetype", easyType.ToString(),
        "onUpdate", strUpdetName, "oncomplete", strCompleteName));
    }

}
