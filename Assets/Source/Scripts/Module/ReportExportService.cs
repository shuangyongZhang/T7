using System;
using System.IO;
using HT.Framework;
using UnityEngine;

namespace LB
{
    public static class ReportExportService
    {
        public static bool ExportWord(RehabReport report)
        {
            if (report == null) return false;
            try
            {
                ReportDocumentModel model = ReportDocumentService.Build(report);
                string tempPath = CreateTempDocx(model);
#if UNITY_ANDROID && !UNITY_EDITOR
                NativeFilePicker.ExportFile(tempPath, success =>
                {
                    Log.Info("[ReportExportService] Word 导出" + (success ? "成功" : "取消") + "：" + tempPath);
                });
#else
                string savePath;
                if (!FileDialogHelper.SaveDialog("保存报告", tempPath, "Word 文档|*.docx", out savePath))
                {
                    Log.Info("[ReportExportService] 用户取消 Word 导出。");
                    return false;
                }
                File.Copy(tempPath, savePath, true);
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("Word 报告已导出");
                Log.Info("[ReportExportService] Word 已导出：" + savePath);
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.Error("[ReportExportService] Word 导出失败：" + e.Message + "\n" + e.StackTrace);
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("Word 导出失败，请查看日志");
                return false;
            }
        }

        public static bool Print(RehabReport report)
        {
            if (report == null) return false;
            try
            {
                ReportDocumentModel model = ReportDocumentService.Build(report);
#if UNITY_ANDROID && !UNITY_EDITOR
                string html = ReportHtmlBuilder.Build(model);
                string jobName = "T7_" + model.ReportNumber;
                if (!AndroidReportPrinter.PrintHtml(html, jobName))
                {
                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("当前设备不支持系统打印");
                    return false;
                }
                Log.Info("[ReportExportService] 已打开 Android 系统打印界面：" + jobName);
#else
                string tempPath = CreateTempDocx(model);
                bool sent = WindowsReportPrinter.PrintDocx(tempPath);
                if (!sent)
                {
                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("未检测到可用打印机，已打开报告，请选择打印机");
                    return false;
                }
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("报告已发送到打印服务");
                Log.Info("[ReportExportService] Windows 打印已提交：" + tempPath);
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.Error("[ReportExportService] 打印失败：" + e.Message + "\n" + e.StackTrace);
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("打印失败，请查看日志");
                return false;
            }
        }

        private static string CreateTempDocx(ReportDocumentModel model)
        {
            Directory.CreateDirectory(Global.TempPath);
            string patientName = model.Patient != null ? model.Patient.Name : "患者";
            string fileName = SanitizeFileName(patientName + "_" + model.Title + "_" + model.Report.CreateTime.ToString("yyyyMMdd_HHmm") + ".docx");
            string path = Path.Combine(Global.TempPath, fileName);
            RehabReportWordExporter.Export(model, path);
            return path;
        }

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid) name = name.Replace(c, '_');
            return name.Replace(' ', '_');
        }
    }
}
