using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// 系统键盘管理器（仅支持 Windows）
/// 自动检测输入框点击，弹出/关闭屏幕键盘 (osk.exe)
/// 并解决输入框全选问题
/// </summary>
namespace LB
{
    public class SystemKeyboardManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("点击输入框时是否自动启动 OSK")]
        public bool autoLaunch = true;

        [Tooltip("点击非输入区域时是否自动关闭 OSK")]
        public bool autoClose = true;

        [Tooltip("OSK 启动后延迟恢复 Unity 焦点的秒数（建议 ≥0.3）")]
        public float focusRestoreDelay = 0.3f;

        [Tooltip("是否在输入框失去焦点时关闭 OSK（如点击其他 UI）")]
        public bool closeOnDeselect = true;

        private Process oskProcess;
        private IntPtr unityWindowHandle = IntPtr.Zero;
        private GameObject lastSelectedObject;
        private bool isOskRunning = false;

        // 当前选中的输入框引用（用于清除全选）
        private GameObject currentInputFieldObject;
        private TMP_InputField currentTmpInput;
        private InputField currentLegacyInput;

        public static SystemKeyboardManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (Application.platform != RuntimePlatform.WindowsEditor &&
                Application.platform != RuntimePlatform.WindowsPlayer)
            {
                Debug.LogWarning("SystemKeyboardManager 仅支持 Windows 平台，脚本已禁用。");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            unityWindowHandle = GetUnityWindowHandle();
            if (unityWindowHandle == IntPtr.Zero)
                Debug.LogWarning("无法获取 Unity 窗口句柄，焦点切换可能失效。");
            CloseOSK();
        }

        private void Update()
        {
            if (!autoLaunch) return;

            // 只在鼠标左键抬起或触摸结束时检测
            bool mouseUp = Input.GetMouseButtonUp(0);
            bool touchEnded = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
            if (!mouseUp && !touchEnded) return;

            GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
            if (currentSelected == lastSelectedObject)
                return;

            lastSelectedObject = currentSelected;

            if (currentSelected == null)
            {
                if (autoClose && isOskRunning)
                    CloseOSK();
                return;
            }

            // 检查是否为输入框
            TMP_InputField tmp = currentSelected.GetComponent<TMP_InputField>();
            InputField legacy = currentSelected.GetComponent<InputField>();

            if (tmp != null)
            {
                currentInputFieldObject = currentSelected;
                currentTmpInput = tmp;
                currentLegacyInput = null;
                LaunchOSK();
            }
            else if (legacy != null)
            {
                currentInputFieldObject = currentSelected;
                currentLegacyInput = legacy;
                currentTmpInput = null;
                LaunchOSK();
            }
            else
            {
                // 点击其他 UI
                if (closeOnDeselect && isOskRunning)
                    CloseOSK();
            }
        }

        // -------------------------------------------------------------
        // 公共方法
        // -------------------------------------------------------------

        public void LaunchOSK()
        {
            if (!IsWindowsPlatform())
            {
                Debug.LogWarning("非 Windows 平台，无法启动 OSK");
                return;
            }

            if (oskProcess != null && !oskProcess.HasExited)
            {
                BringOSKToFront();
                RestoreUnityFocus();
                return;
            }

            try
            {
                oskProcess = Process.Start("osk.exe");
                if (oskProcess == null)
                {
                    Debug.LogError("启动 OSK 失败，进程为空");
                    return;
                }

                isOskRunning = true;

                // 延迟恢复焦点并清除输入框全选
                if (focusRestoreDelay > 0)
                {
                    Invoke(nameof(RestoreUnityFocus), focusRestoreDelay);
                }
                else
                {
                    RestoreUnityFocus();
                }

                Debug.Log("OSK 启动成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"启动 OSK 异常: {ex.Message}");
            }
        }

        public void CloseOSK()
        {
            if (oskProcess != null && !oskProcess.HasExited)
            {
                try
                {
                    oskProcess.Kill();
                    oskProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"关闭 OSK 异常: {ex.Message}");
                }
                oskProcess = null;
                isOskRunning = false;
                CancelInvoke(nameof(RestoreUnityFocus));
                Debug.Log("OSK 已关闭");
            }
            else
            {
                isOskRunning = false;
            }
        }

        public void ToggleOSK()
        {
            if (isOskRunning)
                CloseOSK();
            else
                LaunchOSK();
        }

        public bool IsOSKRunning() => isOskRunning;

        // -------------------------------------------------------------
        // 内部辅助方法
        // -------------------------------------------------------------

        private void BringOSKToFront()
        {
            if (oskProcess != null && !oskProcess.HasExited)
            {
                try
                {
                    IntPtr handle = oskProcess.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                        WindowsAPI.SetForegroundWindow(handle);
                    else
                        Debug.LogWarning("OSK 窗口句柄无效，无法置前");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"将 OSK 置前失败: {ex.Message}");
                }
            }
        }

        private void RestoreUnityFocus()
        {
            if (unityWindowHandle != IntPtr.Zero)
            {
                bool success = WindowsAPI.SetForegroundWindow(unityWindowHandle);
                if (!success)
                    Debug.LogWarning("恢复 Unity 焦点失败，可能被系统阻止。");
            }
            else
            {
                Debug.LogWarning("Unity 窗口句柄无效，无法恢复焦点。");
            }
        }

        private IntPtr GetUnityWindowHandle()
        {
            IntPtr handle = WindowsAPI.GetActiveWindow();
            if (handle == IntPtr.Zero)
                handle = WindowsAPI.GetForegroundWindow();
            return handle;
        }

        private bool IsWindowsPlatform()
        {
            return Application.platform == RuntimePlatform.WindowsEditor ||
                   Application.platform == RuntimePlatform.WindowsPlayer;
        }

        private void OnDestroy()
        {
            CloseOSK();
            if (Instance == this)
                Instance = null;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // 可选：不处理
        }
    }

    public static class WindowsAPI
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
    }
}