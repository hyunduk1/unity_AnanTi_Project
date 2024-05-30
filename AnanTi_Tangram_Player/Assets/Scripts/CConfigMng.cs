using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.IO;

public class CConfigMng : MonoBehaviour
{
    private static CConfigMng _instance;
    public static CConfigMng Instance { get { return _instance; } }

    const int HWND_TOPMOST = -2;
    const uint SWP_HIDEWINDOW = 0x0080;
    const uint SWP_SHOWWINDOW = 0x0040;

    private static string strPath;
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


    private bool m_isFullScreen;    public bool _isFullScreen { get { return m_isFullScreen; } set { m_isFullScreen = value; } }
    private int m_ScreenSizeX;      public int _ScreenSizeX { get { return m_ScreenSizeX; } set { m_ScreenSizeX = value; } }
    private int m_ScreenSizeY;      public int _ScreenSizeY { get { return m_ScreenSizeY; } set { m_ScreenSizeY = value; } }
    private int m_ScreenPosX;       public int _ScreenPosX { get { return m_ScreenPosX; } set { m_ScreenPosX = value; } }
    private int m_ScreenPosY;       public int _ScreenPosY { get { return m_ScreenPosY; } set { m_ScreenPosY = value; } }

    private int m_nUISizeX;         public int _nUISizeX { get { return m_nUISizeX; } set { m_nUISizeX = value; } }
    private int m_nUISizeY;         public int _nUISizeY { get { return m_nUISizeY; } set { m_nUISizeY = value; } }
    private bool m_bCursorEnable; public bool _bCursorEnable { get { return m_bCursorEnable; } set { m_bCursorEnable = value; } }
    private bool m_bFpsToString; public bool _bFpsToString { get { return m_bFpsToString; } set { m_bFpsToString = value; } }
    private int m_nDisPlayDevices; public int _nDisPlayDevices { get { return m_nDisPlayDevices; }set { m_nDisPlayDevices = value; } }

    private string m_strClientIp; public string _strClientIp { get { return m_strClientIp; } set { m_strClientIp = value; } }

    private int m_nClientPort; public int _nClientPort { get { return m_nClientPort; } set { m_nClientPort = value; } }


    private string m_strTangramDataFolder; public string _strTangramDataFolder { get { return m_strTangramDataFolder; } set { m_strTangramDataFolder = value; } }

    private string m_strMEDIATANGRAM_IDLE; public string _strMEDIATANGRAM_IDLE { get { return m_strMEDIATANGRAM_IDLE; } set { m_strMEDIATANGRAM_IDLE = value; } }
    private string m_strMEDIATANGRAM_TUTORIAL; public string _strMEDIATANGRAM_TUTORIAL { get { return m_strMEDIATANGRAM_TUTORIAL; } set { m_strMEDIATANGRAM_TUTORIAL = value; } }
    private string m_strMEDIATANGRAM_TYPE_00; public string _strMEDIATANGRAM_TYPE_00 { get { return m_strMEDIATANGRAM_TYPE_00; } set { m_strMEDIATANGRAM_TYPE_00 = value; } }
    private string m_strMEDIATANGRAM_TYPE_00_0; public string _strMEDIATANGRAM_TYPE_00_0 { get { return m_strMEDIATANGRAM_TYPE_00_0; } set { m_strMEDIATANGRAM_TYPE_00_0 = value; } }
    private string m_strMEDIATANGRAM_TYPE_01; public string _strMEDIATANGRAM_TYPE_01 { get { return m_strMEDIATANGRAM_TYPE_01; } set { m_strMEDIATANGRAM_TYPE_01 = value; } }
    private string m_strMEDIATANGRAM_TYPE_01_1; public string _strMEDIATANGRAM_TYPE_01_1 { get { return m_strMEDIATANGRAM_TYPE_01_1; } set { m_strMEDIATANGRAM_TYPE_01_1 = value; } }
    private string m_strMEDIATANGRAM_TYPE_02; public string _strMEDIATANGRAM_TYPE_02 { get { return m_strMEDIATANGRAM_TYPE_02; } set { m_strMEDIATANGRAM_TYPE_02 = value; } }
    private string m_strMEDIATANGRAM_TYPE_02_2; public string _strMEDIATANGRAM_TYPE_02_2 { get { return m_strMEDIATANGRAM_TYPE_02_2; } set { m_strMEDIATANGRAM_TYPE_02_2 = value; } }
    private string m_strMEDIATANGRAM_TYPE_03; public string _strMEDIATANGRAM_TYPE_03 { get { return m_strMEDIATANGRAM_TYPE_03; } set { m_strMEDIATANGRAM_TYPE_03 = value; } }
    private string m_strMEDIATANGRAM_TYPE_03_3; public string _strMEDIATANGRAM_TYPE_03_3 { get { return m_strMEDIATANGRAM_TYPE_03_3; } set { m_strMEDIATANGRAM_TYPE_03_3 = value; } }
    private bool m_bLoadData; public bool _bLoadData { get { return m_bLoadData; } set { m_bLoadData = value; } }

    void Awake()
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
        Application.targetFrameRate = 60;
        strPath = Application.dataPath + "/StreamingAssets/Config.ini";

    
        m_isFullScreen      = IniReadValuebool("SET_VALUE", "IS_WINDOW_MODE");

        m_ScreenPosX        = IniReadValueInt("SET_VALUE", "SCREEN_POS_X");
        m_ScreenPosY        = IniReadValueInt("SET_VALUE", "SCREEN_POS_Y");

        m_ScreenSizeX       = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_X");
        m_ScreenSizeY       = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_Y");

        m_nUISizeX          = IniReadValueInt("SET_VALUE", "UI_RESOLUTION_SIZE_X");
        m_nUISizeY          = IniReadValueInt("SET_VALUE", "UI_RESOLUTION_SIZE_Y");

        m_bCursorEnable     = IniReadValuebool("SET_VALUE", "IS_CURSOR_ENABLE");
        m_bFpsToString      = IniReadValuebool("SET_VALUE", "IS_FPS_ENABLE");
        m_nDisPlayDevices   = IniReadValueInt("SET_VALUE", "DISPLAY_DEVICE");

        m_strTangramDataFolder = IniReadValue("SET_VALUE", "TANGRAM_DATA_FOLDER");


        m_strMEDIATANGRAM_IDLE = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_IDLE");
        m_strMEDIATANGRAM_TUTORIAL = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TUTORIAL");
        m_strMEDIATANGRAM_TYPE_00 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_00");
        m_strMEDIATANGRAM_TYPE_00_0 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_00_0");
        m_strMEDIATANGRAM_TYPE_01 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_01");
        m_strMEDIATANGRAM_TYPE_01_1 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_01_1");
        m_strMEDIATANGRAM_TYPE_02 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_02");
        m_strMEDIATANGRAM_TYPE_02_2 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_02_2");
        m_strMEDIATANGRAM_TYPE_03 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_03");
        m_strMEDIATANGRAM_TYPE_03_3 = IniReadValue("SET_VALUE", "02_MEDIATANGRAM_TYPE_03_3");


        m_bLoadData = IniReadValuebool("SET_VALUE", "JSON_LOADING");

        m_strClientIp       = IniReadValue("SET_VALUE", "CLIENT_IP");
        m_nClientPort       = IniReadValueInt("SET_VALUE", "CLIENT_PORT");


#if UNITY_EDITOR
#else
        SetScreenResolution();
#endif

    }
    private void Start()
    {
        Cursor.visible = m_bCursorEnable;
    }
    public void SetScreenResolution()
    {
     //   Application.targetFrameRate = 60;
        Cursor.visible = m_bCursorEnable;
        Application.runInBackground = true;
        Screen.SetResolution((int)m_ScreenSizeX, (int)m_ScreenSizeY, false);

        SetWindowPos(GetForegroundWindow(), (IntPtr)HWND_TOPMOST, m_ScreenPosX, m_ScreenPosY, m_ScreenSizeX, m_ScreenSizeY, SWP_SHOWWINDOW);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    public static string IniReadValue(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, strPath);
        return temp.ToString();
    }


    public static float IniReadValueFloat(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, strPath);
        float result = 0.0f;
        float.TryParse(temp.ToString(), out result);
        return result;
    }

    public static bool IniReadValuebool(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, strPath);
        int result = 0;
        int.TryParse(temp.ToString(), out result);
        if (result == 1)
        {
            return true;
        }
        return false;
    }

    public static int IniReadValueInt(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, strPath);
        int result = 0;
        int.TryParse(temp.ToString(), out result);
        return result;
    }


    public static int IniReadValueIntTimeData(string Section, string Key, string strDataPath)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, strDataPath);
        int result = 0;
        int.TryParse(temp.ToString(), out result);
        return result;
    }
}



/*
AAAA	0
AAAB	0
AABA    1
AABB	1
ABAA	2
ABAB	2
ABBA	3
ABBB	3
BAAA	4
BAAB	4
BABA	5
BABB	5
BBAA	6
BBAB	6
BBBA	7
BBBB	7
*/