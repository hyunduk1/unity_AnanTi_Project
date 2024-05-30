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

    private bool m_isFullScreen; public bool _isFullScreen { get { return m_isFullScreen; } set { m_isFullScreen = value; } }
    private int m_ScreenSizeX; public int _ScreenSizeX { get { return m_ScreenSizeX; } set { m_ScreenSizeX = value; } }
    private int m_ScreenSizeY; public int _ScreenSizeY { get { return m_ScreenSizeY; } set { m_ScreenSizeY = value; } }
    private int m_ScreenPosX; public int _ScreenPosX { get { return m_ScreenPosX; } set { m_ScreenPosX = value; } }
    private int m_ScreenPosY; public int _ScreenPosY { get { return m_ScreenPosY; } set { m_ScreenPosY = value; } }
    private string m_strNdiName; public string _strNdiName { get { return m_strNdiName; }set { m_strNdiName = value; } }
    private float m_fRippleStrength; public float _fRippleStrength { get { return m_fRippleStrength; } set { m_fRippleStrength = value; } }
    private float m_fSpeed; public float _fSpeed { get { return m_fSpeed; } set { m_fSpeed = value; } }
    private float m_fDropIneterval; public float _fDropIneterval { get { return m_fDropIneterval; } set { m_fDropIneterval = value; } }

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

  
    private string m_strCaptionTitle = "";



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

        strPath = Application.dataPath + "/StreamingAssets/Config.ini";

        m_isFullScreen = IniReadValuebool("SET_VALUE", "IS_WINDOW_MODE");

        m_ScreenPosX = IniReadValueInt("SET_VALUE", "SCREEN_POS_X");
        m_ScreenPosY = IniReadValueInt("SET_VALUE", "SCREEN_POS_Y");

        m_ScreenSizeX = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_X");
        m_ScreenSizeY = IniReadValueInt("SET_VALUE", "SCREEN_SIZE_Y");
        m_strCaptionTitle = IniReadValue("SET_VALUE", "CAPTION_TITLE");
        m_strNdiName = IniReadValue("SET_VALUE", "NDI_NAME");
        m_fRippleStrength = IniReadValueFloat("SET_VALUE", "RIPPLE_STRENGTH");
        m_fSpeed = IniReadValueFloat("SET_VALUE", "RIPPLE_SPEED");
        m_fDropIneterval = IniReadValueFloat("SET_VALUE", "RIPPLE_DROPINETERVAL");


#if UNITY_EDITOR
#else
        SetScreenResolution();
#endif

    }




    public void SetScreenResolution()
    {
        Cursor.visible = false;
        Application.runInBackground = true;
        Screen.SetResolution((int)m_ScreenSizeX, (int)m_ScreenSizeY, false);
        //   _CanvasScalar.referenceResolution = new Vector2(m_ScreenSizeX, m_ScreenSizeY);
        IntPtr hWnd = FindWindow(null, m_strCaptionTitle);
        SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, m_ScreenPosX, m_ScreenPosY, m_ScreenSizeX, m_ScreenSizeY, SWP_SHOWWINDOW);
    }

    public static void IniWriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, strPath);
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
