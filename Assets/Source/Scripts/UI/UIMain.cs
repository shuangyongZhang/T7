using System;
using Cysharp.Threading.Tasks;
using HT.Framework;
using TheraBytes.BetterUi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace LB
{
    /// <summary>
    /// 主UI
    /// </summary>
    [UIResource("UIMainPanel")]
    // [UIResource("ui", "UIMainPanel", "Prefabs/UI/UIMainPanel")]
    public class UIMain : UILogicResident
    {

        [InjectPath("LeftMenu")] private RectTransform leftMenu;
        [InjectPath("LeftMenu")] private LocationAnimations menuAnimations;


        [InjectPath("LeftMenu")] private BetterButtonToggleGroup buttonToggleGroup;

        [InjectPath("LeftMenu/MenuPanel/PatientInfoButton")] private BetterButtonToggle patientInfoButton;
        [InjectPath("LeftMenu/MenuPanel/SettingButton")] private BetterButtonToggle settingButton;

        [InjectPath("LeftMenu/PatientInfo/NameText")] private TMP_Text patientNameText;
        [InjectPath("LeftMenu/PatientInfo/AvatarImage")] private Image patientAvatarImage;
        [InjectPath("Panel/Content")] private RectTransform content;
        // [InjectPath("LeftMenu/EquipDebugButton")] private Button equipDebugButton;
        [InjectPath("TopPanel/LogoutButton")] private Button logoutButton;
        [InjectPath("TopPanel/QuitButton")] private Button quitButton;
        [InjectPath("TopPanel/DoctorText")] private TMP_Text doctorText;
        [InjectPath("TopPanel/AppNameText")] private TMP_Text appNameText;
        [InjectPath("TopPanel/icon")] private Image iconImage;
        [InjectPath("LeftMenu/SerialStatus")] private GameObject seialStatusGO;
        [InjectPath("LeftMenu/SerialStatus/Normal")] private Image seialNormalImage;
        [InjectPath("LeftMenu/SerialStatus/Abnormal")] private Image seialAbnormalImage;
        private Type currentWindow;
        private Timer hardwareCheckTimer;

        private bool isWindowOpened = true;

        public RectTransform Content
        {
            get { return content; }
            set { content = value; }
        }



        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            patientInfoButton.onClick.AddListener(OnPatientInfo);
            settingButton.onClick.AddListener(OnSetting);
            // equipDebugButton.gameObject.SetActive(true);
            // equipDebugButton.onClick.AddListener(() =>
            // {
            //     Main.m_UI.OpenUI<UIEquipDebug>().Forget();
            // });
            seialStatusGO.SetActive(ConfigDataManager.Instance.IsCompensation);
            logoutButton.onClick.AddListener(OnLogout);
            quitButton.onClick.AddListener(OnQuit);
            Main.m_Event.Subscribe<EventPatientChanged>(OnPatientChanged);
            Main.m_Event.Subscribe<EventUIOpened>(OnUIOpened);
            appNameText.text = AppDefine.AppName + " V" + Application.version.Substring(0, 1);
            hardwareCheckTimer = Timer.Register(1.0f, CheckHardwareStatus, null, true, true);
            Main.m_Event.Subscribe<EventChangeCompensation>(OnChangeCompensation);
        }
        private void CheckHardwareStatus()
        {
            if (SerialPortModule.Instance.IsConnected())
            {
                seialNormalImage.gameObject.SetActive(true);
                seialAbnormalImage.gameObject.SetActive(false);
            }
            else
            {
                seialNormalImage.gameObject.SetActive(false);
                seialAbnormalImage.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            patientNameText.text = "患者: 空";
            doctorText.text = "医师:" + Global.CurAdminInfo.Account;
            iconImage.sprite = Global.CurAdminInfo.Sex == Sex.Male ? Global.IconDic["maleDoctor"] : Global.IconDic["femaleDoctor"];
            if (Global.IsQuickLogin)
            {
                patientInfoButton.gameObject.SetActive(true);
                settingButton.gameObject.SetActive(false);
            }
            else
            {
                patientInfoButton.gameObject.SetActive(true);
                settingButton.gameObject.SetActive(true);
            }
            OpenModularWindow(typeof(UIPatientInfo));

        }
        private void OnChangeCompensation(EventHandlerBase eventHandler)
        {
            if (eventHandler is EventChangeCompensation e && e.IsCompensation)
                seialStatusGO.SetActive(true);
            else
                seialStatusGO.SetActive(false);
        }
        /// <summary>
        /// 置顶UI
        /// </summary>
        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            Timer.Cancel(hardwareCheckTimer);
            patientInfoButton.onClick.RemoveAllListeners();
            settingButton.onClick.RemoveAllListeners();
            logoutButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            Main.m_Event.Unsubscribe<EventPatientChanged>(OnPatientChanged);
            Main.m_Event.Unsubscribe<EventUIOpened>(OnUIOpened);
            Main.m_Event.Unsubscribe<EventChangeCompensation>(OnChangeCompensation);
        }



        /// <summary>
        /// UI逻辑刷新
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

        }

        public void OpenModularWindow(Type window)
        {

            NotifyButtonOn(currentWindow);

            SwitchModularWindow(window);
        }

        public void ExpandLeftMenu()
        {

            menuAnimations.StartAnimation("Expand", 3);
        }
        public void CollapseLeftMenu()
        {
            menuAnimations.StartAnimation("Collapse", 3);
        }
        public void SwitchModularWindow(Type window)
        {
            if (currentWindow == window && Main.m_UI.GetOpenedUI(currentWindow) != null) return;
            if (!isWindowOpened) return;
            isWindowOpened = false;
            if (currentWindow != null) Main.m_UI.CloseUI(currentWindow);
            Main.m_UI.OpenUI(window).Forget();
            NotifyButtonOn(window);
            currentWindow = window;
        }
        public void NotifyButtonOn(Type window)
        {
            Log.Info("NotifyButtonOn" + window);
            if (window == typeof(UIPatientInfo)) buttonToggleGroup.NotifyButtonOn(patientInfoButton, false);
            if (window == typeof(UISetting)) buttonToggleGroup.NotifyButtonOn(settingButton, false);

        }
        private void OnPatientChanged(EventHandlerBase eventHandler)
        {
            // Log.Info("接收到EventPatientChanged事件，参数：" + (eventHandler as EventPatientChanged).PatientInfo.Name);
            var patientInfo = (eventHandler as EventPatientChanged).PatientInfo;
            if (patientInfo != null)
            {
                patientNameText.text = patientInfo.Name;
                patientAvatarImage.sprite = patientInfo.Sex == Sex.Female ? Global.IconDic["women"] : Global.IconDic["man"];
            }

            if (Global.CurPatientInfo != patientInfo)
            {
                Global.CurPatientInfo = patientInfo;
            }
        }

        private void OnUIOpened(EventHandlerBase eventHandler)
        {
            var uiLogic = (eventHandler as EventUIOpened).UILogic;
            var type = uiLogic.GetType();
            if (
                 type == typeof(UIPatientInfo) ||
                type == typeof(UISetting))
            {
                isWindowOpened = true;
            }

        }
        private void OnPatientInfo()
        {
            OpenModularWindow(typeof(UIPatientInfo));
        }

        private void OnSetting()
        {
            OpenModularWindow(typeof(UISetting));
        }


        private void OnLogout()
        {
            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确定注销登录?", "确定", "取消", (index) =>
            {
                Global.Reset();
                Main.m_Procedure.SwitchProcedure<LoginProcedure>();
                return true;
            });

        }

        private void OnQuit()
        {
            // AppBootstrap.Instance.ApplicationWantsToQuit();


            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确认关闭应用?", "确定", "取消", (index) =>
            {
                Main.m_UI.OpenUI<UIStartLoading>("应用关闭中......").Forget();
                Application.Quit();
                return true;
            });

        }

    }
}