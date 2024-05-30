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
	private static CNetWorkMng _instance;
	public static CNetWorkMng Instance { get { return _instance; } }

	//public GameObject _ReConnectPopUp;

	private CUDPNetWork m_UdpNetwork;
	private Queue<string> m_MsgQuee;

	public bool _IsDebuging = false;
	private bool m_bPauseEnable = false;


	[SerializeField] public string SendIP;

	private bool m_IsServer; public bool _IsServer { get { return m_IsServer; } set { m_IsServer = value; } }
	private bool m_IsContent00; public bool _IsContent00 { get { return m_IsContent00; } set { m_IsContent00 = value; } }
	private bool m_IsContent01; public bool _IsContent01 { get { return m_IsContent01; } set { m_IsContent01 = value; } }
	private bool m_IsContent02; public bool _IsContent02 { get { return m_IsContent02; } set { m_IsContent02 = value; } }
	private bool m_IsContent03; public bool _IsContent03 { get { return m_IsContent03; } set { m_IsContent03 = value; } }
	private bool m_IsLooping; public bool _IsLooping { get { return m_IsLooping; } set { m_IsLooping = value; } }
	private float m_fContentVolume; public float _fContentVolume { get { return m_fContentVolume; } set { m_fContentVolume = value; } }

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

		m_fContentVolume = 1.0f;
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


	/*public void Send(PROTOCOL protocol,int nFrameValue=0)
	{ 
		if (m_IsServer == false)
			return;
		
		string SendString = "";
		int nValue = (int)protocol;

		switch (protocol){
			case PROTOCOL.WEATHER_DRAW:				SendString = "{ \"ID\":0 }"; break;	
			case PROTOCOL.TEMP_DRAW:				SendString = "{ \"ID\":1 }"; break; 
			case PROTOCOL.AIRCLEAN_DRAW:			SendString = "{ \"ID\":2 }"; break; 

			case PROTOCOL.WATCHDAINFO_DRAW:         SendString = "{ \"ID\":3 }"; break;
			case PROTOCOL.WEATHERINFO_DRAW:			SendString = "{ \"ID\":4 }"; break;
			case PROTOCOL.TEMPINFO_DRAW:			SendString = "{ \"ID\":5 }"; break;
			case PROTOCOL.AIRCLEANINFO_DRAW:		SendString = "{ \"ID\":6 }"; break;
			case PROTOCOL.UPDATE_DATA_INFO:			SendString = "{ \"ID\":7 }"; break;
		}

		//m_UdpNetwork.Send(StringToByte(SendString), CConfigMng.Instance._listSendIP[i].strSentIP, CConfigMng.Instance._listSendIP[i].nPORT);
		Debug.Log(SendString);
		*//*for (int i=0; i<CConfigMng.Instance._listSendIP.Count; i++){
			if (m_UdpNetwork.GetReceiveIp()!= CConfigMng.Instance._listSendIP[i].strSentIP){
				m_UdpNetwork.Send(StringToByte(SendString), CConfigMng.Instance._listSendIP[i].strSentIP, CConfigMng.Instance._listSendIP[i].nPORT);
			}
		}*//*
	}*/


	public void PacketParser(string strPacket)
	{
		JsonData jData = JsonMapper.ToObject(strPacket);
	
		Debug.Log("Receive :   " + strPacket + " -> " + (PROTOCOL)int.Parse(jData["ID"].ToString()));

		switch ((PROTOCOL)int.Parse(jData["ID"].ToString()) )
		{
			case PROTOCOL.MSG_HEART_BEAT: // 0


				break;
			case PROTOCOL.MSG_IDLE: // 1
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();
				break;

			case PROTOCOL.MSG_BOOK_GATE_INTRODUCTION: //2
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_BOOK_GATE_START_MOVIE:  //3
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_BOOK_GATE_MOVIE:  //4
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_MEDIA_SOUND_MOVING:  //5
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_MEDIA_TANGRAM_TUTORIAL:  //6
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TUTORIAL);
				SituationSet();

				break;

			case PROTOCOL.MSG_MEDIA_TANGRAM_PLAY: // 7
				//CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strVideoFile01);

				switch(int.Parse(jData["VALUE"].ToString()))
                {
					case 0:
						CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_00,false);
						m_IsContent00 = true;
						break;

					case 1:
						CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_01,false);
						m_IsContent01 = true;
						break;

					case 2:
						CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_02,false);
						m_IsContent02 = true;
						break;

					case 3:
						CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_03,false);
						m_IsContent03 = true;
						break;

					default: break;
				}

				break;

			case PROTOCOL.MSG_MEDIA_TANGRAM_MOVING: // 8 
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_MEDIA_WALL_GAME_START:  // 9
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();

				break;

			case PROTOCOL.MSG_MEDIA_WALL_MOVING:  // 10
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();


				break;

			case PROTOCOL.MSG_MEDIA_FREE_PLAY_TIME: // 11
				m_IsLooping = true;
				CPanelMng.Instance._nCurrentAutoPlay = 0;
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_TYPE_00, false);

				break;
			case PROTOCOL.MSG_MEDIA_ALL_FREE_PLAY_TIME:  // 12
				CPanelMng.Instance.insertdisplay_moviepanel(CConfigMng.Instance._strMEDIATANGRAM_IDLE);
				SituationSet();
				break;

			case PROTOCOL.MSG_MEDIA_VOLUME_CONTENTS:
				if(jData["VALUE"].ToString() != null)
                {
					m_fContentVolume = float.Parse(jData["VALUE"].ToString());
				}
				else
                {
					Debug.Log("MSG_MEDIA_VOLUME_CONTENTS ---- VALUE : ERROR");
                }
				break;

			default:break;
		}
	}

	void SituationSet()
    {
		m_IsContent00 = false;
		m_IsContent01 = false;
		m_IsContent02 = false;
		m_IsContent03 = false;
		m_IsLooping = false;

	}

	private IEnumerator PacketGenerator()
	{
		WaitForSeconds waitSec = new WaitForSeconds(0.001f);
		while (true)
		{
			if (m_MsgQuee.Count > 0){ 
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

