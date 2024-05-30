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

	private float m_nSoundVolume; public float _nSoundVolum { get { return m_nSoundVolume; } set { m_nSoundVolume = value; } }

	



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


		m_nSoundVolume = 1.0f;
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

    }

	public void Send(PROTOCOL protocol,int nFrameValue=0)
	{ 
		/*if (m_IsServer == false)
			return;*/
		
		string SendString = "";
		int nValue = (int)protocol;

		switch (protocol)
		{                   //{ \"ID\":14 ,\"VALUE\":2}
			//볼륨값 브로드캐스팅 
			case PROTOCOL.MSG_MEDIA_VOLUME_CONTENTS:				SendString = "{ \"ID\":14 ,\"VALUE\":"+m_nSoundVolume.ToString() +"}"; break;
		}

		Debug.Log(SendString);
		Debug.Log(CConfigMng.Instance._strSendIP + "/" + CConfigMng.Instance._nSendPort + "----보냄 ----" + m_nSoundVolume);
		m_UdpNetwork.Send(StringToByte(SendString), CConfigMng.Instance._strSendIP, CConfigMng.Instance._nSendPort);
	}
	

    public void PacketParser(string strPacket)
    {
        JsonData jData = JsonMapper.ToObject(strPacket);

        Debug.Log(strPacket);

        Debug.Log("Receive :   " + strPacket + " -> " + (PROTOCOL)int.Parse(jData["ID"].ToString()));

        switch ((PROTOCOL)int.Parse(jData["ID"].ToString()))
        {
            case PROTOCOL.MSG_HEART_BEAT:

                break;
			case PROTOCOL.MSG_IDLE:

				break;
			case PROTOCOL.MSG_BOOK_GATE_INTRODUCTION:

				break;
			case PROTOCOL.MSG_BOOK_GATE_START_MOVIE:

				break;
			case PROTOCOL.MSG_BOOK_GATE_MOVIE:

				break;
			case PROTOCOL.MSG_MEDIA_SOUND_MOVING:

				break;
			case PROTOCOL.MSG_MEDIA_TANGRAM_TUTORIAL:

				break;
			case PROTOCOL.MSG_MEDIA_TANGRAM_PLAY:

				break;
			case PROTOCOL.MSG_MEDIA_TANGRAM_MOVING:

				break;
			case PROTOCOL.MSG_MEDIA_WALL_GAME_START:

				break;
			case PROTOCOL.MSG_MEDIA_WALL_MOVING:

				break;
			case PROTOCOL.MSG_MEDIA_FREE_PLAY_TIME:

				break;
			case PROTOCOL.MSG_MEDIA_ALL_FREE_PLAY_TIME:

				break;

			case PROTOCOL.MSG_MEDIA_VOLUME_CONTENTS:
				m_nSoundVolume = float.Parse(jData["VALUE"].ToString());
				Debug.Log(m_nSoundVolume + "------받음");
				Send(PROTOCOL.MSG_MEDIA_VOLUME_CONTENTS);

				break;

			default: break;
        }
    }
    private IEnumerator PacketGenerator()
	{
		WaitForSeconds waitSec = new WaitForSeconds(0.001f);
		while (true)
		{
			if (m_MsgQuee.Count > 0){
				//ParsingData(m_MsgQuee.Dequeue());
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