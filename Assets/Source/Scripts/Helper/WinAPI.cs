using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LB
{
    public static partial class WinAPI
    {
        static IntPtr ptr;
        public static IntPtr UnityHWnd
        {
            get
            {
                if (ptr == null || ptr == IntPtr.Zero)
                {
                    ptr = GetUnityWindow();
                }
                return ptr;
            }
        }

        #region const fields
        public const int OFN_READONLY = 0x1;
        public const int OFN_OVERWRITEPROMPT = 0x2;
        public const int OFN_HIDEREADONLY = 0x4;
        public const int OFN_NOCHANGEDIR = 0x8;
        public const int OFN_SHOWHELP = 0x10;
        public const int OFN_ENABLEHOOK = 0x20;
        public const int OFN_ENABLETEMPLATE = 0x40;
        public const int OFN_ENABLETEMPLATEHANDLE = 0x80;
        public const int OFN_NOVALIDATE = 0x100;
        public const int OFN_ALLOWMULTISELECT = 0x200;
        public const int OFN_EXTENSIONDIFFERENT = 0x400;
        public const int OFN_PATHMUSTEXIST = 0x800;
        public const int OFN_FILEMUSTEXIST = 0x1000;
        public const int OFN_CREATEPROMPT = 0x2000;
        public const int OFN_SHAREAWARE = 0x4000;
        public const int OFN_NOREADONLYRETURN = 0x8000;
        public const int OFN_NOTESTFILECREATE = 0x10000;
        public const int OFN_NONETWORKBUTTON = 0x20000;
        public const int OFN_NOLONGNAMES = 0x40000;
        public const int OFN_EXPLORER = 0x80000;
        public const int OFN_NODEREFERENCELINKS = 0x100000;
        public const int OFN_LONGNAMES = 0x200000;
        public const int OFN_ENABLEINCLUDENOTIFY = 0x400000;
        public const int OFN_ENABLESIZING = 0x800000;
        public const int OFN_DONTADDTORECENT = 0x2000000;
        public const int OFN_FORCESHOWHIDDEN = 0x10000000;
        public const int OFN_EX_NOPLACESBAR = 0x1;
        public const int OFN_SHAREFALLTHROUGH = 2;
        public const int OFN_SHARENOWARN = 1;
        public const int OFN_SHAREWARN = 0;
        // Name of the Unity window class used to find the window handle.
        public const string UNITY_WND_CLASSNAME = "UnityWndClass";
        #endregion

        #region Win32 Warpper
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClipCursor(ref RECT rcClip);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClipCursor(out RECT rcClip);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("user32.dll")]

        public static extern void mouse_event(MouseEventFlag dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(int vKey);
        public const int VK_MEDIA_STOP = 0xB2;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        //调用windows的系统锁定
        [DllImport("user32.dll")]
        public static extern void BlockInput(bool Block);

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int CommDlgExtendedError();


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        // 通过将每个窗口的句柄依次传递给应用程序定义的回调函数，枚举与线程关联的所有非子窗口。
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        //检索调用线程的线程标识符。
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        // 检索指定窗口所属的类的名称。
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        #endregion

        #region Functions
        public static IntPtr GetUnityWindow()
        {
            var unityHWnd = IntPtr.Zero;
            EnumThreadWindows(GetCurrentThreadId(), (hWnd, lParam) =>
            {
                var classText = new StringBuilder(UNITY_WND_CLASSNAME.Length + 1);
                GetClassName(hWnd, classText, classText.Capacity);

                if (classText.ToString() == UNITY_WND_CLASSNAME)
                {
                    unityHWnd = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            if (unityHWnd == IntPtr.Zero) //如果没找到窗口（非主线程中使用时会出现找不到unity窗口），使用 fallback 方案
            {
                unityHWnd = GetForegroundWindow();
            }
            return unityHWnd;
        }
        #endregion

        #region Assistant Structures
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public IntPtr filePtr;  //多选文件时不能用string或StringBuilder
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }

        [Flags]
        public enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        #endregion
    }
}