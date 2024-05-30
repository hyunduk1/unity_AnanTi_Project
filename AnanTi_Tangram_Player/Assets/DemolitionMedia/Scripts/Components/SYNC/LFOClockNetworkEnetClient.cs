﻿using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using ENet;
using Event = ENet.Event;
using EventType = ENet.EventType;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Stopwatch = System.Diagnostics.Stopwatch;



namespace DemolitionStudios.DemolitionMedia
{
    using ClockType = System.Double;

    struct Optional<T>
    {
        private readonly bool hasValue;
        public bool HasValue { get { return hasValue; } }

        private readonly T value;
        public T Value
        {
            get
            {
                if (!hasValue)
                {
                    throw new InvalidOperationException();
                }
                return value;
            }
        }

        public Optional(T value, bool hasValue = true)
        {
            this.value = value;
            this.hasValue = hasValue;
        }

        public static Optional<T> Empty => new Optional<T>(default(T), false);

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }
    }

    internal class EnetClientThread : IDisposable
    {
        private ConcurrentQueue<MessageBase> queue;
        private Host client;
        private Peer peer;
        private Thread thread;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private uint timeout_scale;
        private int sleep_time;
        private ClockType lastTimeReceivedSyncFromServer;
        private ClockType lastPositionReceivedFromServer;
        private ClockType rttToServer;
        private DateTime timeStart;
        private Stopwatch stopwatch;
        private bool enetLibraryInitialized = false;

        // Input packet reading & parsing
        byte[] readBuffer;
        MemoryStream readStream;
        BinaryReader reader;

        public bool Connected { get; private set; } = false;
        public bool Running { get; private set; } = false;
        public ClockType LastUpdateFromServerTime { get { return lastTimeReceivedSyncFromServer; } }
        public ClockType RTT { get { return rttToServer; } }
        public string ServerIp { get; private set; }

        public bool Start(ConcurrentQueue<MessageBase> message_queue, string server_ip, ushort server_port, uint timeout_scale_, int update_freq)
        {
            queue = message_queue;
            timeStart = DateTime.Now;
            stopwatch = Stopwatch.StartNew();

            timeout_scale = timeout_scale_;

            ServerIp = server_ip;
            InitENet(server_ip, server_port);

            sleep_time = (int)Math.Floor(1000.0f / update_freq);

            thread = new Thread(new ThreadStart(Execute));
            thread.Start();

            return true;
        }

        public void Stop()
        {
            cts.Cancel();
            thread.Join();
            client.Dispose();
            Connected = false;
            lastTimeReceivedSyncFromServer = lastPositionReceivedFromServer = rttToServer = 0;
        }

        private void InitENet(string server_ip, ushort server_port)
        {
            if (!enetLibraryInitialized)
            {
                Library.Initialize();
                enetLibraryInitialized = true;
            }

            client = new Host();
            Address address = new Address();

            address.SetHost(server_ip);
            address.Port = server_port;
            client.Create();
            Utilities.Log("Connecting");
            peer = client.Connect(address);

            readBuffer = new byte[1024];
            readStream = new MemoryStream(readBuffer);
            reader = new BinaryReader(readStream);
        }

        public void Execute()
        {
            Running = true;

            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    Utilities.Log("Exiting client thread");
                    Running = false;
                    break;
                }

                if (client.CheckEvents(out Event netEvent) <= 0)
                {
                    if (client.Service(5, out netEvent) <= 0)
                        continue;
                }

                switch (netEvent.Type)
                {
                    case EventType.None:
                        break;

                    case EventType.Connect:
                        // Docs: https://love2d.org/wiki/enet.peer:timeout
                        netEvent.Peer.Timeout(16 * timeout_scale, 400 * timeout_scale, 1000 * timeout_scale);
                        Connected = true;
                        Utilities.Log("Client connected to server - ID: " + peer.ID);
                        break;

                    case EventType.Disconnect:
                        Connected = false;
                        Running = false;
                        Utilities.Log("Client disconnected from server");
                        return;

                    case EventType.Timeout:
                        Connected = false;
                        Running = false;
                        Utilities.Log("Client connection timeout");
                        return;

                    case EventType.Receive:
                        //Utilities.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                        ParsePacket(ref netEvent);
                        netEvent.Packet.Dispose();
                        break;
                }

                Thread.Sleep(sleep_time);
            }

            client.Flush();
        }

        static void SendRoundTripPing(ref Event netEvent)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)Protocol.PacketId.RoundTripPing);
            var packet = default(Packet);
            packet.Create(buffer);
            netEvent.Peer.Send(0, ref packet);
        }

        private void ParsePacket(ref Event netEvent)
        {
            readStream.Position = 0;
            netEvent.Packet.CopyTo(readBuffer);
            var packetId = (Protocol.PacketId)reader.ReadByte();

            switch (packetId)
            {
                case Protocol.PacketId.RoundTripPong:
                    {
                        if (rttToServer == 0)
                            rttToServer = TimeGetSeconds() - lastTimeReceivedSyncFromServer;
                        else
                            rttToServer = (rttToServer + TimeGetSeconds() - lastTimeReceivedSyncFromServer) / 2.0f;
                        break;
                    }
                case Protocol.PacketId.Sync: {
                    SendRoundTripPing(ref netEvent);

                    ulong timestamp = reader.ReadUInt64();
                    ClockType position = 0;
                    if (typeof(ClockType) == typeof(System.Single))
                    {
                        position = reader.ReadSingle();
                    }
                    else if (typeof(ClockType) == typeof(System.Double))
                    {
                        position = reader.ReadDouble();
                    }
                    else if (typeof(ClockType) == typeof(System.Decimal))
                    {
                        position = reader.ReadDouble();
                    }

                    lastTimeReceivedSyncFromServer = TimeGetSeconds();
                    lastPositionReceivedFromServer = position;

                    var msg = new SyncMessage();
                    msg.timestamp = timestamp;
                    msg.position = position;
                    queue.Enqueue(msg);
                    break;
                }
                case Protocol.PacketId.Speed: {
                    ClockType speed = 0;
                    if (typeof(ClockType) == typeof(System.Single))
                    {
                        speed = reader.ReadSingle();
                    }
                    else if (typeof(ClockType) == typeof(System.Double))
                    {
                        speed = reader.ReadDouble();
                    }
                    else if (typeof(ClockType) == typeof(System.Decimal))
                    {
                        speed = reader.ReadDouble();
                    }

                    var msg = new SpeedMessage();
                    msg.speed = speed;
                    queue.Enqueue(msg);
                    break;
                }
                case Protocol.PacketId.Pause: {
                    bool pause = reader.ReadBoolean();

                    var msg = new PauseMessage();
                    msg.pause = pause;
                    queue.Enqueue(msg);
                    break;
                }
                default:
                    break;
            }
        }

        public ClockType TimeGetSeconds()
        {
            return (ClockType)stopwatch.ElapsedTicks / Stopwatch.Frequency;
        }

        public ClockType TimeGetSecondsDateTime()
        {
            return (ClockType)(DateTime.Now - timeStart).TotalSeconds;
        }

        public void Dispose()
        {
            Stop();
            Library.Deinitialize();
            enetLibraryInitialized = false;
        }
    }

    [AddComponentMenu("Demolition Media/LFO Clock Network Enet Client")]
    public class LFOClockNetworkEnetClient : MonoBehaviour, IPropertyPreview
    {
        private List<LFOClockBase> impl = new List<LFOClockBase>();

        private ClockType rttMultiplier = 0.5;
        private ClockType lastPositionRecieved = 0;
        private ClockType resyncPosition = 0;
        private ClockType lastSyncDifference = 0;
        private int resyncCount = 0;
        private bool sampleFilesMissing = false;

        /// Target media
        public List<Media> media;

        /// GUI skin
        public GUISkin skin;

        /// Client parameters
        [SerializeField] public string ServerIp = "127.0.0.1";
        [SerializeField] public ushort Port = 7777;
        [SerializeField] public uint TimeoutScale = 2;
        [SerializeField] public int UpdateFrequency = 1000;
        [SerializeField] private bool AutoReconnect = true;
        private EnetClientThread client;
        private ConcurrentQueue<MessageBase> message_queue;

        /// Amount of frames at which, difference with server is exceeded, triggers resyncing
        [SerializeField] public ClockType ResyncDiffFrames = 2.0;

        [SerializeField] public bool ForceVSync = true;
        [SerializeField] public bool ShowGUI = true;

        #region IPropertyPreview implementation

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            driver.AddFromName<LFOClockNetworkEnetClient>(gameObject, "_time");
        }

        #endregion


        #region MonoBehaviour implementation
        void Awake()
        {
            if (media.Count == 0)
            {
                var targetMedia = GetComponent<Media>();
                media.Add(targetMedia);
            }

            for (int idx = 0; idx < media.Count; ++idx)
            {
                if (media[idx] == null)
                {
                    impl.Add(null);
                    continue;
                }

                var clock = new LFOClockBase();
                clock.Awake();

                clock.media = media[idx];
                clock.skin = skin;

                impl.Add(clock);
            }
            
            if (ForceVSync)
                QualitySettings.vSyncCount = 1;
        }

        void Start()
        {
            for (int idx = 0; idx < media.Count; ++idx)
            {
                if (media[idx] == null)
                    continue;

                impl[idx].Start();
            }

            TryReadServerIpFromFile();
            //ServerIp = CConfigMng.Instance._strMediaServerIP;
           // Port = (ushort)CConfigMng.Instance._nMediaServerPort;
            message_queue = new ConcurrentQueue<MessageBase>();
            client = new EnetClientThread();
            client.Start(message_queue, ServerIp, Port, TimeoutScale, UpdateFrequency);

            for (int idx = 0; idx < media.Count; ++idx)
            {
                if (media[idx] == null)
                    continue;

                media[idx].Events.AddListener(OnMediaPlayerEvent);
            }
        }

        // Callback function to handle events
        public void OnMediaPlayerEvent(Media source, MediaEvent.Type type, MediaError error)
        {
            if (type == MediaEvent.Type.OpenFailed)
            {
                if (source.mediaUrl.Contains("SampleVideos"))
                {
                    sampleFilesMissing = true;
                }
            }
        }

        void UpdateClient()
        {
            // Construct a list of booleans, filled with False
            var resynced = new List<bool>(new bool[Math.Max(media.Count, 1)]);

            MessageBase msg;
            int syncMsgCount = 0;
            while (message_queue.TryDequeue(out msg))
            {
                switch (msg.type)
                {
                    case Protocol.PacketId.Sync:
                        // Note: position = syncMsg.position + avg[rtt/2] + extrapolation
                        var syncMsg = (SyncMessage)msg;
                        ClockType rtt_half = client.RTT * rttMultiplier;
                        ClockType position = syncMsg.position;
                        
                        lastPositionRecieved = position; // no rtt

                        for (int idx = 0; idx < media.Count; ++idx)
                        {
                            if (media[idx] == null || !media[idx].IsOpened)
                                continue;

                            if (!IsLocalServer() && !impl[idx].Pause && !Utilities.ApproximatelyEqual(Math.Abs(impl[idx].Speed), 0))
                            {
                                position += rtt_half;
                                break;
                            }
                        }


                        for (int idx = 0; idx < media.Count; ++idx)
                        {
                            if (media[idx] == null)
                                continue;

                            var resyncThreshold = impl[idx].Speed * ResyncDiffFrames / media[idx].VideoFramerate;
                            lastSyncDifference = impl[idx].ComputeDifference(impl[idx].Position, position/* + rtt_half*/);
                            if (lastSyncDifference >= resyncThreshold ||
                                (impl[idx].Pause && !Utilities.ApproximatelyEqual(lastSyncDifference, 0f)))
                            {
                                resyncPosition = position;
                                //Utilities.Log("Resync pos: " + resyncPosition.ToString("n3") + "; impl.Position: "
                                //    + impl.Position.ToString("n3") + "; position: " + position.ToString("n3"));

                                impl[idx].Position = position;
                                //impl.ResetDeltaTime();
                                resynced[idx] = true;
                                resyncCount += 1;
                            }
                        }

                        syncMsgCount += 1;
                        break;

                    case Protocol.PacketId.Speed:
                        var speedMsg = (SpeedMessage)msg;
                        for (int idx = 0; idx < media.Count; ++idx)
                        {
                            if (media[idx] == null)
                                continue;

                            impl[idx].Speed = speedMsg.speed;
                        }
                        break;

                    case Protocol.PacketId.Pause:
                        var pauseMsg = (PauseMessage)msg;
                        for (int idx = 0; idx < media.Count; ++idx)
                        {
                            if (media[idx] == null)
                                continue;

                            impl[idx].Pause = pauseMsg.pause;
                        }

                        if (pauseMsg.pause)
                        {
                            resyncPosition = lastPositionRecieved;
                            for (int idx = 0; idx < media.Count; ++idx)
                            {
                                if (media[idx] == null)
                                    continue;

                                impl[idx].Position = lastPositionRecieved;
                                //impl.ResetDeltaTime();

                                resynced[idx] = true;
                            }

                            //Utilities.Log("Resync pos pause: " + resyncPosition.ToString("n3"));
                        }
                        break;
                }
            }

            for (int idx = 0; idx < media.Count; ++idx)
            {
                if (media[idx] == null)
                    continue;

                bool updateClock = !resynced[idx] && client.Connected;
                impl[idx].Update(updateClock: updateClock, seekToFrame: true);
                impl[idx].UpdateTexture();
            }
        }

        void Update()
        {
            UpdateClient();

            if (AutoReconnect && !client.Connected && !client.Running)
            {
                Reconnect();
            }

            Utils.HandleKeyboardVsyncAndScale();
            if (Input.GetKeyDown(KeyCode.H))
                ShowGUI = !ShowGUI;

            if (Input.GetKeyDown(KeyCode.Equals))
            {
                rttMultiplier += 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                rttMultiplier -= 0.05f;
            }
        }

        public void OnDestroy()
        {
            client.Dispose();
        }

        private void GUICallback(int idx)
        {
            var styleError = new GUIStyle();
            styleError.normal.textColor = Color.red;
            styleError.fontSize = impl[idx].GetIMGUIFontSize() - 3;

            var styleHotkeys = new GUIStyle();
            styleHotkeys.normal.textColor = Color.white;
            styleHotkeys.fontSize = impl[idx].GetIMGUIFontSize();

            if (sampleFilesMissing)
            {
                GUILayout.BeginVertical();
                var openingUrl = Media.GetOpeningUrl(media[idx].mediaUrl, media[idx].urlType);
                GUILayout.Label("Can't open '" + openingUrl + "'", styleError);
                GUILayout.Label("Please check out 'README Pro Sync.txt' for sample videos download link", styleError);
                GUILayout.EndVertical();
                return;
            }

            //ClockType extrapolation_corr = client.TimeGetSeconds() - client.LastUpdateFromServerTime;
            var topText = "[Client] " + "Sync pos: " + lastPositionRecieved.ToString("n3") + /*" (" + extrapolation_corr.ToString("n3") + "s ago) " + */
                                    "; Resynced at " + resyncPosition.ToString("n3") + "; Diff: " + ((int)Math.Ceiling(lastSyncDifference * 1000)).ToString("D3") + "ms" +
                                    "; Resync frames: ";
            var helpText = "G - toggle full size video, H - toggle UI, F - toggle borderless fullscreen";
            var sideText = "RTT: " + client.RTT.ToString("n3") + "s";
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(topText, GUILayout.ExpandWidth(false));
            GUILayout.Label($"{ResyncDiffFrames:n1}", GUILayout.ExpandWidth(false));
            ResyncDiffFrames = GUILayout.HorizontalSlider((float)ResyncDiffFrames, 0.0f, 3.0f, GUILayout.Width(100));
            GUILayout.Label(" Resyncs: " + resyncCount.ToString(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.Label(helpText, styleHotkeys, GUILayout.ExpandWidth(false));


            GUILayout.BeginHorizontal();
            AutoReconnect = GUILayout.Toggle(AutoReconnect, "Auto Reconnect", GUILayout.ExpandWidth(false));
            if (!client.Connected)
            {
                if (!AutoReconnect)
                {
                    if (GUILayout.Button("Connect to ", GUILayout.ExpandWidth(false)))
                    {
                        Reconnect();
                    }
                }
                ServerIp = GUILayout.TextField(ServerIp, GUILayout.ExpandWidth(false));
                GUILayout.Label(" [Not connected]", GUILayout.ExpandWidth(false));
            }
            else
            {
                if (GUILayout.Button("Stop ", GUILayout.ExpandWidth(false)))
                {
                    client.Stop();
                }
                GUILayout.Label("Connected to " + client.ServerIp + "; ", GUILayout.ExpandWidth(false));
                
                GUILayout.Label(sideText, GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void OnGUI()
        {
            if (ShowGUI)
            {
                for (int idx = 0; idx < media.Count; ++idx)
                {
                    if (media[idx] == null || !media[idx].IsOpened)
                        continue;

                    impl[idx].OnGUI(() => { GUICallback(idx); });
                }
            }
        }

        #endregion

        #region utilities

        private void Reconnect()
        {
            client.Dispose();
            client = new EnetClientThread();
            client.Start(message_queue, ServerIp, Port, TimeoutScale, UpdateFrequency);
        }

        private Optional<bool> isLocalServer = Optional<bool>.Empty;
        private string prevServerIp;
        private bool IsLocalServer()
        {
            if (ServerIp != prevServerIp || !isLocalServer.HasValue)
            {
                isLocalServer = ServerIp == "127.0.0.1" || ServerIp == "localhost" || ServerIp == GetLocalIPAddress();
                prevServerIp = ServerIp;
            }
            return isLocalServer.Value;
        }

        private static string GetLocalIPAddress(NetworkInterfaceType interfaceType)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType != interfaceType)
                    continue;

                Utilities.Log($"Found local {interfaceType} adapter: {ni.Name}");
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.Address.ToString();
                }
            }
            return "";
        }

        private static string GetLocalIPAddress()
        {
            string result = GetLocalIPAddress(NetworkInterfaceType.Ethernet);
            if (result != "")
                return result;
            result = GetLocalIPAddress(NetworkInterfaceType.Wireless80211);
            if (result != "")
                return result;

            // Fallback to different API
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "";
        }

        private bool TryReadServerIpFromFile()
        {
            string path = "server_ip.txt";
            try
            {
                StreamReader reader = new StreamReader(path);
                string ip = reader.ReadLine();
                reader.Close();
                if (Uri.CheckHostName(ip) == UriHostNameType.Unknown)
                {
                    return false;
                }
                else
                {
                    ServerIp = ip;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
