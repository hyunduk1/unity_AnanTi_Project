using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class CSoundMng : MonoBehaviour
{
    private static CSoundMng _instance;
    public static CSoundMng Instance { get { return _instance; } }

    private int m_nVolumeInsert = 0;
    private bool m_bChangeVolume = true;
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_APPCOMMAND = 0x319;
    private const int APPCOMMAND_VOLUME_UP = 0x0a;
    private const int APPCOMMAND_VOLUME_DOWN = 0x09;
    private const int APPCOMMAND_VOLUME_MUTE = 0x08;
    private const int APPCOMMAND_MICROPHONE_VOLUME_UP = 0x0e;
    private const int APPCOMMAND_MICROPHONE_VOLUME_DOWN = 0x0d;

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
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            InvokeRepeating("SetMasterVolume", 0.0f, 0.01f);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            InvokeRepeating("DecreaseMasterVolume", 0.0f, 0.01f);
        }
    }
    //--------------------------------------------------------------------------
    public void SetMasterVolume()
    {
     
        if(m_bChangeVolume == true)
        {
            CancelInvoke("DecreaseMasterVolume");

            m_nVolumeInsert++;
            Debug.Log(m_nVolumeInsert);
            //volume = Mathf.Clamp(2, 0, 100);
            IntPtr hwnd = GetActiveWindowHandle();
            SendMessageW(hwnd, WM_APPCOMMAND, hwnd, (IntPtr)(APPCOMMAND_VOLUME_UP << 16));
            if (m_nVolumeInsert >= 40)
            {
                CancelInvoke("SetMasterVolume");
                m_nVolumeInsert = 40;
                m_bChangeVolume = false;
            }
        }
        

    }

    public void DecreaseMasterVolume()
    {
        if(m_bChangeVolume == false)
        {
            CancelInvoke("SetMasterVolume");
            m_nVolumeInsert--;
            Debug.Log(m_nVolumeInsert);
            //volume = Mathf.Clamp(volume, 0, 100);
            IntPtr hwnd = GetActiveWindowHandle();
            SendMessageW(hwnd, WM_APPCOMMAND, hwnd, (IntPtr)(APPCOMMAND_VOLUME_DOWN << 16));
            if (m_nVolumeInsert <= 0)
            {
                CancelInvoke("DecreaseMasterVolume");
                m_nVolumeInsert = 0;
                m_bChangeVolume = true;
            }
        }
        
    }

    //--------------------------------------------------------------------------
    private IntPtr GetActiveWindowHandle()
    {
        IntPtr hwnd = new IntPtr(10);
        if (Application.isFocused)
        {
            hwnd = GetActiveWindow();
        }
        return hwnd;
    }

    private IntPtr GetActiveWindow()
    {
        IntPtr hwnd = new IntPtr(10);
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            hwnd = GetActiveWindowBuild();
        }
        else
        {
            hwnd = GetForegroundWindow();
        }
        return hwnd;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private IntPtr GetActiveWindowBuild()
    {
        IntPtr hwnd = new IntPtr(10);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            hwnd = GetForegroundWindow();
        }
        return hwnd;
    }

}
