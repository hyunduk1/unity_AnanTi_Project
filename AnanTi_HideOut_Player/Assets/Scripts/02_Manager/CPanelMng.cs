using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class CPanelMng : MonoBehaviour
{
    private static CPanelMng _instance;
    public static CPanelMng Instance { get { return _instance; } }
    private List<string> m_strpanelname = null;
    private Dictionary<string, GameObject> m_listPrefabs;

    public Canvas _DisplaynodeCanvas;

    public GameObject _GuideImage;


    private GameObject m_CurrentContent;
    private string m_strCurrentFileName;

    private GameObject m_IdleVideoPanel;
    private GameObject m_ContentVideoPanel;

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_APPCOMMAND = 0x319;
    private const int APPCOMMAND_VOLUME_UP = 0x0a;
    private const int APPCOMMAND_VOLUME_DOWN = 0x09;
    private const int APPCOMMAND_VOLUME_MUTE = 0x08;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        m_listPrefabs = new Dictionary<string, GameObject>();
        m_strpanelname = new List<string>();
        m_CurrentContent = null;

        loadprefabs("", "00_VideoPlayer");
    }
    // start is called before the first frame update
    void Start()
    {
        _GuideImage.SetActive(CConfigMng.Instance._bGuideEnable);
        insertIdle_moviepanel();
        insertContent_moviepanel();
    }

    // update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeMoviePanel(true);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeMoviePanel(false);
        }
    }

    /// <summary>
    /// <see langword="true"/> 
    /// 컨텐츠 플레이 영상
    /// <see langword="false"/>
    /// 아이들 플레이 영상
    /// </summary>
    /// <param name="ChangeMovie"></param>
    public void ChangeMoviePanel(bool ChangeMovie)
    {
        if (ChangeMovie == true)
        {
            if (m_ContentVideoPanel.GetComponent<CanvasGroup>().alpha == 0)
            {
                m_IdleVideoPanel.GetComponent<CUIPanel>().FadeOutWindow();
                m_ContentVideoPanel.GetComponent<CUIPanel>().FadeInWindow();
            }
        }
        else
        {
            if (m_IdleVideoPanel.GetComponent<CanvasGroup>().alpha == 0)
            {
                m_IdleVideoPanel.GetComponent<CUIPanel>().FadeInWindow();
                m_ContentVideoPanel.GetComponent<CUIPanel>().FadeOutWindow();
            }
        }

    }

    //--------------------------------------------------------------------------
    //mp4 영상 특성상 처음 영상 압축을 해제후 플레이 시키기 때문에 버벅거려서
    //두개를 띄우고 알파로 영상 교체
    private void insertContent_moviepanel()
    {
        GameObject tempwindow;

        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs["00_VideoPlayer"]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        tempwindow.GetComponent<CUIPanel>().FadeOutWindow();
        tempwindow.GetComponent<CUIPanel>().VideoPlayer_Content_Panel(true, true);
        m_ContentVideoPanel = tempwindow;
    }

    private void insertIdle_moviepanel()
    {
        

        GameObject tempwindow;

        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs["00_VideoPlayer"]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        tempwindow.GetComponent<CUIPanel>().FadeInWindow();
        tempwindow.GetComponent<CUIPanel>().VideoPlayer_Idel_Panel(true, true);
        m_IdleVideoPanel = tempwindow;
    }
    //---------------------------------------------------------------------------------------

    public void loadprefabs(string strfoldername, string strfilename)
    {
        GameObject tempobject = Resources.Load(strfoldername + strfilename) as GameObject;

        if (tempobject != null)
        {
            m_listPrefabs.Add(strfilename, tempobject);
            m_strpanelname.Add(strfilename);
        }
        else
        {
        }
    }
}