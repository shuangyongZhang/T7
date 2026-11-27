using System;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [UIResource("UIExitResetPanel")]
    public class UIExitResetPanel : UILogicResident
    {
        [InjectPath("DateTimeText")] private TMP_Text dateTimeText;
        [InjectPath("UnloadingValueBox/UnloadingValueText")] private TMP_Text unloadingValueText;
        [InjectPath("DecreaseUnloadingButton")] private Button decreaseUnloadingButton;
        [InjectPath("ConfirmUnloadingButton")] private Button confirmUnloadingButton;
        [InjectPath("ZeroButton")] private Button zeroButton;
        [InjectPath("CompleteButton")] private Button completeButton;
        [InjectPath("ExitResetDeviceText")] private TMP_Text deviceText;
        [InjectPath("ResetStatusText")] private TMP_Text statusText;

        private float refreshAccumulator;

        public override void OnInit()
        {
            base.OnInit();
            decreaseUnloadingButton.onClick.AddListener(OnDecreaseUnloading);
            confirmUnloadingButton.onClick.AddListener(OnConfirmUnloading);
            zeroButton.onClick.AddListener(OnZero);
            completeButton.onClick.AddListener(OnComplete);
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            refreshAccumulator = 0f;
            deviceText.text = RehabDeviceCommands.IsAvailable
                ? "设备通讯已接入 · 减重数值来自设备"
                : "设备通讯未接入 · 机械臂动作接口待接入";
            statusText.text = "减重数值仅用于展示，可通过向下箭头调小或点击归零。";
            RefreshUnloadingValue();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            refreshAccumulator += Time.deltaTime;
            if (refreshAccumulator >= 1f)
            {
                refreshAccumulator = 0f;
                dateTimeText.text = DateTime.Now.ToString("yyyy-MM-dd | HH:mm");
                RefreshUnloadingValue();
            }
            if (Input.GetKeyDown(KeyCode.Escape)) ClosePanel();
        }

        private void OnDecreaseUnloading()
        {
            DeviceCommandResult result = RehabDeviceCommands.DecreaseUnloading();
            statusText.text = RehabDeviceCommands.GetResultMessage(result);
            RefreshUnloadingValue();
        }

        private void OnConfirmUnloading()
        {
            float? value = RehabDeviceCommands.CurrentUnloadingKg;
            statusText.text = value.HasValue
                ? "已确认当前减重数值：" + value.Value.ToString("F1") + " kg"
                : "暂无设备减重数值可确认";
            RefreshUnloadingValue();
        }

        private void OnZero()
        {
            DeviceCommandResult result = RehabDeviceCommands.ZeroUnloading();
            statusText.text = RehabDeviceCommands.GetResultMessage(result);
            RefreshUnloadingValue();
        }

        private void OnComplete()
        {
            statusText.text = "正在结束并退出软件...";
            ExitResetWorkflow.ConfirmExit();
        }

        private void RefreshUnloadingValue()
        {
            float? value = RehabDeviceCommands.CurrentUnloadingKg;
            unloadingValueText.text = value.HasValue ? value.Value.ToString("F1") + " kg" : "-- kg";
        }

        private void ClosePanel()
        {
            Main.m_UI.CloseUI<UIExitResetPanel>();
        }

        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
            UIMain main = Main.m_UI.GetOpenedUI<UIMain>();
            if (main != null)
            {
                UIEntity.transform.SetParent(main.UIEntity.transform, true);
                UIEntity.transform.SetAsLastSibling();
                UIEntity.transform.localScale = Vector3.one;
                UIEntity.transform.localPosition = Vector3.zero;
            }
        }

        public override void OnDestroy()
        {
            decreaseUnloadingButton.onClick.RemoveAllListeners();
            confirmUnloadingButton.onClick.RemoveAllListeners();
            zeroButton.onClick.RemoveAllListeners();
            completeButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
