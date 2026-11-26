using System;
using HT.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace LB
{
    /// <summary>
    /// 全局
    /// </summary>
    public static class Global
    {

        //输入管理密码间隔时间
        public const int EnterPassIntervalSecond = 120;
        //上一次输入管理密码时间
        public static DateTime LastEnterPassTime = DateTime.MinValue;

        //存档保存目录
        public static string SavePath
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return Path.Combine(FantomLib.AndroidPlugin.GetExternalStorageDirectory(),".LVBAIZHINENG_SZOT3");
#else
                return Application.persistentDataPath;
#endif
            }
        }

        //导出打印截图文件等保存目录
        public static string TempPath = Path.Combine(Global.SavePath, "Temp");
        //配置文件读取路径
        public static string ConfigPath = Path.Combine(Global.SavePath, "Config");
        public static string HospitalLogoPath = Path.Combine(Global.SavePath, "hospitalLogo.png");
        //IconCollectionData字典
        public static Dictionary<string, Sprite> IconDic = new Dictionary<string, Sprite>();

        public static string EvalDataGroupPath = "evaluationSetting.json";

        public static string IniParserPath = "game.txt";

        public static bool IsQuickLogin = false;
        //当前医院信息
        public static HospitalInfo CurHospitalInfo
        {
            get;
            set;
        }
        //当前管理者
        public static AdminInfo CurAdminInfo
        {
            get;
            set;
        }
        //当前患者
        public static PatientInfo CurPatientInfo
        {
            get;
            set;
        }

        public static void Reset()
        {
            IsQuickLogin = false;
            CurAdminInfo = null;
            CurPatientInfo = null;
            LastEnterPassTime = DateTime.MinValue;
        }
    }
}
