#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace LB
{
    public static class WindowsReportPrinter
    {
        private const uint PRINTER_ENUM_LOCAL = 0x00000002;
        private const uint PRINTER_ENUM_CONNECTIONS = 0x00000004;
        private const int PD_NOPAGENUMS = 0x00000004;
        private const int PD_NOSELECTION = 0x00000008;
        private const int PD_USEDEVMODECOPIESANDCOLLATE = 0x00400000;
        private const int PD_RESULT_APPLY = 2;

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumPrinters(uint flags, string name, uint level, IntPtr printerEnum,
            uint bufferSize, ref uint needed, ref uint returned);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool PrintDlgEx([In, Out] PRINTDLGEX printDialog);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr memory);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetDefaultPrinter(StringBuilder printerName, ref int length);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDefaultPrinter(string printerName);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class PRINTDLGEX
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public int Flags2;
            public int ExclusionFlags;
            public IntPtr lpPageRanges;
            public int nPageRanges;
            public IntPtr lpMinPage;
            public IntPtr lpMaxPage;
            public int nCopies;
            public IntPtr hInstance;
            public string lpPrintTemplateName;
            public IntPtr lpCallback;
            public int nPropertyPages;
            public IntPtr lphPropertyPages;
            public int nStartPage;
            public int dwResultAction;
        }

        public static bool HasInstalledPrinters()
        {
            uint needed = 0;
            uint returned = 0;
            EnumPrinters(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS, null, 4, IntPtr.Zero, 0, ref needed, ref returned);
            return returned > 0 || needed > 0;
        }

        public static bool PrintDocx(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return false;

            if (!HasInstalledPrinters())
            {
                string selectedPrinter = ShowSystemPrintDialog();
                if (string.IsNullOrEmpty(selectedPrinter))
                {
                    OpenDocument(path);
                    return false;
                }

                string previousPrinter = GetDefaultPrinter();
                bool changed = SetDefaultPrinter(selectedPrinter);
                bool sent = PrintToDefaultPrinter(path);
                if (changed && !string.IsNullOrEmpty(previousPrinter)) SetDefaultPrinter(previousPrinter);
                return sent;
            }

            return PrintToDefaultPrinter(path);
        }

        private static bool PrintToDefaultPrinter(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    Verb = "print",
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                OpenDocument(path);
                return false;
            }
        }

        private static string ShowSystemPrintDialog()
        {
            PRINTDLGEX dialog = new PRINTDLGEX();
            dialog.lStructSize = Marshal.SizeOf(dialog);
            dialog.Flags = PD_NOPAGENUMS | PD_NOSELECTION | PD_USEDEVMODECOPIESANDCOLLATE;
            try
            {
                if (!PrintDlgEx(dialog) || dialog.dwResultAction != PD_RESULT_APPLY || dialog.hDevNames == IntPtr.Zero)
                    return null;

                int driverOffset = Marshal.ReadInt16(dialog.hDevNames, 0);
                int deviceOffset = Marshal.ReadInt16(dialog.hDevNames, 2);
                int outputOffset = Marshal.ReadInt16(dialog.hDevNames, 4);
                if (deviceOffset <= 0) return null;
                return Marshal.PtrToStringUni(IntPtr.Add(dialog.hDevNames, deviceOffset));
            }
            catch
            {
                return null;
            }
            finally
            {
                if (dialog.hDevNames != IntPtr.Zero) GlobalFree(dialog.hDevNames);
                if (dialog.hDevMode != IntPtr.Zero) GlobalFree(dialog.hDevMode);
            }
        }

        private static string GetDefaultPrinter()
        {
            int length = 0;
            GetDefaultPrinter(null, ref length);
            if (length <= 0) return null;
            StringBuilder name = new StringBuilder(length);
            return GetDefaultPrinter(name, ref length) ? name.ToString() : null;
        }

        public static void OpenDocument(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }
    }
}
#endif
