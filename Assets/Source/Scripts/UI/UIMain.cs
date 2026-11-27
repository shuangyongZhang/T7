using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    /// <summary>
    /// 主UI：顶部状态栏 + 左侧导航栏（由 WebUI2UnityUI 转换的 UIMainPanel 驱动）
    /// </summary>
    [UIResource("UIMainPanel")]
    public class UIMain : UILogicResident
    {
        [InjectPath("Content")] private RectTransform content;

        [InjectPath("HeaderBar/AppNameText")] private TMP_Text appNameText;
        [InjectPath("HeaderBar/PatientBadge/CurrentPatientNameText")] private TMP_Text patientNameText;
        [InjectPath("HeaderBar/PatientBadge/CurrentPatientMetaText")] private TMP_Text patientMetaText;
        [InjectPath("HeaderBar/PatientBadge/CurrentPatientMRNText")] private TMP_Text patientMRNText;
        [InjectPath("HeaderBar/PatientBadge/DiagnosisText")] private TMP_Text diagnosisText;
        [InjectPath("HeaderBar/DeviceStatusChip")] private Image deviceStatusChip;
        [InjectPath("HeaderBar/DeviceStatusChip/DeviceStatusDot")] private Image deviceStatusDot;
        [InjectPath("HeaderBar/DeviceStatusChip/DeviceStatusText")] private TMP_Text deviceStatusText;
        [InjectPath("HeaderBar/UserBadge/UserNameText")] private TMP_Text userNameText;
        [InjectPath("HeaderBar/UserBadge/LogoutButton")] private Button logoutButton;
        [InjectPath("HeaderBar/QuitButton")] private Button quitButton;

        [InjectPath("NavRail/NavPatientButton")] private Button patientButton;
        [InjectPath("NavRail/NavAssessmentButton")] private Button assessmentButton;
        [InjectPath("NavRail/NavTrainingButton")] private Button trainingButton;
        [InjectPath("NavRail/NavReportButton")] private Button reportButton;
        [InjectPath("NavRail/NavSettingButton")] private Button settingButton;

        private Type currentWindow;
        private bool isWindowOpened = true;

        public RectTransform Content
        {
            get { return content; }
            set { content = value; }
        }

        private readonly Dictionary<Transform, Transform> modalOriginalParents = new Dictionary<Transform, Transform>();

        // 弹窗提升到主UI根节点最末兄弟，使其盖住 HeaderBar / NavRail；关闭时还原回原面板。
        public void ShowModal(GameObject modal)
        {
            if (modal == null) return;
            Transform t = modal.transform;
            if (!modalOriginalParents.ContainsKey(t)) modalOriginalParents[t] = t.parent;
            t.SetParent(UIEntity.transform, true);
            t.SetAsLastSibling();
            modal.SetActive(true);
        }

        public void HideModal(GameObject modal)
        {
            if (modal == null) return;
            modal.SetActive(false);
            Transform t = modal.transform;
            Transform back;
            if (modalOriginalParents.TryGetValue(t, out back))
            {
                modalOriginalParents.Remove(t);
                if (back != null) t.SetParent(back, true);
            }
        }

        public override void OnInit()
        {
            base.OnInit();

            patientButton.onClick.AddListener(OnPatientInfo);
            settingButton.onClick.AddListener(OnSetting);
            assessmentButton.onClick.AddListener(OnAssessment);
            trainingButton.onClick.AddListener(OnTraining);
            reportButton.onClick.AddListener(OnReport);
            logoutButton.onClick.AddListener(OnLogout);
            quitButton.onClick.AddListener(OnQuit);
            Application.wantsToQuit += ExitResetWorkflow.OnWantsToQuit;

            Main.m_Event.Subscribe<EventPatientChanged>(OnPatientChanged);
            Main.m_Event.Subscribe<EventUIOpened>(OnUIOpened);

            appNameText.text = "上肢智能康复训练与评估系统";
            Color statusColor = new Color(0.557f, 0.557f, 0.576f, 1f);
            deviceStatusText.text = "上肢机械臂通讯未接入";
            deviceStatusText.color = statusColor;
            deviceStatusDot.color = statusColor;
            deviceStatusChip.color = new Color(0.941f, 0.941f, 0.941f, 1f);
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            userNameText.text = (Global.CurAdminInfo == null ? "未知用户" : Global.CurAdminInfo.Name) +
                (Global.CurAdminInfo != null && Global.CurAdminInfo.Role == Role.Administrator ? " (管理员)" : " (操作员)");
            ClearPatientHeader();

            settingButton.gameObject.SetActive(!Global.IsQuickLogin);
            OpenModularWindow(typeof(UIPatientInfo));
        }

        private void ClearPatientHeader()
        {
            patientNameText.text = "未选择";
            patientMetaText.text = "(男 · 0岁)";
            patientMRNText.text = "-";
            diagnosisText.text = "诊断: 未选择患者";
        }

        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            patientButton.onClick.RemoveAllListeners();
            settingButton.onClick.RemoveAllListeners();
            assessmentButton.onClick.RemoveAllListeners();
            trainingButton.onClick.RemoveAllListeners();
            reportButton.onClick.RemoveAllListeners();
            logoutButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            Main.m_Event.Unsubscribe<EventPatientChanged>(OnPatientChanged);
            Main.m_Event.Unsubscribe<EventUIOpened>(OnUIOpened);
            Application.wantsToQuit -= ExitResetWorkflow.OnWantsToQuit;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public void OpenModularWindow(Type window, params object[] args)
        {
            SwitchModularWindow(window, args);
        }

        public void SwitchModularWindow(Type window, params object[] args)
        {
            if (currentWindow == window && Main.m_UI.GetOpenedUI(currentWindow) != null) return;
            if (!isWindowOpened) return;
            isWindowOpened = false;
            if (currentWindow != null) Main.m_UI.CloseUI(currentWindow);
            Main.m_UI.OpenUI(window, args).Forget();
            NotifyButtonOn(window);
            currentWindow = window;
        }

        private void SetNavActive(Button activeButton)
        {
            HighlightNav(patientButton, activeButton == patientButton);
            HighlightNav(assessmentButton, activeButton == assessmentButton);
            HighlightNav(trainingButton, activeButton == trainingButton);
            HighlightNav(reportButton, activeButton == reportButton);
            HighlightNav(settingButton, activeButton == settingButton);
        }

        private static void HighlightNav(Button button, bool active)
        {
            if (button == null) return;
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = active
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0f, 0f, 0f, 0f);
            }
            Color textColor = active
                ? new Color(0.078f, 0.337f, 0.941f, 1f)
                : new Color(0.271f, 0.318f, 0.369f, 1f);
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>())
            {
                text.color = textColor;
            }
            Transform indicator = button.transform.Find("ActiveIndicator");
            if (indicator != null) indicator.gameObject.SetActive(active);
        }

        public void NotifyButtonOn(Type window)
        {
            if (window == typeof(UIPatientInfo)) SetNavActive(patientButton);
            else if (window == typeof(UIAssessment)) SetNavActive(assessmentButton);
            else if (window == typeof(UITraining)) SetNavActive(trainingButton);
            else if (window == typeof(UIReport)) SetNavActive(reportButton);
            else if (window == typeof(UISetting)) SetNavActive(settingButton);
        }

        private void OnPatientChanged(EventHandlerBase eventHandler)
        {
            var patientInfo = (eventHandler as EventPatientChanged).PatientInfo;
            if (patientInfo != null)
            {
                patientNameText.text = patientInfo.Name ?? "未选择";
                patientMetaText.text = "(" + (patientInfo.Sex == Sex.Female ? "女" : "男") + " · " + patientInfo.Age + "岁)";
                patientMRNText.text = string.IsNullOrEmpty(patientInfo.CardNum) ? "-" : patientInfo.CardNum;
                diagnosisText.text = "诊断: " + (string.IsNullOrEmpty(patientInfo.Diagnose) ? "未填写" : patientInfo.Diagnose);
            }
            else
            {
                ClearPatientHeader();
            }

            if (Global.CurPatientInfo != patientInfo) Global.CurPatientInfo = patientInfo;
        }

        private void OnUIOpened(EventHandlerBase eventHandler)
        {
            var type = (eventHandler as EventUIOpened).UILogic.GetType();
            if (type == typeof(UIPatientInfo) || type == typeof(UIAssessment) ||
                type == typeof(UITraining) || type == typeof(UIReport) || type == typeof(UISetting)) isWindowOpened = true;
        }

        private void OnPatientInfo()
        {
            OpenModularWindow(typeof(UIPatientInfo));
        }

        private void OnAssessment()
        {
            if (Global.CurPatientInfo == null)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("请先选择患者");
                return;
            }
            OpenModularWindow(typeof(UIAssessment));
        }

        private void OnTraining()
        {
            OpenModularWindow(typeof(UITraining));
        }

        private void OnReport()
        {
            OpenModularWindow(typeof(UIReport));
        }

        private void OnSetting()
        {
            OpenModularWindow(typeof(UISetting));
        }

        private void OnLogout()
        {
            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确定注销登录?", "确定", "取消", index =>
            {
                Global.Reset();
                Main.m_Procedure.SwitchProcedure<LoginProcedure>();
                return true;
            });
        }

        private void OnQuit()
        {
            ExitResetWorkflow.RequestExit();
        }
    }
}
