using HT.Framework;
using ArduinoBluetoothAPI;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using Timer = UnityTimer.Timer;

namespace LB
{
    [CustomModule("SerialPortModule", true)]
    public class SerialPortModule : CustomModuleBase
    {
        private static SerialPortModule _instance;
        public static SerialPortModule Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["SerialPortModule"].Cast<SerialPortModule>();
                return _instance;
            }
        }
        private readonly object _writeQueueLock = new object();
        private Queue<byte[]> writeQueue = new Queue<byte[]>();
        public string DeviceVersion { get; set; } = "0.0.1";

        private readonly object _receiveBufferLock = new object();
        private byte[] _receiveBuffer = new byte[1024];
        private int _bufferLength = 0;
        private const int FRAME_HEADER1 = 0x55;
        private const int FRAME_HEADER2 = 0x53;
        private const int FRAME_LENGTH = 11;

        #region 新加参数
        public ushort x;
        public ushort y;
        public ushort z;
        public short shortX;
        public short shortZ;
        private string m_getString;
        private Vector3 m_angle;
        private byte[] _bytes;
        private JY901Value message = new JY901Value();
        public JY901Value Message
        {
            get { return message; }
        }
        public event HTFAction<string> MessageReceivedHandler;

        #endregion

        private BluetoothHelper bluetoothHelper;

        public BluetoothHelper BluetoothHelper
        {
            get { return bluetoothHelper; }
        }
        private string bluetoothName;

        private string lastBluetoothName;
        /// <summary>
        /// 连接中
        /// </summary>
        private bool isConnecting;
        /// <summary>
        /// 重新连接中
        /// </summary>
        private bool isReconnecting;

        private Timer checkTimer;
        private Timer heartbeatTimer;
        // 事件订阅标记，避免代偿开关反复切换导致重复订阅
        private bool blueNameEventSubscribed;
        private bool compensationEventSubscribed;

        private bool checkVersion;

        public bool isReverse;
        /// <summary>
        /// 是否运行中
        /// </summary>
        public override bool IsRunning
        {
            get
            {
                return base.IsRunning;
            }
            set
            {
                base.IsRunning = value;
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        private bool isConnected;
        private float lastDataReceivedTime;
        private const float DATA_TIMEOUT = 3.0f; // 3秒超时
#endif
        public override void OnInit()
        {
            base.OnInit();
        }
        public override void OnReady()
        {
            base.OnReady();
            IsRunning = false;
        }

        /// <summary>
        /// 版本比较，暂时无意义
        /// </summary>
        public void CheckDeviceVer() { }
        public void SetMessageData(SerialConfigData serialConfigData)
        {
            message.values = new KeyResolveValue[serialConfigData.value.Length];
            for (int i = 0; i < serialConfigData.value.Length; i++)
                message.values[i] = new KeyResolveValue(serialConfigData.value[i].key, serialConfigData.isDegree);
        }

        public void ConnectBluetoothDevice()
        {
            // 监听代偿开关改变事件（无论当前开关状态都要订阅，以便运行期切换）
            if (!compensationEventSubscribed)
            {
                Main.m_Event.Subscribe<EventChangeCompensation>(OnCompensationChange);
                compensationEventSubscribed = true;
            }

            // 代偿开关关闭时不连接蓝牙
            if (!ConfigDataManager.Instance.IsCompensation)
            {
                Log.Info("代偿开关已关闭，不连接蓝牙");
                return;
            }

            TryStartBluetoothConnection();
        }

        /// <summary>
        /// 代偿开关改变：打开时开始连接蓝牙，关闭时断开并停止重连
        /// </summary>
        private void OnCompensationChange(EventHandlerBase e)
        {
            if (e is EventChangeCompensation evt)
            {
                if (evt.IsCompensation)
                {
                    Log.Info("代偿开关已打开，开始连接蓝牙");
                    TryStartBluetoothConnection();
                }
                else
                {
                    Log.Info("代偿开关已关闭，断开蓝牙并停止重连");
                    StopBluetoothConnection();
                }
            }
        }

        /// <summary>
        /// 尝试启动蓝牙连接（仅代偿开关打开时生效）
        /// </summary>
        private void TryStartBluetoothConnection()
        {
            if (!ConfigDataManager.Instance.IsCompensation)
                return;

            // 监听蓝牙名称改变事件
            if (!blueNameEventSubscribed)
            {
                Main.m_Event.Subscribe<EventChangeBlueName>(OnBluetoothNameChange);
                blueNameEventSubscribed = true;
            }

            if (isConnecting || (bluetoothHelper != null && bluetoothHelper.isConnected()))
            {
                Log.Info("蓝牙设备已连接或者正在连接中");
                return;
            }
            bluetoothName = SaveConfigModule.Instance.GetUserData<string>("portName", "JSLB01");
            if (string.IsNullOrEmpty(bluetoothName))
            {
                Log.Error("没有配置蓝牙设备名");
                return;
            }
            ReadyConnect();
            ConnectBluetooth();
            checkTimer = Timer.Register(1.0f, CheckBluetoothConnect, null, true, true);
            heartbeatTimer = Timer.Register(0.5f, Outside_Write_Data, null, true, true);
        }

        /// <summary>
        /// 停止蓝牙连接：断开设备、取消定时器、重置连接状态
        /// </summary>
        private void StopBluetoothConnection()
        {
            if (checkTimer != null)
            {
                checkTimer.Cancel();
                checkTimer = null;
            }
            if (heartbeatTimer != null)
            {
                heartbeatTimer.Cancel();
                heartbeatTimer = null;
            }
            CleanupBluetooth();
            lastBluetoothName = null;
            ResetConnect();
            ResetReconnet();
        }
        private void OnBluetoothNameChange(EventHandlerBase e)
        {
            // 代偿开关关闭时忽略蓝牙名变更，不触发连接
            if (!ConfigDataManager.Instance.IsCompensation)
                return;
            bluetoothName = SaveConfigModule.Instance.GetUserData<string>("portName", "JSLB01");
            if (string.IsNullOrEmpty(bluetoothName))
            {
                Log.Error("没有配置蓝牙设备名");
                return;
            }
            if (lastBluetoothName == bluetoothName)
            {
                Log.Info($"蓝牙设备未改变:{bluetoothName}");
                return;
            }
            if (checkTimer != null)
            {
                checkTimer.Cancel();
                checkTimer = null;
            }
            if (heartbeatTimer != null)
            {
                heartbeatTimer.Cancel();
                heartbeatTimer = null;
            }
            if (bluetoothHelper != null)
            {
                bluetoothHelper.OnConnected -= OnConnected;
                bluetoothHelper.OnConnectionFailed -= OnConnectionFailed;
                bluetoothHelper.OnDataReceived -= OnMessageReceived;
                bluetoothHelper.Disconnect();
                bluetoothHelper = null;
            }
            ReadyConnect();
            ConnectBluetooth();
            checkTimer = Timer.Register(1.0f, CheckBluetoothConnect, null, true, true);
            heartbeatTimer = Timer.Register(0.5f, Outside_Write_Data, null, true, true);
        }

        private void ReadyConnect()
        {
            ResetConnect();
            ResetReconnet();
        }
        public void ConnectBluetooth()
        {
            string errMessage = "";
            try
            {
#if UNITY_EDITOR
                BluetoothHelper.BLUETOOTH_SIMULATION = true;
#endif
                Debug.Log("连接蓝牙设备名:" + bluetoothName);
                BluetoothHelper.BLE = false;
                CleanupBluetooth();

                if (string.IsNullOrEmpty(lastBluetoothName) || !lastBluetoothName.Equals(bluetoothName))
                {
                    Debug.Log("创建新的蓝牙助手实例");
                    bluetoothHelper = BluetoothHelper.GetNewInstance(bluetoothName);
                }
                else
                {
                    Debug.Log("使用已存在的蓝牙助手实例--应该进不来");
                    bluetoothHelper = BluetoothHelper.GetInstance(bluetoothName);
                }

                bluetoothHelper.OnConnected += OnConnected;
                bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
                bluetoothHelper.OnDataReceived += OnMessageReceived;
                bluetoothHelper.setFixedLengthBasedStream(11);
                bluetoothHelper.Disconnect();
                bluetoothHelper.Connect();
                isConnecting = true;
                lastBluetoothName = bluetoothName;
            }
            catch (BluetoothHelper.BlueToothNotEnabledException)
            {
                errMessage = "蓝牙没有打开,请打开蓝牙并完成配对";
            }
            catch (BluetoothHelper.BlueToothNotSupportedException)
            {
                errMessage = "此设备不支持蓝牙";
            }
            catch (BluetoothHelper.BlueToothNotReadyException)
            {
                errMessage = "没有配对上蓝牙设备" + bluetoothName;
            }
            catch (Exception)
            {
                errMessage = "连接蓝牙发生未知错误";
            }

            if (errMessage != "" && errMessage != "此设备不支持蓝牙")
            {
                Log.Info(errMessage);
                ResetReconnet();
                return;
            }
        }
        private void ResetConnect()
        {
            isConnecting = false;
        }
        //重新校准
        public void ReCalibrate()
        {
            Recalibration();
        }
        private void ResetReconnet()
        {
            isReconnecting = false;
        }
        private void ReconnetBluetooth()
        {
            // 代偿开关关闭时不重连
            if (!ConfigDataManager.Instance.IsCompensation)
                return;
            if (bluetoothHelper == null)
            {
                ConnectBluetooth();
                return;
            }
            isReconnecting = true;
            isConnecting = true;
            Debug.Log("重新连接蓝牙设备名:" + bluetoothName);
            bluetoothHelper.Disconnect();
            bluetoothHelper.Connect();
        }
        void OnConnected(BluetoothHelper helper)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            isConnected = true;
            lastDataReceivedTime = Time.time;
#endif
            Log.Info("Connection Success");
            try
            {
                Log.Info("StartListening");
                helper.StartListening();
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
            ResetConnect();
            ResetReconnet();

        }
        void OnConnectionFailed(BluetoothHelper helper)
        {
            Log.Info("Connection Failed");
            isConnected = false;
            ResetConnect();
            ResetReconnet();
        }
        private void CleanupBluetooth()
        {
            if (bluetoothHelper != null)
            {
                try
                {
                    bluetoothHelper.OnConnected -= OnConnected;
                    bluetoothHelper.OnConnectionFailed -= OnConnectionFailed;
                    bluetoothHelper.OnDataReceived -= OnMessageReceived;
                    bluetoothHelper.Disconnect();
                }
                catch (Exception ex)
                {
                    Log.Error("断开蓝牙连接失败:" + ex.Message);
                }
                bluetoothHelper = null;
            }
        }
        private void CheckBluetoothConnect()
        {
            // 代偿开关关闭时不检查、不重连蓝牙
            if (!ConfigDataManager.Instance.IsCompensation)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
			 if (bluetoothHelper != null)
			 {
                if (!bluetoothHelper.IsBluetoothEnabled())
                {
                    bluetoothHelper.Disconnect();
                    bluetoothHelper.EnableBluetooth(true);
                    return;
                }
			 }
            }
            catch (Exception ex)
            {
                Log.Warning($"Android 蓝牙状态检查异常: {ex.Message}");
            }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            // 检查数据接收超时（3秒未收到数据认为断开），仅在已连接状态下检查
            if (isConnected && Time.time - lastDataReceivedTime > DATA_TIMEOUT)
            {
                Log.Info("3秒未接收到数据, 认为连接断开");
                isConnected = false;
                ReconnetBluetooth();
                return;
            }
#endif
            if (!isConnecting && !isReconnecting && (bluetoothHelper == null || !bluetoothHelper.isConnected()))
            {
                Log.Info("蓝牙连接丢失, 重新连接");
                ReconnetBluetooth();
            }
        }
        public bool IsConnected()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return bluetoothHelper != null && bluetoothHelper.isConnected() && isConnected;
#else
            return bluetoothHelper != null && bluetoothHelper.isConnected();
#endif

        }

        #region 读取数据 移植蓝牙+jy901
        private StringBuilder _hexStringBuilder = new StringBuilder();

        private void OnMessageReceived(BluetoothHelper helper)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            lastDataReceivedTime = Time.time;
#endif
            if (helper == null)
                return;
            _bytes = helper.ReadBytes();
            if (_bytes == null || _bytes.Length == 0)
                return;
            lock (_receiveBufferLock)
            {
                if (_bufferLength + _bytes.Length > _receiveBuffer.Length)
                {
                    int newSize = Math.Max(_receiveBuffer.Length * 2, _bufferLength + _bytes.Length);
                    Array.Resize(ref _receiveBuffer, newSize);
                }
                Array.Copy(_bytes, 0, _receiveBuffer, _bufferLength, _bytes.Length);
                _bufferLength += _bytes.Length;
                ProcessBuffer();
            }
        }

        private readonly byte[] _tempFrame = new byte[11];
        private const int MAX_FRAMES_PER_UPDATE = 50;

        private void ProcessBuffer()
        {
            if (_receiveBuffer == null)
                return;

            int processedLength = 0;
            int frameCount = 0;

            while (_bufferLength - processedLength >= FRAME_LENGTH && frameCount < MAX_FRAMES_PER_UPDATE)
            {
                int headerIndex = FindHeader(processedLength);
                if (headerIndex < 0)
                {
                    break;
                }

                if (headerIndex > processedLength)
                {
                    processedLength = headerIndex;
                    continue;
                }

                if (_bufferLength - headerIndex >= FRAME_LENGTH)
                {
                    Array.Copy(_receiveBuffer, headerIndex, _tempFrame, 0, FRAME_LENGTH);
                    ProcessFrame(_tempFrame);
                    processedLength = headerIndex + FRAME_LENGTH;
                    frameCount++;
                }
                else
                {
                    break;
                }
            }

            if (processedLength > 0 && processedLength < _bufferLength)
            {
                Array.Copy(_receiveBuffer, processedLength, _receiveBuffer, 0, _bufferLength - processedLength);
            }
            _bufferLength -= processedLength;

            if (_bufferLength > 4096)
            {
                _bufferLength = 0;
            }
        }

        private int FindHeader(int startIndex)
        {
            for (int i = startIndex; i <= _bufferLength - 2; i++)
            {
                if (_receiveBuffer[i] == FRAME_HEADER1 && _receiveBuffer[i + 1] == FRAME_HEADER2)
                {
                    return i;
                }
            }
            return -1;
        }

        private readonly int[] _frameArray = new int[11];
        private bool _isProcessingFrame = false;

        private void ProcessFrame(byte[] frame)
        {
            if (frame == null || frame.Length < FRAME_LENGTH)
                return;

            if (_isProcessingFrame)
                return;

            _isProcessingFrame = true;

            try
            {
                for (int i = 0; i < FRAME_LENGTH; i++)
                {
                    _frameArray[i] = frame[i] & 0xFF;
                }

                x = Convert.ToUInt16(((int)(_frameArray[3] << 8) | _frameArray[2]) * 180f / 32768f);
                y = Convert.ToUInt16(((int)(_frameArray[5] << 8) | _frameArray[4]) * 180f / 32768f);
                z = Convert.ToUInt16(((int)(_frameArray[7] << 8) | _frameArray[6]) * 180f / 32768f);

                ConfigDataManager configManager = ConfigDataManager.Instance;
                if (configManager != null)
                {
                    if (configManager.evalData.cnTitle == "颈椎左侧屈右侧屈")
                    {
                        m_angle.x = x;
                        m_angle.y = y;
                        m_angle.z = z;
                    }
                    else
                    {
                        shortX = (short)x;
                        shortZ = (short)z;
                        if (x > 180 && x <= 360) shortX -= 360;
                        if (z > 180 && z <= 360) shortZ -= 360;

                        if (y <= 90 && Mathf.Abs(shortX) > 90)
                        {
                            y = (ushort)(180 - y);
                        }
                        else if (y >= 270 && Mathf.Abs(shortX) <= 90)
                        {
                            y = (ushort)(y - 360);
                        }
                        else if (y >= 270 && Mathf.Abs(shortX) > 90)
                        {
                            y = (ushort)(180 - y);
                        }

                        m_angle.x = shortX;
                        m_angle.y = (short)y;
                        m_angle.z = shortZ;
                    }
                }

                if (message.values != null)
                {
                    int len = message.values.Length;
                    if (len > 0) message.values[0].SetValue(m_angle.x, isReverse);
                    if (len > 1) message.values[1].SetValue(m_angle.y, isReverse);
                    if (len > 2) message.values[2].SetValue(m_angle.z, isReverse);
                }

                if (MessageReceivedHandler != null)
                {
                    _hexStringBuilder.Length = 0;
                    for (int i = 0; i < frame.Length; i++)
                    {
                        _hexStringBuilder.AppendFormat("{0:X2} ", frame[i]);
                    }
                    m_getString = _hexStringBuilder.ToString();
                    MessageReceivedHandler(m_getString);
                }
            }
            finally
            {
                _isProcessingFrame = false;
            }
        }

        private int[] HexToArray(string[] _split)
        {
            int[] retval = new int[10];

            for (int i = 0; i < retval.Length; i++)
            {
                if (string.IsNullOrEmpty(_split[i]))
                    return null;

                retval[i] = Convert.ToInt32(_split[i], 16);
            }

            if (string.IsNullOrWhiteSpace(_split[10]))
                return null;

            return retval;
        }

        private bool SetupAngle(string[] _split)
        {
            if (_split.Length < 11)
                return false;

            int[] array = HexToArray(_split);

            if (array == null)
                return false;

            x = Convert.ToUInt16(((array[3] << 8) | array[2]) / 32768f * 180f);
            y = Convert.ToUInt16(((array[5] << 8) | array[4]) / 32768f * 180f);
            z = Convert.ToUInt16(((array[7] << 8) | array[6]) / 32768f * 180f);

            if (ConfigDataManager.Instance.evalData.cnTitle == "颈椎左侧屈右侧屈")
            {
                m_angle = new Vector3(x, y, z);
            }
            else
            {
                shortX = (short)x;
                shortZ = (short)z;
                if (x > 180 && x <= 360) shortX -= 360;
                if (z > 180 && z <= 360) shortZ -= 360;

                if (y <= 90 && Mathf.Abs(shortX) > 90)
                {
                    y = (ushort)(180 - y);
                }
                else if (y >= 270 && Mathf.Abs(shortX) <= 90)
                {
                    y = (ushort)(y - 360);
                }
                else if (y >= 270 && Mathf.Abs(shortX) > 90)
                {
                    y = (ushort)(180 - y);
                }

                m_angle.x = shortX;
                m_angle.y = (short)y;
                m_angle.z = shortZ;
            }
            try
            {
                if (message.values.Length > 0) message.values[0].SetValue(m_angle.x, isReverse);
                if (message.values.Length > 1) message.values[1].SetValue(m_angle.y, isReverse);
                if (message.values.Length > 2) message.values[2].SetValue(m_angle.z, isReverse);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                Debug.Log("Values Length: " + message.values.Length);
            }

            return true;
        }
        #endregion


        #region 写入数据 移植蓝牙+jy901
        public bool SendHex(string hexString)
        {
            byte[] data = HexStringToBytes(hexString);
            if (data == null || data.Length == 0)
                return false;
            try
            {
                EnqueueWriteQueue(data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send data: {ex.Message}");
                return false;
            }
        }

        private byte[] HexStringToBytes(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "").ToUpper();
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private void EnqueueWriteQueue(byte[] writeData)
        {
            lock (_writeQueueLock)
            {
                writeQueue.Enqueue(writeData);
            }
        }
        private void Outside_Write_Data()
        {
            byte[] data = null;
            lock (_writeQueueLock)
            {
                if (writeQueue.Count > 0)
                    data = writeQueue.Dequeue();
            }
            if (data != null)
            {
                WriteData(ref data);
            }
        }
        private void WriteData(ref Byte[] writeData)
        {
            Log.Info(string.Format("{0} 发送消息：{1}", DateTime.Now.ToString("HH:mm:ss:fff"), ToHexStrFromByte(writeData)));
            if (IsConnected())
            {
                Send(writeData);
            }
            else
            {
                Log.Info(string.Format("{0} 设备未连接,无法发送消息", DateTime.Now.ToString("HH:mm:ss:fff")));
            }
        }
        private bool Send(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;
            bluetoothHelper.SendData(data);
            return true;
        }
        private string ToHexStrFromByte(byte[] byteDatas)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < byteDatas.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", byteDatas[i]));
            }
            return builder.ToString().Trim();
        }
        #endregion
        public override void OnTerminate()
        {
            base.OnTerminate();
            if (checkTimer != null)
            {
                checkTimer.Cancel();
                checkTimer = null;
            }
            if (heartbeatTimer != null)
            {
                heartbeatTimer.Cancel();
                heartbeatTimer = null;
            }
            if (bluetoothHelper != null)
            {
                bluetoothHelper.OnConnected -= OnConnected;
                bluetoothHelper.OnConnectionFailed -= OnConnectionFailed;
                bluetoothHelper.OnDataReceived -= OnMessageReceived;
                bluetoothHelper.Disconnect();
                bluetoothHelper = null;
            }
            ResetConnect();
            ResetReconnet();
            if (blueNameEventSubscribed)
            {
                Main.m_Event.Unsubscribe<EventChangeBlueName>(OnBluetoothNameChange);
                blueNameEventSubscribed = false;
            }
            if (compensationEventSubscribed)
            {
                Main.m_Event.Unsubscribe<EventChangeCompensation>(OnCompensationChange);
                compensationEventSubscribed = false;
            }
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();
        }
        public void Recalibration()
        {
            foreach (var v in message.values)
                v.Recalibration();
        }
    }
}