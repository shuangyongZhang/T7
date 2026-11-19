using System.Collections.Generic;
using HT.Framework;
using System.IO;
using System;
using System.Diagnostics;

namespace LB
{

    [System.Serializable]
    public class UserData
    {
        public int MusicVolume;
        public int SoundVolume;
        public int VoiceVolume;
        public string UserName;
        public string Password;
        public string AdminPassword;
        public string HospitalName;
        public string PortName;
        public bool AgingTest;

        //UserData构造函数
        public UserData(int musicVolume, int soundVolume, int voiceVolume, string userName, string password)
        {
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
            VoiceVolume = voiceVolume;
            UserName = userName;
            Password = password;

        }
    }


    /// <summary>
    /// 存档模块
    /// </summary>
    [CustomModule("Save", true)]
    public class SaveConfigModule : CustomModuleBase
    {

        // 老化测试
        [JahroConsole.JahroCommand("老化测试", "发送时保证蓝牙连接!", "测试设置")]
        public static void SetAgingTest(bool agingTest)
        {
            Instance.SetUserData("agingTest", agingTest);
            UserData.AgingTest = agingTest;
            Instance.SaveUserData();

        }

        // 修改portName
        [JahroConsole.JahroCommand("串口设置", "输入串口名", "系统设置")]
        public static void SetSerialPortName(string portName)
        {
            Instance.SetUserData("portName", portName);
            UserData.PortName = portName;
            Instance.SaveUserData();
            // 抛出蓝牙名称改变事件
            Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventChangeBlueName>().Fill(portName));
        }

        [JahroConsole.JahroCommand("代偿开关", "开启代偿保证蓝牙连接!", "系统设置")]
        public static void SetCompensation(bool compensation)
        {
            Instance.SetUserData("compensation", compensation);
            Instance.SaveUserData();
            // 抛出代偿开关改变事件
            Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventChangeCompensation>().Fill(compensation));
        }

        [JahroConsole.JahroCommand("设置超级管理员密码", "设置超级管理员密码", "系统设置")]
        public static void SetAdminPassword(string password)
        {
            Instance.SetUserData("adminPassword", password);
            UserData.AdminPassword = password;
            Instance.SaveUserData();
        }


        //存档数据字典
        private static Dictionary<string, object> _userDataDic = new Dictionary<string, object>();
        //缓存的用户数据，用于第一次初始化。
        public static UserData UserData = new UserData(100, 100, 100, "", "");



        //SaveConfigModule的单例
        private static SaveConfigModule _instance;

        public static SaveConfigModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Main.m_CustomModule["Save"].Cast<SaveConfigModule>();
                }
                return _instance;
            }
        }




        //根据key值 存储数据到userData，数据类型需要和key值对应
        public void SetUserData(string key, object value)
        {
            if (_userDataDic.ContainsKey(key))
            {
                _userDataDic[key] = value;
            }
            else
            {
                _userDataDic.Add(key, value);
            }
        }

        //根据key值 获取数据
        public T GetUserData<T>(string key, T defaultValue = default(T))
        {
            if (_userDataDic.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            return defaultValue;
        }
        //使用SimpleJSON将_userDataDic数据存储到本地路径persistent目录下
        public void SaveUserData()
        {
            string json = JsonMapper.ToJson(_userDataDic);
            string path = Global.SavePath + "/UserData.json";
            FileHelper.WriteAllText(path, json);
#if UNITY_EDITOR
            Log.Info("SaveUserData:" + path);
            Log.Info("SaveUserData:" + json);
#endif

        }

        private T LoadJson<T>(string filePath) where T : new()
        {
            if (!FileHelper.IsFileExists(filePath))
            {
                Log.Warning("File does not exist: " + filePath);
                return new T(); // 返回一个新的实例作为默认值
            }

            try
            {
                string fileContent = FileHelper.ReadAllText(filePath);
                T data = JsonMapper.ToObject<T>(fileContent);
                return data;
            }
            catch (Exception e)
            {
                Log.Error("Error loading JSON file: " + filePath + ". Exception: " + e.Message);
                return new T();
            }
        }



        //使用JsonMapper将_userDataDic中 用户初始数据从本地路径persistent目录下读取  并赋值给userData

        void LoadUserData()
        {
            string path = Global.SavePath + "/UserData.json";
            _userDataDic = LoadJson<Dictionary<string, object>>(path);

            UserData.MusicVolume = GetUserData<int>("musicVolume", 50);
            UserData.SoundVolume = GetUserData<int>("soundVolume", 50);
            UserData.VoiceVolume = GetUserData<int>("voiceVolume", 50);
            UserData.UserName = GetUserData<string>("userName");
            UserData.Password = GetUserData<string>("password");
            UserData.AdminPassword = GetUserData<string>("adminPassword");

            UserData.PortName = GetUserData<string>("portName");
            UserData.AgingTest = GetUserData<bool>("agingTest");

#if UNITY_EDITOR
            Log.Info("LoadUserData:" + JsonMapper.ToJson(_userDataDic));
#endif
        }

        //移到AppBootstrap
        void InitSaveEnv()
        {
            //创建存档相关的目录和文件
            Log.Info("SavePath " + Global.SavePath);
            //创建数据保存目录
            try
            {
                if (!Directory.Exists(Global.SavePath))
                {
                    Directory.CreateDirectory(Global.SavePath);
                }

                //重新设置db文件保存路径
                DBManagerModule.Instance.Initialize();

                LoadUserData();

                SetVolume();
            }
            catch (Exception e)
            {
                Log.Error("初始化保存环境失败: " + e.Message);
                Log.Error("错误堆栈: " + e.StackTrace);
            }
        }


        /// <summary>
        /// 初始化模块
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

#if UNITY_ANDROID && !UNITY_EDITOR
            CheckPermissionAndRequest();
#else
            InitSaveEnv();
#endif


        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void CheckPermissionAndRequest()
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                InitSaveEnv();

            }
            else
            {
                CheckPermissionAndRequest();
            }

        }
#endif
        private void SetVolume()
        {
            Main.m_Audio.BackgroundVolume = UserData.MusicVolume / 100.0f;
            Main.m_Audio.OneShootVolume = Main.m_Audio.SingleVolume = Main.m_Audio.MultipleVolume = UserData.SoundVolume / 100.0f;

        }


        /// <summary>
        /// 模块准备工作
        /// </summary>
        public override void OnReady()
        {
            base.OnReady();

            //自动启动
            IsRunning = false;

        }
        /// <summary>
        /// 刷新模块
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        /// <summary>
        /// 终结模块
        /// </summary>
        public override void OnTerminate()
        {
            base.OnTerminate();
        }
        /// <summary>
        /// 暂停模块
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();
        }
        /// <summary>
        /// 恢复模块
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
        }
    }
}