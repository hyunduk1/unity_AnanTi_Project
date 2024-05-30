using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.IO;
using AudioStream;

public class CConfigMng : MonoBehaviour
{
    private static CConfigMng _instance;
    public static CConfigMng Instance { get { return _instance; } }

    const int HWND_TOPMOST = -2;
    const uint SWP_HIDEWINDOW = 0x0080;
    const uint SWP_SHOWWINDOW = 0x0040;

    public AudioSourceOutputDevice _AudioDevice;

    private static string strPath;
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    private string m_strAudioFolderName; public string _strAudioFolderName { get { return m_strAudioFolderName; } set { m_strAudioFolderName = value; } }
    private string m_strAudio_L_FileName; public string _strAudio_L_FileName { get { return m_strAudio_L_FileName; } set { m_strAudio_L_FileName = value; } }
    private string m_strAudio_R_FileName; public string _strAudio_R_FileName { get { return m_strAudio_R_FileName; } set { m_strAudio_R_FileName = value; } }
    private bool m_bAudioLoop; public bool _bAudioLoop { get { return m_bAudioLoop; } set { m_bAudioLoop = value; } }
    private float m_fAudioVolume; public float _fAudioVolume { get { return m_fAudioVolume; } set { m_fAudioVolume = value; } }
    private int m_nAudioL; public int _nAudioL { get { return m_nAudioL; } set { m_nAudioL = value; } }
    private int m_nAudioR; public int _nAudioR { get { return m_nAudioR; } set { m_nAudioR = value; } }
    private int m_nAudioDevice; public int _nAudioDeviceMode { get { return m_nAudioDevice; } set { m_nAudioDevice = value; } }

    private bool m_isFullScreen; public bool _isFullScreen { get { return m_isFullScreen; } set { m_isFullScreen = value; } }
    private int m_ScreenSizeX; public int _ScreenSizeX { get { return m_ScreenSizeX; } set { m_ScreenSizeX = value; } }
    private int m_ScreenSizeY; public int _ScreenSizeY { get { return m_ScreenSizeY; } set { m_ScreenSizeY = value; } }
    private int m_ScreenPosX; public int _ScreenPosX { get { return m_ScreenPosX; } set { m_ScreenPosX = value; } }
    private int m_ScreenPosY; public int _ScreenPosY { get { return m_ScreenPosY; } set { m_ScreenPosY = value; } }

    private bool m_bCursorEnable; public bool _bCursorEnable { get { return m_bCursorEnable; } set { m_bCursorEnable = value; } }

    private string m_strSendIP; public string _strSendIP { get { return m_strSendIP; } set { m_strSendIP = value; } }
    private string m_strClientIP; public string _strClientIP { get { return m_strClientIP; } set { m_strClientIP = value; } }
    private int m_nClientPort; public int _nClientPort { get { return m_nClientPort; } set { m_nClientPort = value; } }
    private int m_nSendPort; public int _nSendPort { get { return m_nSendPort; } set { m_nSendPort = value; } }
    private bool m_bIsSoundServer; public bool _bIsSoundServer { get { return m_bIsSoundServer; } set { m_bIsSoundServer = value; } }


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

        m_bIsSoundServer = IniReadValuebool("SET_VALUE", "IS_SOUND_SERVER");
        m_isFullScreen      = IniReadValuebool("SET_VALUE", "IS_WINDOW_MODE");

        m_ScreenPosX        = IniReadValueInt("SET_VALUE", "SCREEN_POS_X");
        m_ScreenPosY        = IniReadValueInt("SET_VALUE", "SCREEN_POS_Y");

        m_ScreenSizeX       = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_X");
        m_ScreenSizeY       = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_Y");
        m_bCursorEnable     = IniReadValuebool("SET_VALUE", "IS_CURSOR_ENABLE");

        m_strAudioFolderName = IniReadValue("SET_VALUE", "AUDIO_FOLDER_NAME");
        m_strAudio_L_FileName = IniReadValue("SET_VALUE", "AUDIO_L_FILE_NAME");
        m_strAudio_R_FileName = IniReadValue("SET_VALUE", "AUDIO_R_FILE_NAME");

        m_bAudioLoop = IniReadValuebool("SET_VALUE", "AUDIO_LOOP");
        m_fAudioVolume = IniReadValueFloat("SET_VALUE", "AUDIO_VOLUME");
        m_nAudioL = IniReadValueInt("SET_VALUE", "AUDIO_LEFT");
        m_nAudioR = IniReadValueInt("SET_VALUE", "AUDIO_RIGHT");
        m_nAudioDevice = IniReadValueInt("SET_VALUE", "AUDIO_DEVICE_MODE");

        m_strSendIP = IniReadValue("NET_WORK", "SEND_IP");
        m_nClientPort = IniReadValueInt("NET_WORK", "CLIENT_PORT");
        m_strClientIP = IniReadValue("NET_WORK", "CLIENT_IP");
        m_nSendPort = IniReadValueInt("NET_WORK", "SEND_PORT");

        _AudioDevice.outputDriverID = _nAudioDeviceMode;



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