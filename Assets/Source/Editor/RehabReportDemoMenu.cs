using UnityEditor;
using UnityEngine;
using HT.Framework;

namespace LB
{
    public static class RehabReportDemoMenu
    {
        [MenuItem("Tools/T7/报告演示数据/为当前患者生成演示报告")]
        public static void Generate()
        {
            if (!Application.isPlaying || Global.CurPatientInfo == null)
            {
                EditorUtility.DisplayDialog("生成演示报告", "请先进入 Play 模式并选择患者。", "确定");
                return;
            }

            if (RehabReportDemoSeeder.GenerateForCurrentPatient())
            {
                RefreshReportUI();
                EditorUtility.DisplayDialog("生成演示报告", "已为当前患者生成演示报告。", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("生成演示报告", "生成失败，请查看 Console。", "确定");
            }
        }

        [MenuItem("Tools/T7/报告演示数据/清除当前患者演示报告")]
        public static void Clear()
        {
            if (!Application.isPlaying || Global.CurPatientInfo == null)
            {
                EditorUtility.DisplayDialog("清除演示报告", "请先进入 Play 模式并选择患者。", "确定");
                return;
            }

            RehabReportDemoSeeder.ClearForCurrentPatient();
            RefreshReportUI();
            EditorUtility.DisplayDialog("清除演示报告", "当前患者的演示报告已清理。", "确定");
        }

        private static void RefreshReportUI()
        {
            UIReport reportUI = Main.m_UI.GetOpenedUI<UIReport>();
            if (reportUI != null) reportUI.RefreshFromDatabase();
        }
    }
}
