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

    private GameObject m_CurrentObj;

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
    }
    // start is called before the first frame update
    void Start()
    {
        //insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
        m_nCurrentAutoPlay = 0;
    }

    // update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TUTORIAL);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            insertdisplay_moviepanel("A2_MT-" + m_nCurrentAutoPlay + ".mp4", false);

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_01);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_02);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_03);
        }
        //CurrentVideoPlay();
    }


    //-----------------------------------------------------------------------------------------------------------------------
    public void insertdisplay_moviepanel(string CurrentFileName, bool bIsLooping = true)
    {
        
        if (m_strCurrentFileName == CurrentFileName)
        {
            return;
        }
        if (m_CurrentObj != null)
        {
            m_CurrentObj.GetComponent<CUIPanel>().FadeOutWindow();
        }
        
        m_strCurrentFileName = CurrentFileName;

        
        GameObject tempwindow;

        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs["VIdeoPlayer"]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0, 0.0f);
        rectTransform.anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        tempwindow.GetComponent<CUIPanel>().FadeInWindow();

        tempwindow.GetComponent<CVideoPlayer>().InitiallizeContents(CConfigMng.Instance._strTangramDataFolder, CurrentFileName, false, bIsLooping, true);
        m_CurrentObj = tempwindow;
        
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