
using UnityEngine;
using HT.Framework;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace LB
{


    [CustomModule("AppBootstrap", true)]
    public class AppBootstrap : CustomModuleBase
    {

        private static AppBootstrap _instance;
        public static AppBootstrap Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["AppBootstrap"].Cast<AppBootstrap>();
                return _instance;
            }
        }

        private bool wantsToQuit = false;
        public event HTFAction ApplicationQuitEvent;

        private bool _initializedLogin;
        private bool _initializedMain;
        /// <summary>
        /// 初始化模块
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            Main.m_Main.ApplicationQuitEvent += OnApplicationQuit;
            Main.m_Main.ApplicationFocusEvent += OnApplicationFocus;


        }


        /// <summary>
        /// 模块准备工作
        /// </summary>
        public override void OnReady()
        {
            base.OnReady();

            IsRunning = false;
            LitJsonRegiter.Register();
            Screen.SetResolution(1920, 1080, true);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!Main.m_Debug.IsEnableDebugger)
            {
                if (!CheckSoftKeyPWD()) return;
            }
#endif
        }
        private bool CheckSoftKeyPWD()
        {
            SoftKey ytsoftkey = new SoftKey();
            string KeyPath = "";
            if (ytsoftkey.FindPort(0, ref KeyPath) != 0 && ytsoftkey.CheckKeyByReadEprom() != 0)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "未能找到指定的加密锁,请插入对应的加密锁后,再打开应用!", "确定", null, (index) =>
                {
                    ApplicationQuit();
                    return true;
                });
                return false;
            }

            return true;
        }
        //初始化 登录流程
        public void InitializeOnLogin()
        {
            if (_initializedLogin) return;
            _initializedLogin = true;

        }
        //初始化 主流程
        public async UniTask InitializeOnMain()
        {
            if (_initializedMain) return;
            _initializedMain = true;
            if (Main.m_Debug.IsEnableDebugger)
                await FileCopyModule.Instance.CopyFilesOnStartup();
            await ConfigDataManager.Instance.ReadConfigData();
            await DataSetManager.Instance.LoadDataSetAsync();
            SerialPortModule.Instance.ConnectBluetoothDevice();
        }



        //退出APP
        private void ApplicationQuit()
        {
            wantsToQuit = true;
            ApplicationQuitEvent?.Invoke();
#if !UNITY_EDITOR
            Main.m_Coroutiner.StartCoroutine(HandleApplicationQuit());
#endif

        }
        //处理退出APP
        private IEnumerator HandleApplicationQuit()
        {
            yield return Main.m_UI.OpenUI<UILoading>("正在关闭程序");

            yield return YieldInstructioner.GetWaitUntil(() =>
            {
                return Main.m_UI.GetOpenedUI<UILoading>() != null;
            });



            Main.m_UI.CloseUI<UILoading>();
            Application.Quit();
        }


        private void OnApplicationQuit()
        {
            if (!wantsToQuit) return;

            // #if !UNITY_EDITOR
            //             Process.GetCurrentProcess().Kill();//解决退出卡死问题
            // #endif
        }




        private void OnApplicationFocus(bool focus)
        {

        }


        /// <summary>
        /// 终结模块
        /// </summary>
        public override void OnTerminate()
        {
            base.OnTerminate();

            Main.m_Main.ApplicationQuitEvent -= OnApplicationQuit;
            Main.m_Main.ApplicationFocusEvent -= OnApplicationFocus;
        }



    }

}
