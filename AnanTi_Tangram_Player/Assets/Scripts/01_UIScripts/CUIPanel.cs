using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;

public class CUIPanel : MonoBehaviour
{
    private CanvasGroup m_CanvasGroup;
    public GameObject m_MoveObj;
  //  public MediaPlayer _VideoWindow;
    // Start is called before the first frame update
    void Start()
    {
        m_CanvasGroup = transform.GetComponent<CanvasGroup>();
        m_MoveObj.GetComponent<RectTransform>();
        if(CConfigMng.Instance._bLoadData == true)
        {
            SaveData loadData = SaveSystem.Load("SAVE");
            SavePosition loadPos = SaveSystem.LoadScale("SAVE_SCALE");
            m_MoveObj.transform.localPosition = loadData.Content_Pos[0];
            m_MoveObj.transform.localScale = loadPos.Content_Scale[0];
        }
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                m_MoveObj.transform.position += new Vector3(10, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                m_MoveObj.transform.position += new Vector3(-10, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                m_MoveObj.transform.position += new Vector3(0, 10);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                m_MoveObj.transform.position += new Vector3(0, -10);
            }
        }
        if (Input.GetKey(KeyCode.F2))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                m_MoveObj.transform.localScale += new Vector3(0.005f, 0);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                m_MoveObj.transform.localScale += new Vector3(-0.005f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                m_MoveObj.transform.localScale += new Vector3(0.0f, 0.005f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                m_MoveObj.transform.localScale += new Vector3(0.0f, -0.005f);
            }
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("¿˙¿Â");
            SaveData saveData = new SaveData(m_MoveObj.transform.localPosition);
            SavePosition savepos = new SavePosition(m_MoveObj.transform.localScale);
            SaveSystem.Save(saveData, "SAVE");
            SaveSystem.SaveScale(savepos, "SAVE_SCALE");
        }
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
    }
    public void FadeInComplete()
    {
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
