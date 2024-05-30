﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ENet;
using Event = ENet.Event;
using EventType = ENet.EventType;
using Stopwatch = System.Diagnostics.Stopwatch;



namespace DemolitionStudios.DemolitionMedia
{
    using ClockType = System.Double;

    internal class EnetServerThread : IDisposable
    {
        public ConcurrentQueue<string> events;
        private ConcurrentQueue<byte[]> queue;
        private Host server = new Host();
        private Thread thread;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private uint timeout_scale;
        private int sleep_time;
        private Func<ClockUpdateResult> update_callback;
        private Dictionary<Peer, ClockType> serverRTTToClients = new Dictionary<Peer, ClockType>();
        private ClockType lastTimeServerSentTimeSyncMessageToClients;
        private Dictionary<Peer, ClockType> lastTimeServerReceivedRTTResponseFromClient = new Dictionary<Peer, ClockType>();
        private DateTime time_start;
        private Stopwatch stopwatch;

        // Input packet reading & parsing
        byte[] readBuffer;
        MemoryStream readStream;
        BinaryReader reader;

        // Number of peers currently connected
        public uint NumPeers { get; private set; } = 0;
        public uint NumPeersEnet { get { return server.PeersCount; } }

        public bool Start(ConcurrentQueue<byte[]> message_queue, ushort port, int max_connections, uint timeout_scale_, int update_freq,
                          Func<ClockUpdateResult> update_callback_fn)
        {

            
            queue = message_queue;
            time_start = DateTime.Now;
            stopwatch = Stopwatch.StartNew();
            timeout_scale = timeout_scale_;

            InitENet(port, max_connections);

            sleep_time = (int)Math.Floor(1000.0 / update_freq);
            update_callback = update_callback_fn;

            thread = new Thread(new ThreadStart(Execute));
            thread.Start();

            return true;
        }

        private void InitENet(ushort port, int max_connections)
        {
            Library.Initialize();
           
            server = new Host();
            Address address = new Address();
            
            address.Port = port;
            server.Create(address, max_connections);

            readBuffer = new byte[1024];
            readStream = new MemoryStream(readBuffer);
            reader = new BinaryReader(readStream);

            Utilities.Log($"ENet Playback Sync Server started on {port}");
        }

        public void Execute()
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    Utilities.Log("Exiting server thread");
                    break;
                }

                // Note: also possible to update clock with high frequency, but we don't do it 
                //       anymore because each client use its own clock for smooth playback
                var result = update_callback.Invoke();

                byte[] packet;
                while (queue.TryDequeue(out packet))
                {
                    BroadcastPacket(packet);
                }

                if (server.CheckEvents(out Event netEvent) <= 0)
                {
                    if (server.Service(5, out netEvent) <= 0)
                        continue;
                }

                switch (netEvent.Type)
                {
                    case EventType.None:
                        break;

                    case EventType.Connect:
                        Utilities.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        // Docs: https://love2d.org/wiki/enet.peer:timeout
                        netEvent.Peer.Timeout(16 * timeout_scale, 400 * timeout_scale, 1000 * timeout_scale);
                        BroadcastPauseEvent(result.paused);
                        BroadcastSpeedEvent(result.speed);
                        NumPeers += 1;
                        break;

                    case EventType.Disconnect:
                        string eventStr = "Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP;
                        events.Enqueue(eventStr);
                        Utilities.Log(eventStr);
                        NumPeers -= 1;
                        break;

                    case EventType.Timeout:
                        Utilities.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        NumPeers -= 1;
                        break;

                    case EventType.Receive:
                        HandlePacket(ref netEvent);
                        netEvent.Packet.Dispose();
                        break;
                }

                Thread.Sleep(sleep_time);
            }

            server.Flush();
        }

        private void HandlePacket(ref Event netEvent)
        {
            readStream.Position = 0;
            netEvent.Packet.CopyTo(readBuffer);
            var packetId = (Protocol.PacketId)reader.ReadByte();

            ClockType time_now = TimeGetSeconds();
            if (packetId == Protocol.PacketId.RoundTripPing)
            {
                if (serverRTTToClients.ContainsKey(netEvent.Peer))
                {
                    // If we already have a rtt recorded for this connection the new rount trip time is the old time
                    // averaged with the new time that was just calculated.
                    serverRTTToClients[netEvent.Peer] = (serverRTTToClients[netEvent.Peer] + (time_now - lastTimeServerSentTimeSyncMessageToClients)) / 2.0;
                }
                else
                {
                    // If there is no rtt recorded yet for this connection then we use the time we just calculated
                    serverRTTToClients[netEvent.Peer] = (time_now - lastTimeServerSentTimeSyncMessageToClients);
                }
                lastTimeServerReceivedRTTResponseFromClient[netEvent.Peer] = time_now;

                // Server sends RTT message back to client so that the client can calculate rtt from server
                SendRoundTripPong(ref netEvent);
            }
        }

        static void SendRoundTripPong(ref Event netEvent)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)Protocol.PacketId.RoundTripPong);
            var packet = default(Packet);
            packet.Create(buffer);
            netEvent.Peer.Send(0, ref packet);
        }

//        void BroadcastSyncEvent(ulong timestamp, ClockType position)
//        {
//            var protocol = new Protocol();
//            var buffer = protocol.Serialize((byte)Protocol.PacketId.Sync, timestamp, position);
//            var packet = default(Packet);
//            packet.Create(buffer);
//            server.Broadcast(0, ref packet);
//#if !REL
//            // TODO: need?
//#endif
//            server.Flush();
//            lastTimeServerSentTimeSyncMessageToClients = TimeGetSeconds();
//        }

        void BroadcastPauseEvent(bool pause)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)Protocol.PacketId.Pause, pause);
            var packet = default(Packet);
            packet.Create(buffer);
            server.Broadcast(0, ref packet);
            server.Flush();
        }

        void BroadcastSpeedEvent(ClockType speed)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)Protocol.PacketId.Speed, speed);
            var packet = default(Packet);
            packet.Create(buffer);
            server.Broadcast(0, ref packet);
            server.Flush();
        }

        void BroadcastPacket(byte[] packetData)
        {
            var packet = default(Packet);
            packet.Create(packetData);
            server.Broadcast(0, ref packet);
            server.Flush();
        }

        public ClockType TimeGetSeconds()
        {
            return (ClockType)stopwatch.ElapsedTicks / Stopwatch.Frequency;
        }

        public ClockType TimeGetSecondsDateTime()
        {
            return (ClockType)(DateTime.Now - time_start).TotalSeconds;
        }

        public void Dispose()
        {
            cts.Cancel();
            if (thread != null)
                thread.Join();
            server.Flush();
            server.Dispose();
            Library.Deinitialize();
        }
    }


    [AddComponentMenu("Demolition Media/LFO Clock Network Enet Server")]
    public class LFOClockNetworkEnetServer : MonoBehaviour, ITimeControl
    {
        private LFOClockBase impl = new LFOClockBase();

        /// Target media
        public Media media;

        [SerializeField, Range(-10, 10)] public ClockType Speed = 1;
        [SerializeField] public bool Pause = false;


        /// GUI skin
        public GUISkin skin;

        int _mouseDownFrame = 0;
        float _mouseDownTime = 0.0f;

        [SerializeField] public ushort Port = 7777;
        [SerializeField] public int MaxConnections = 32;
        [SerializeField] public uint TimeoutScale = 2;
        [SerializeField] public int UpdateFrequency = 120;
        [SerializeField] public float WarmupTime = 0.0f;
        private int messageId = 0;
        private ConcurrentQueue<byte[]> message_queue;
        private EnetServerThread server;
        private bool playOnStartDone = false;
        private bool sampleFilesMissing = false;

        [SerializeField] public bool ForceVSync = true;
        [SerializeField] public bool EnableKeyboardAndMouseControls = true;
        [SerializeField] public bool ShowGUI = true;

        // Provide a custom clock source if needed using ClockProvider (see "Sample Clock Provider" script)
        // Please note that Play() / Stop() methods won't work in that case
        public IClockProvider ClockProvider;
        bool _hasClockProvider => ClockProvider != null;

        // Audio source component
        private AudioSource _audioSource;
        private bool _hasAudioSource { get { return _audioSource != null && _audioSource.enabled && _audioSource.clip != null && !_audioSource.mute; } }

        #region public interface 
        public bool Play
        {
            get
            {
                if (_hasAudioSource)
                    return _audioSource.isPlaying;
                else
                    return !impl.Pause;
            }
            set
            {
                if (_hasAudioSource)
                {
                    if (value)
                        _audioSource.Play();
                    else
                        _audioSource.Pause();
                }
                else
                {
                    impl.Pause = !value;
                }
            }
        }
        public void Stop()
        {
            if (_hasAudioSource)
            {
                _audioSource.Pause();
                _audioSource.time = 0.0f;
            }
            else
            {
                impl.Pause = true;
                impl.Position = 0.0;
            }
        }
        #endregion

        #region ITimeControl implementation
        private bool _controlledByTimeline;

        public void OnControlTimeStart()
        {
            _controlledByTimeline = true;

            // In the external time mode, we can't know the actual playback
            // speed but sure that it's positive (Control Track doesn't support
            // reverse playback), so we assume that the speed is 1.0.
            Speed = 1;

        }

        public void OnControlTimeStop()
        {
            // Note: to be able to pause the timeline in the editor, we're not removing the flag here
            //_controlledByTimeline = false;
        }

        public void SetTime(double time)
        {
            impl.Position = time;
            impl.Speed = 1.0;
        }
        #endregion


        #region MonoBehaviour implementation
        void Awake()
        {
            impl.Awake();

            if (media == null)
                media = GetComponent<Media>();

            impl.media = media;
            impl.skin = skin;

            _audioSource = GetComponent<AudioSource>();

            if (ForceVSync)
                QualitySettings.vSyncCount = 1;

         //   Port = (ushort)CConfigMng.Instance._nMediaServerPort;

#if UNITY_EDITOR
             EditorApplication.pauseStateChanged += OnPauseStateChanged;
#endif
        }

        void Start()
        {
            impl.Pause = true;
            impl.Start();

            message_queue = new ConcurrentQueue<byte[]>();
            server = new EnetServerThread();
            server.Start(message_queue, Port, MaxConnections, TimeoutScale, UpdateFrequency,
                         //() => { return impl.UpdateAndGetResult(updateClock: true, 
                         //                                       seekToFrame: false); });
                         () => { return impl.GetResult(); });

            media.Events.AddListener(OnMediaPlayerEvent);


        }


        // Callback function to handle events
        public void OnMediaPlayerEvent(Media source, MediaEvent.Type type, MediaError error)
        {
            if (type == MediaEvent.Type.OpenFailed)
            {
                if (media.mediaUrl.Contains("SampleVideos"))
                {
                    sampleFilesMissing = true;
                }
            }
        }

        void UpdateServer()
        {
            var speedOld = impl.Speed;
            var pauseOld = impl.Pause;

            // Note: for audio source we don't have warmup
            if (!playOnStartDone && (impl.Pause && WarmupTime > 0 && Time.realtimeSinceStartup > WarmupTime)
                || _hasAudioSource)
            {
                impl.Pause = false;
                playOnStartDone = true;
            }

            if (_hasClockProvider)
            {
                impl.Position = ClockProvider.GetPosition();
                impl.Speed = Speed = ClockProvider.GetSpeed();
                impl.Pause = Pause = ClockProvider.GetPause();
            }
            else if (_hasAudioSource)
            {
                impl.Position = _audioSource.time;
                impl.Speed = Speed = 1.0;
                impl.Pause = Pause = !_audioSource.isPlaying;
            }

            var updateClock = !_controlledByTimeline && !_hasClockProvider && !_hasAudioSource;
            impl.Update(updateClock: updateClock, seekToFrame: true, updateTexture: true);
            {
                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Sync, (ulong)messageId++, impl.Position);
                message_queue.Enqueue(packet);
            }
            {
                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Pause, impl.Pause);
                message_queue.Enqueue(packet);
            }
            {
                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Speed, impl.Speed);
                message_queue.Enqueue(packet);
            }

            if (!_hasClockProvider && EnableKeyboardAndMouseControls)
                HandleKeyboardAndMouse();
            var speedNew = _hasClockProvider ? ClockProvider.GetSpeed() : Speed;
            var pauseNew = _hasClockProvider ? ClockProvider.GetPause() : Pause;
            if (!Utilities.ApproximatelyEqual(speedOld, speedNew))
            {
                impl.Speed = speedNew;

                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Speed, speedNew);
                message_queue.Enqueue(packet);
            }
            if (pauseOld != pauseNew && (Mathf.Approximately(WarmupTime, 0f) || playOnStartDone))
            {
                impl.Pause = pauseNew;

                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Pause, pauseNew);
                message_queue.Enqueue(packet);
            }
        }

        void Update()
        {
            UpdateServer();

            Utils.HandleKeyboardVsyncAndScale();
            if (Input.GetKeyDown(KeyCode.H))
                ShowGUI = !ShowGUI;
        }

        void OnApplicationPause(bool pause)
        {
            if (message_queue != null)
            {
                var protocol = new Protocol();
                var packet = protocol.Serialize((byte)Protocol.PacketId.Pause, pause);
                message_queue.Enqueue(packet);
            }
        }

#if UNITY_EDITOR
        private void OnPauseStateChanged(PauseState state)
        {
            var protocol = new Protocol();
            var packet = protocol.Serialize((byte)Protocol.PacketId.Pause, state == PauseState.Paused);
            message_queue.Enqueue(packet);
        }
#endif

        public void OnDestroy()
        {
            server.Dispose();
        }

        void GUICallback()
        {
            var styleError = new GUIStyle();
            styleError.normal.textColor = Color.red;
            styleError.fontSize = impl.GetIMGUIFontSize() - 3;

            var styleHotkeys = new GUIStyle();
            styleHotkeys.normal.textColor = Color.white;
            styleHotkeys.fontSize = impl.GetIMGUIFontSize();

            if (sampleFilesMissing)
            {
                GUILayout.BeginVertical();
                var openingUrl = Media.GetOpeningUrl(media.mediaUrl, media.urlType);
                GUILayout.Label("Can't open '" + openingUrl + "'", styleError);
                GUILayout.Label("Please check out 'README Pro Sync.txt' for sample videos download link", styleError);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("[Server] " + "Last position sent: " + impl.Position.ToString("n3"), GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            GUILayout.Label("Clients: " + server.NumPeersEnet + "(" + server.NumPeers + ")", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            if (_controlledByTimeline)
                GUILayout.Label("Timeline: " + _controlledByTimeline, GUILayout.ExpandWidth(false));
            else if (_hasClockProvider)
                GUILayout.Label("Clock provider: " + ClockProvider.Name, GUILayout.ExpandWidth(false));
            else if (_hasAudioSource)
                GUILayout.Label("Audio Source Clock", GUILayout.ExpandWidth(false));
            else
                GUILayout.Label("Internal LFO Clock", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            var helpText = "G - toggle full size video, H - toggle UI, F - toggle borderless fullscreen";
            if (_audioSource != null)
                helpText += ", M - toggle mute";
            GUILayout.Label(helpText, styleHotkeys, GUILayout.ExpandWidth(false));

        }

        void OnGUI()
        {
            if (ShowGUI)
                impl.OnGUI(() => { GUICallback(); });
        }

        void HandleKeyboardAndMouse()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Pause = !Pause;
                if (_hasAudioSource)
                {
                    if (Pause)
                        _audioSource.Pause();
                    else
                        _audioSource.Play();
                }
            }
            else if (_hasAudioSource || (_audioSource != null && _audioSource.mute))
            {
                if (Input.GetKeyDown(KeyCode.M))
                {
                    if (!_audioSource.mute)
                    {
                        // Switch from audio source clock to internal
                        _audioSource.mute = true;
                        impl.Pause = Pause = !_audioSource.isPlaying;
                    }
                    else
                    {
                        // Switch from internal to audio source clock
                        if (impl.Pause)
                            _audioSource.Pause();
                        else
                            _audioSource.Play();
                        _audioSource.time = (float)impl.Position;
                        _audioSource.mute = false;
                    }
                }
            }
            if (!_hasAudioSource)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    Speed = 1.0;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    Speed = 2.0;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    Speed = 3.0;
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    Speed = 4.0;
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                    Speed = 0.5;
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                    Speed = 6.0;
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                    Speed = 7.0;
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                    Speed = 8.0;
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                    Speed = 9.0;
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                    Speed = 0.0;
                else if (Input.GetKeyDown(KeyCode.Equals))
                    Speed *= 2;
                else if (Input.GetKeyDown(KeyCode.Minus))
                    Speed = -1 * Speed;
            }

            if (Input.GetMouseButton(0))
            {
                float mousePosRelX = Input.mousePosition.x / Screen.width;

                if (_hasAudioSource)
                {
                    float position = _audioSource.clip.length * mousePosRelX;
                    _audioSource.time = position;
                }
                else
                {
                    float position = media.DurationSeconds * mousePosRelX;
                    int frame = (int)(position * media.VideoFramerate);
                    if (frame != _mouseDownFrame)
                    {
                        _mouseDownFrame = frame;
                        _mouseDownTime = Time.realtimeSinceStartup;
                    }
                    impl.Position = position;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (!_hasAudioSource)
                {
                    Utilities.Log("Mouse up");
                    if (!impl.Pause/* && Time.realtimeSinceStartup - _mouseDownTime < 5e-1*/)
                    {
                        Utilities.Log("Seek correction set");
                        impl.SetMakeSeekCorrection(true);
                    }
                }
            }
        }
#endregion

    }
}
