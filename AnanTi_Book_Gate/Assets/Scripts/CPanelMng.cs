using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPanelMng : MonoBehaviour
{
    private static CPanelMng _instance;
    public static CPanelMng Instance { get { return _instance; } }
    private int m_nCurrentAutoPlay; public int _nCurrentAutoPlay { get { return m_nCurrentAutoPlay; } set { m_nCurrentAutoPlay = value; } }
    private List<string> m_strpanelname = null;
    private Dictionary<string, GameObject> m_listPrefabs;

    public Canvas _DisplaynodeCanvas;
    public CVideoPlayer _VideoPlayer;

    public Canvas _UguiResolution;

    private GameObject m_CurrentObj;

    private GameObject _CreateUVMap;

    private string m_strCurrentFileName;


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
        m_CurrentObj = null;

        loadprefabs("", "VIdeoPlayer");
        loadprefabs("", "UV");
    }
    // start is called before the first frame update
    void Start()
    {
        insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
    }

    // update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            if(_CreateUVMap != null)
                Destroy(_CreateUVMap);
            else
                insertdisplay_UVpanel("UV");
        }
    }

    public void SoundUpdatePacket(float fValue)
    {
        m_CurrentObj.GetComponent<CVideoPlayer>()._VideoWindow.Control.SetVolume(fValue);
    }
    public void insertdisplay_moviepanel(string CurrentFileName, bool bIsLooping = true)
    {
        
        if (m_strCurrentFileName == CurrentFileName)
            return;
        if (m_CurrentObj != null)
            m_CurrentObj.GetComponent<CUIPanel>().FadeOutWindow();

        m_strCurrentFileName = CurrentFileName;

        GameObject tempwindow;  

        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs["VIdeoPlayer"]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0, 0.0f);
        rectTransform.anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        tempwindow.GetComponent<CUIPanel>().FadeInWindow();

        tempwindow.GetComponent<CVideoPlayer>().InitiallizeContents(CConfigMng.Instance._strstrBookGateDataFolder, CurrentFileName, false, bIsLooping, true);

        m_CurrentObj = tempwindow;
    }

    //ÇÁ·ÎÁ§¼Ç ¸ÊÇÎ¿ë uv ¶ç¿ì±â
    public void insertdisplay_UVpanel(string UV)
    {
        if (_CreateUVMap != null)
            Destroy(_CreateUVMap);

        GameObject tempwindow;
        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs[UV]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0, 0.0f);
        rectTransform.anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        _CreateUVMap = tempwindow;
    }

    //---------------------------------------------------------------------------------------------------------

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