using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.Events;
using System.Timers;
using System.Runtime.InteropServices;
using LitJson;
using UnityEngine.SceneManagement;
using System.IO;

public class CNetWorkMng : MonoBehaviour
{
    #region SingleTon
    private static CNetWorkMng _instance;
	public static CNetWorkMng Instance { get { return _instance; } }

    #endregion
    //public GameObject _ReConnectPopUp;

    private CUDPNetWork m_UdpNetwork;
	private Queue<string> m_MsgQuee;

	public bool _IsDebuging = false;
	private bool m_bPauseEnable = false;


	[SerializeField] public string SendIP;

	private bool m_IsServer; public bool _IsServer { get { return m_IsServer; } set { m_IsServer = value; } }
	private bool m_IsNextMovie; public bool _IsNextMovie { get { return m_IsNextMovie; } set { m_IsNextMovie = value; } }
	private float m_fContentSoundVolume; public float _fContentSoundVolume { get { return m_fContentSoundVolume; } set { m_fContentSoundVolume = value; } }


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
		m_MsgQuee = new Queue<string>();
		m_IsServer=false;
		m_UdpNetwork = gameObject.AddComponent<CUDPNetWork>();

		m_fContentSoundVolume = 1.0f;
	}


    private void OnApplicationPause(bool pause)
    {
        if (pause){
			m_bPauseEnable = true;
		}
        else{
			if(m_bPauseEnable){
				m_bPauseEnable = false;
			}
        }
    }

	public void ResetNetWork()
    {
       m_UdpNetwork.ReleaseNetWorkUdp();

       Destroy(m_UdpNetwork);
	   m_UdpNetwork = null;
	   m_MsgQuee.Clear();
	}

	public void ReConnetNetWork()
    {
		m_UdpNetwork = gameObject.AddComponent<CUDPNetWork>();
		m_UdpNetwork.Initialize((x) => ReceiveData(x));
	}

    void Start()
	{
		m_IsServer = m_UdpNetwork.Initialize((x) => ReceiveData(x));
		StartCoroutine("PacketGenerator");
	}
    private void Update()
    {
		if(Input.GetKeyDown(KeyCode.P))
        {
			Send(PROTOCOL.MSG_BOOK_GATE_MOVIE);
		}
		
    }

    public void Send(PROTOCOL protocol, int nFrameValue = 0)
    {
        /*if (m_IsServer == false)
            return;*/

        string SendString = "";
        int nValue = (int)protocol;

        switch (protocol)
        {
            case PROTOCOL.MSG_BOOK_GATE_MOVIE: SendString = "{ \"ID\":4 }"; break;
        }

        m_UdpNetwork.Send(StringToByte(SendString), CConfigMng.Instance._strSendIp, CConfigMng.Instance._nSendPort);
        Debug.Log(SendString);
        
    }


    public void PacketParser(string strPacket)
	{
		JsonData jData = JsonMapper.ToObject(strPacket);
	
		Debug.Log("Receive :   " + strPacket + " -> " + (PROTOCOL)int.Parse(jData["ID"].ToString()));

        try
        {
			switch ((PROTOCOL)int.Parse(jData["ID"].ToString()))
			{
				case PROTOCOL.MSG_HEART_BEAT: // 0


					break;
				case PROTOCOL.MSG_IDLE: // 1
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_BOOK_GATE_INTRODUCTION: //2
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_Intro);
					break;


				case PROTOCOL.MSG_BOOK_GATE_START_MOVIE:  //3
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_Story, false);
					m_IsNextMovie = true;
					break;

				case PROTOCOL.MSG_BOOK_GATE_MOVIE:  //4
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_End, false);
					Debug.Log("Server : End영상 받음");
					m_IsNextMovie = false;
					break;

				case PROTOCOL.MSG_MEDIA_SOUND_MOVING:  //5
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_TANGRAM_TUTORIAL:  //6
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_TANGRAM_PLAY: // 7
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_TANGRAM_MOVING: // 8 
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_WALL_GAME_START:  // 9
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_WALL_MOVING:  // 10
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;

				case PROTOCOL.MSG_MEDIA_FREE_PLAY_TIME: // 11
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;
				case PROTOCOL.MSG_MEDIA_ALL_FREE_PLAY_TIME:  // 12
					CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strBookGate_IDLE_IDLE);
					break;
				case PROTOCOL.MSG_MEDIA_VOLUME_CONTENTS:
					if (jData["VALUE"].ToString() != null)
					{
						m_fContentSoundVolume = float.Parse(jData["VALUE"].ToString());
						CPanelMng.Instance.SoundUpdatePacket(m_fContentSoundVolume);
					}
					break;
				default: break;
			}
		}
		catch(Exception ex)
        {
			Debug.Log("ErrorMessge" + ex);
        }
		
	}
	public void ItweenEventStart(string strUpdetName, string strCompleteName, float fValueA, float fValueB, float fSpeed, float fDelay, iTween.EaseType easyType)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", fValueA, "to", fValueB, "time", fSpeed, "delay", fDelay, "easetype", easyType.ToString(),
		"onUpdate", strUpdetName, "oncomplete", strCompleteName));
	}
	private IEnumerator PacketGenerator()
	{
		WaitForSeconds waitSec = new WaitForSeconds(0.001f);
		while (true)
		{
			if (m_MsgQuee.Count > 0)
			{ 
				PacketParser(m_MsgQuee.Dequeue());
			}
			yield return waitSec;
		}
	}
	void ReceiveData(byte[] bytes)
	{
		m_MsgQuee.Enqueue(ByteToString(bytes));
	}

	private string ByteToString(byte[] strByte)
	{
		string str = System.Text.Encoding.Default.GetString(strByte);
		return str;
	}

	private byte[] StringToByte(string str)
	{
		byte[] StrByte = System.Text.Encoding.UTF8.GetBytes(str);
		return StrByte;
	}

	public string GetSendString(string a, int b, string c, float d)
	{
		string strTemp;
		strTemp = "{" +
			"\"" + a + "\"" + ":" + b + "," +
			"\"" + c + "\"" + ":" + d +
			"}";
		return strTemp;
	}

	public string GetSendString(string a, int b, string c, int d)
    {
		string strTemp;
		strTemp = "{"+
			"\"" + a + "\"" + ":" + b +","+
			"\"" + c + "\"" + ":" + d +
			"}";
		return strTemp;
	}
	public string GetSendString(string a, int b)
    {
        string strTemp;
        strTemp = "{" +
                        "\"" + a + "\"" + ":" + b +
						"}";
		return strTemp;

	}
	public string GetSendString(string a, string b)
    {
		string strTemp;
		strTemp = "{" +
						"\"" + a + "\"" + "," + b +
					"}";
		return strTemp;
    }
}