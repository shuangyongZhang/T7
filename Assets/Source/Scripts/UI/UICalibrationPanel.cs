using System;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [UIResource("UICalibrationPanel")]
    public class UICalibrationPanel : UILogicResident
    {
        [InjectPath("CalibrationIcon")] private Image calibrationIcon;
        [InjectPath("CalibrationPatientText")] private TMP_Text patientText;
        [InjectPath("CalibrationStatusText")] private TMP_Text statusText;
        [InjectPath("DeviceZeroConfirmedToggle")] private Toggle deviceZeroConfirmedToggle;
        [InjectPath("ConfirmWearButton")] private Button confirmWearButton;
        [InjectPath("ZeroUnloadingButton")] private Button zeroUnloadingButton;
        [InjectPath("EnterRomButton")] private Button enterRomButton;
        [InjectPath("CancelCalibrationButton")] private Button cancelCalibrationButton;
        [InjectPath("CloseCalibrationButton")] private Button closeCalibrationButton;

        private Action continuation;
        private bool wearConfirmed;
        private bool zeroSucceeded;

        public override void OnInit()
        {
            base.OnInit();
            confirmWearButton.onClick.AddListener(OnConfirmWear);
            zeroUnloadingButton.onClick.AddListener(OnZeroUnloading);
            enterRomButton.onClick.AddListener(OnEnterRom);
            cancelCalibrationButton.onClick.AddListener(ClosePanel);
            closeCalibrationButton.onClick.AddListener(ClosePanel);
            deviceZeroConfirmedToggle.onValueChanged.AddListener(_ => RefreshButtons());
            SetIcon(calibrationIcon, "upperLimb");
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            continuation = args != null && args.Length > 0 ? args[0] as Action : null;
            wearConfirmed = false;
            zeroSucceeded = false;
            deviceZeroConfirmedToggle.SetIsOnWithoutNotify(false);
            patientText.text = Global.CurPatientInfo != null
                ? "当前患者：" + Global.CurPatientInfo.Name + "（" + Global.CurPatientInfo.CardNum + "）"
                : "当前患者：未选择";
            statusText.text = "等待佩戴确认";
            zeroUnloadingButton.interactable = false;
            RefreshButtons();
        }

        private void OnConfirmWear()
        {
            wearConfirmed = true;
            zeroUnloadingButton.interactable = true;
            statusText.text = "佩戴已确认，请执行减重归零";
            confirmWearButton.interactable = false;
        }

        private void OnZeroUnloading()
        {
            if (!wearConfirmed) return;
            DeviceCommandResult result = RehabDeviceCommands.ZeroUnloading();
            zeroSucceeded = result == DeviceCommandResult.Success;
            statusText.text = RehabDeviceCommands.GetResultMessage(result);
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool hasProof = zeroSucceeded || deviceZeroConfirmedToggle.isOn;
            enterRomButton.interactable = wearConfirmed && hasProof;
            if (wearConfirmed && hasProof) statusText.text = "标定确认完成，可进入上肢 ROM 评估";
        }

        private void OnEnterRom()
        {
            if (!wearConfirmed || (!zeroSucceeded && !deviceZeroConfirmedToggle.isOn)) return;
            Action afterRom = continuation;
            Main.m_UI.CloseUI<UICalibrationPanel>();
            UIMain main = Main.m_UI.GetOpenedUI<UIMain>();
            if (main != null) main.OpenModularWindow(typeof(UIAssessment), true, afterRom);
        }

        private void ClosePanel()
        {
            Main.m_UI.CloseUI<UICalibrationPanel>();
        }

        private static void SetIcon(Image image, string key)
        {
            if (image == null) return;
            Sprite sprite;
            if (Global.IconDic.TryGetValue(key, out sprite))
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
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
            confirmWearButton.onClick.RemoveAllListeners();
            zeroUnloadingButton.onClick.RemoveAllListeners();
            enterRomButton.onClick.RemoveAllListeners();
            cancelCalibrationButton.onClick.RemoveAllListeners();
            closeCalibrationButton.onClick.RemoveAllListeners();
            deviceZeroConfirmedToggle.onValueChanged.RemoveAllListeners();
            base.OnDestroy();
        }
    }
}
