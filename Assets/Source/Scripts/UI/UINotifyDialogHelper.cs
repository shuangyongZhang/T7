

using System;
using System.Collections.Generic;
using HT.Framework;
using UIWidgets;
using UnityEngine;
using UnityEngine.Serialization;

namespace LB
{
    public class UINotifyDialogHelper : SingletonBehaviourBase<UINotifyDialogHelper>
    {
        // /// <summary>
        // /// DatePicker template.
        // /// </summary>
        // [SerializeField]
        // [FormerlySerializedAs("pickerTemplate")]
        // protected DatePicker PickerTemplate;
        /// <summary>
        /// Auto-hide notification template.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("NotifyAutoHide")]
        protected Notify NotifyAutoHideTemplate;
        /// <summary>
        /// 普通对话框模板
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("普通对话框模板")]
        protected Dialog DialogTemplate;
        /// <summary>
        /// 异常对话框模板
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("异常对话框模板")]
        protected Dialog ExceptionDialogTemplate;

        // private PrefabInfo notifyPrefab = new PrefabInfo("ui", "NotifyTemplateAutoHide", "Prefabs/UI/Custom/NotifyTemplateAutoHide");

        private void Start()
        {

        }
        private Action<string> dateCallback;

        public void ShowDatePicker(DateTime defaultDate, Action<string> callback)
        {
            dateCallback = callback;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // DatePicker picker = PickerTemplate.Clone();
            // // show picker
            // picker.Show(defaultDate, OnPickerValueSelected, OnPickerValueCanceled);

#elif UNITY_ANDROID

            
            FantomLib.AndroidPlugin.ShowDatePickerDialog(defaultDate.ToString("yyyy/M/d"), "yyyy/M/d", new FantomLib.AndroidDatePickerDialogCallback((dateTime)=>{
                dateCallback?.Invoke(dateTime);
            }));
            // Pickers datePicker = new Pickers();
            // datePicker.pickDate(today.Year, today.Month - 1, today.Day, OnReceiveAdmission);

#endif
        }

        public void ShowSingleChoiceDialog(string title, string[] items, int checkedItem)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID

            FantomLib.AndroidPlugin.ShowSingleChoiceDialog(title, items,checkedItem, new FantomLib.AndroidSingleChoiceDialogCallback((res)=>{
                
            },(res)=>{
               
            }),true,"确定","取消");
#endif
        }

        void OnPickerValueSelected(DateTime value)
        {
            dateCallback?.Invoke(value.ToString("yyyy/M/d"));
        }

        void OnPickerValueCanceled()
        {

        }

        /// <summary>
        /// Show notify and close after 3 seconds.
        /// </summary>
        public void ShowNotifyAutoHide(string messageText)
        {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            NotifyAutoHideTemplate.Clone().Show(messageText, customHideDelay: 3f);
#elif UNITY_ANDROID
            FantomLib.AndroidPlugin.ShowToast(messageText);
#endif

        }

        public Dialog ShowExceptionDialog(string titleText, string messageText, string confirmText = null, string cancelText = null, Func<int, bool> onConfirm = null, Func<int, bool> onCancel = null, bool cancelable = true)
        {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var dialogButtons = new List<DialogButton>();
            if (!string.IsNullOrEmpty(confirmText)) dialogButtons.Add(new DialogButton(confirmText, onConfirm != null ? onConfirm : Dialog.AlwaysClose));
            if (!string.IsNullOrEmpty(cancelText)) dialogButtons.Add(new DialogButton(cancelText, onCancel != null ? onCancel : Dialog.AlwaysClose));
            var dialog = ExceptionDialogTemplate.Clone();
            dialog.Show(
                title: titleText,
                message: messageText,
                buttons: dialogButtons,
                focusButton: "确定",
                modal: true,
                modalColor: new Color(0f, 0f, 0f, 0.7f)
                );
            return dialog;
#elif UNITY_ANDROID
            FantomLib.AndroidPlugin.ShowDialog(titleText, messageText, new FantomLib.AndroidDialogCallback(() =>
                    {
                        if (onConfirm != null) onConfirm(0);
                    },() =>
                    {
                        if (onCancel != null) onCancel(0);
                    }), confirmText, cancelText, cancelable);
            return null;
#endif

        }
        public Dialog ShowPromptDialog(string titleText, string messageText, string confirmText = null, string cancelText = null, Func<int, bool> onConfirm = null, Func<int, bool> onCancel = null, bool cancelable = true)
        {

            // #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var dialogButtons = new List<DialogButton>();
            if (!string.IsNullOrEmpty(confirmText)) dialogButtons.Add(new DialogButton(confirmText, onConfirm != null ? onConfirm : Dialog.AlwaysClose));
            if (!string.IsNullOrEmpty(cancelText)) dialogButtons.Add(new DialogButton(cancelText, onCancel != null ? onCancel : Dialog.AlwaysClose));
            var dialog = DialogTemplate.Clone();
            dialog.Show(
                title: titleText,
                message: messageText,
                buttons: dialogButtons,
                focusButton: "确定",
                modal: true,
                modalColor: new Color(0f, 0f, 0f, 0.7f)
                );
            return dialog;
            // #elif UNITY_ANDROID
            // FantomLib.AndroidPlugin.ShowDialog(titleText, messageText, new FantomLib.AndroidDialogCallback(() =>
            //         {
            //             if (onConfirm != null) onConfirm(0);
            //         }, () =>
            //         {
            //             if (onCancel != null) onCancel(0);
            //         }), confirmText, cancelable);
            // return null;
            // #endif

        }

    }
}