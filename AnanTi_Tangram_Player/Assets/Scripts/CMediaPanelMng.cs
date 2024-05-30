using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMediaPanelMng : MonoBehaviour
{
    private static CMediaPanelMng _instance;
    public static CMediaPanelMng Instance { get { return _instance; } }
    private List<string> m_strpanelname = null;
    private Dictionary<string, GameObject> m_listPrefabs;

    public Canvas _DisplaynodeCanvas;

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

        loadprefabs("", "Hap");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void insertdisplay_moviepanel(string CurrentFileName)
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

        tempwindow = MonoBehaviour.Instantiate(m_listPrefabs["Hap"]) as GameObject;
        tempwindow.transform.SetParent(_DisplaynodeCanvas.transform);
        RectTransform rectTransform = tempwindow.transform.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = new Vector2(0, 0.0f);
        rectTransform.anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        tempwindow.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        tempwindow.GetComponent<CUIPanel>().FadeInWindow();

        tempwindow.GetComponent<CDemolitionPlayer>().MediaPlayer(CConfigMng.Instance._strTangramDataFolder, CurrentFileName);
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
