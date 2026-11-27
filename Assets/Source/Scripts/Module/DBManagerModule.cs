using System.Collections.Generic;
using UnityEngine;
using HT.Framework;
using System;
using SimpleSQL;

namespace LB
{
    /// <summary>
    /// 数据库模块
    /// </summary>
    [CustomModule("DBManagerModule", true)]
    public class DBManagerModule : CustomModuleBase
    {

        private static DBManagerModule _instance;
        public static DBManagerModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Main.m_CustomModule["DBManagerModule"].Cast<DBManagerModule>();
                }

                return _instance;
            }
        }

        private SimpleSQL.SimpleSQLManager dbManager;
        public class DB_VERSION
        {
            [PrimaryKey]
            public int ID { get; set; }
            public string Version { get; set; }
        }

        public class PatientColumnInfo
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            dbManager = GameObject.Find("DB Manager").GetComponent<SimpleSQL.SimpleSQLManager>();
            if (dbManager != null)
            {
                dbManager.Close();
                dbManager.Dispose();
            }

            // string sql = "ALTER TABLE \"AdminInfo\" ADD COLUMN \"IsForbidden\" BOOL";
            // dbManager.Execute(sql);

        }

        //数据库升级表结构
        private void UpgrageDB()
        {

            if (dbManager == null) return;
            DB_VERSION dbVer = new DB_VERSION();
            if (!CheckTableExist<DB_VERSION>())
            {
                dbManager.CreateTable<DB_VERSION>();
                dbVer = new DB_VERSION() { ID = 1, Version = "1.0" };
                dbManager.Insert(dbVer);

            }
            dbVer = dbManager.QueryFirstRecord<DB_VERSION>(out bool recordExists, "SELECT * FROM DB_VERSION");
            if (dbVer.Version == "1.0")
            {

            }
        }
        private bool CheckTableExist<T>()
        {

            string tableName = typeof(T).Name;

            string query = "SELECT * FROM sqlite_master WHERE type='table' AND name=?";

            dbManager.QueryFirstRecord(out bool recordExists, dbManager.GetMapping(typeof(T)), query, tableName);

            return recordExists;
        }
        private bool CheckColumnExist<T>(string columnName)
        {

            string tableName = typeof(T).Name;

            string query = string.Format("SELECT * FROM sqlite_master WHERE name='{0}' AND sql LIKE '%{1}%'", tableName, columnName);

            dbManager.QueryFirstRecord(out bool recordExists, dbManager.GetMapping(typeof(T)), query);

            return recordExists;
        }

        /// <summary>
        /// 模块准备工作
        /// </summary>
        public override void OnReady()
        {
            base.OnReady();
            //自动启动
            IsRunning = false;

        }
        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (dbManager != null)
            {
                dbManager.overridePathMode = SimpleSQLManager.OverridePathMode.Absolute;
                dbManager.overrideBasePath = Global.SavePath;
                dbManager.Initialize(true);
            }
#endif

            UpgrageDB();
            EnsurePatientCalibrationColumns();
            CreateTreatmentPlanTable();
            CreateRehabReportTable();
            CreateRehabReportSampleTable();
        }


        /// <summary>
        /// 刷新模块
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        /// <summary>
        /// 终结模块
        /// </summary>
        public override void OnTerminate()
        {
            base.OnTerminate();
        }
        /// <summary>
        /// 暂停模块
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();
        }
        /// <summary>
        /// 恢复模块
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
        }

        private void EnsurePatientCalibrationColumns()
        {
            if (dbManager == null) return;
            dbManager.BeginTransaction();
            try
            {
                List<PatientColumnInfo> columns = dbManager.Query<PatientColumnInfo>("PRAGMA table_info(PatientInfo)") ?? new List<PatientColumnInfo>();
                bool hasIsCalibrated = columns.Exists(c => c.Name == "IsCalibrated");
                bool hasCalibrationTime = columns.Exists(c => c.Name == "CalibrationTime");
                if (!hasIsCalibrated)
                    dbManager.Execute("ALTER TABLE \"PatientInfo\" ADD COLUMN \"IsCalibrated\" INTEGER NOT NULL DEFAULT 0");
                if (!hasCalibrationTime)
                    dbManager.Execute("ALTER TABLE \"PatientInfo\" ADD COLUMN \"CalibrationTime\" DATETIME");
                dbManager.Commit();
            }
            catch (Exception e)
            {
                dbManager.Rollback();
                Log.Info(e.Message);
            }
        }

        private void CreateTreatmentPlanTable()
        {
            if (dbManager != null) dbManager.CreateTable<TreatmentPlan>();
        }

        #region Hospital
        public HospitalInfo QueryHospitalInfo()
        {
            if (dbManager != null)
            {
                bool recordExists;
                var hospitalInfo = dbManager.QueryFirstRecord<HospitalInfo>(out recordExists, "SELECT * FROM HospitalInfo");
                return hospitalInfo;
            }
            return null;
        }

        public bool InsertHospitalInfo(HospitalInfo hospitalInfo)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Insert(hospitalInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }
        public bool UpdateHospitalInfo(HospitalInfo hospitalInfo)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.UpdateTable(hospitalInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region AdminInfo

        public void CreateAdminInfoTable()
        {
            if (dbManager != null)
            {
                dbManager.CreateTable<AdminInfo>();
            }
        }
        public List<AdminInfo> QueryAllAdminInfos()
        {
            try
            {
                if (dbManager != null)
                {
                    List<AdminInfo> adminInfos = dbManager.Query<AdminInfo>("SELECT * FROM AdminInfo");
                    return adminInfos;
                }
            }
            catch
            {

            }
            return null;
        }

        public AdminInfo QueryAdminInfoTableByName(string adminName)
        {
            if (dbManager != null)
            {
                bool recordExists;
                AdminInfo adminInfo = dbManager.QueryFirstRecord<AdminInfo>(out recordExists, "SELECT * FROM AdminInfo WHERE Account = ?", adminName);
                return adminInfo;
            }
            return null;
        }
        public bool InsertAdminInfo(AdminInfo adminInfo)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Insert(adminInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }
        public bool UpdateAdminInfo(AdminInfo adminInfo, string oldAccount = "")
        {
            try
            {
                if (dbManager != null)
                {
                    if (string.IsNullOrEmpty(oldAccount))
                    {
                        dbManager.UpdateTable(adminInfo);
                    }
                    else
                    {
                        dbManager.Execute("UPDATE AdminInfo SET Account = ?, Password = ?, Role = ?, Name = ?, Dept = ?, Telephone = ?, IsForbidden = ? WHERE Account = ?", adminInfo.Account, adminInfo.Password, adminInfo.Role, adminInfo.Name, adminInfo.Dept, adminInfo.Telephone, adminInfo.IsForbidden, oldAccount);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }

        public bool DeleteAdminInfo(string account)
        {
            if (dbManager == null || string.IsNullOrEmpty(account)) return false;
            try
            {
                dbManager.Execute("DELETE FROM AdminInfo WHERE Account = ?", account);
                return true;
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
        }
        #endregion

        #region PatientInfo

        public void CreatePatientInfoTable()
        {
            if (dbManager != null)
            {
                dbManager.CreateTable<PatientInfo>();
            }
        }

        public bool UpdatePatientsDoctor(string oldDoctor, string newDoctor)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Execute("UPDATE PatientInfo SET Doctor = ? WHERE Doctor = ?", newDoctor, oldDoctor);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }


        public bool InsertPatientInfo(PatientInfo patientInfo)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Insert(patientInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;

        }


        public bool UpdatePatientInfo(PatientInfo patientInfo)
        {
            try
            {
                if (dbManager != null)
                {
                    // 更新患者信息
                    dbManager.UpdateTable(patientInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;


        }

        public bool DeletePatientInfo(PatientInfo patientInfo)
        {
            return patientInfo != null && DeletePatientInfoWithReports(patientInfo.ID);
        }

        public bool DeletePatientInfoByID(string patientID)
        {
            return DeletePatientInfoWithReports(patientID);
        }

        private bool DeletePatientInfoWithReports(string patientID)
        {
            if (dbManager == null || string.IsNullOrEmpty(patientID)) return false;
            dbManager.BeginTransaction();
            try
            {
                dbManager.Execute("DELETE FROM RehabReportSample WHERE ReportID IN (SELECT ReportID FROM RehabReport WHERE PatientID = ?)", patientID);
                dbManager.Execute("DELETE FROM RehabReport WHERE PatientID = ?", patientID);
                dbManager.Execute("DELETE FROM PatientInfo WHERE ID = ?", patientID);
                dbManager.Commit();
                return true;
            }
            catch (Exception e)
            {
                dbManager.Rollback();
                Log.Info(e.Message);
                return false;
            }
        }
        public List<PatientInfo> SearchPatientInfo(string search)
        {
            try
            {
                if (dbManager != null)
                {
                    search = "%" + search + "%";
                    List<PatientInfo> patientInfos = dbManager.Query<PatientInfo>("SELECT * FROM PatientInfo WHERE Name LIKE ? OR ID LIKE ?", search, search);
                    return patientInfos;
                }
            }
            catch
            {

            }
            return null;
        }
        public List<PatientInfo> QueryAllPatientInfos(AdminInfo admin)
        {
            try
            {
                if (dbManager != null)
                {
                    List<PatientInfo> patientInfos;
                    if (admin.Role == Role.Administrator)
                    {
                        patientInfos = dbManager.Query<PatientInfo>("SELECT * FROM PatientInfo");
                    }
                    else
                    {
                        patientInfos = dbManager.Query<PatientInfo>("SELECT * FROM PatientInfo WHERE Doctor = ?", admin.Account);
                    }

                    return patientInfos;
                }
            }
            catch
            {

            }
            return null;
        }
        public PatientInfo QueryPatientInfoByID(string patientID)
        {
            try
            {
                if (dbManager != null)
                {
                    PatientInfo patientInfo = dbManager.QueryFirstRecord<PatientInfo>(out bool recordExists, "SELECT * FROM PatientInfo WHERE ID = ?", patientID);
                    if (recordExists)
                    {
                        return patientInfo;
                    }
                }
            }
            catch
            {

            }
            return null;
        }
        #endregion

        #region TreatmentPlan
        public List<TreatmentPlan> QueryAllTreatmentPlans()
        {
            try
            {
                if (dbManager != null)
                    return dbManager.Query<TreatmentPlan>("SELECT * FROM TreatmentPlan ORDER BY UpdateTime DESC, PlanID DESC");
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
            return null;
        }

        public bool InsertTreatmentPlan(TreatmentPlan plan)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Insert(plan);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
            return false;
        }

        public bool UpdateTreatmentPlan(TreatmentPlan plan)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.UpdateTable(plan);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
            return false;
        }

        public bool DeleteTreatmentPlan(int planID)
        {
            if (dbManager == null || planID <= 0) return false;
            try
            {
                dbManager.Execute("DELETE FROM TreatmentPlan WHERE PlanID = ?", planID);
                return true;
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
        }
        #endregion

        #region RehabReport
        public void CreateRehabReportTable()
        {
            if (dbManager != null)
            {
                dbManager.CreateTable<RehabReport>();
            }
        }

        public void CreateRehabReportSampleTable()
        {
            if (dbManager != null)
            {
                dbManager.CreateTable<RehabReportSample>();
            }
        }

        public bool InsertRehabReport(RehabReport report, IReadOnlyList<RehabSample> samples)
        {
            if (dbManager == null) return false;
            dbManager.BeginTransaction();
            try
            {
                long reportID;
                dbManager.Insert(report, out reportID);
                report.ReportID = (int)reportID;
                if (samples != null && samples.Count > 0)
                {
                    List<RehabReportSample> rows = new List<RehabReportSample>(samples.Count);
                    for (int i = 0; i < samples.Count; i++)
                    {
                        RehabSample sample = samples[i];
                        if (sample == null || !sample.IsValid) continue;
                        rows.Add(new RehabReportSample
                        {
                            ReportID = report.ReportID,
                            SampleIndex = i,
                            TimestampSeconds = sample.TimestampSeconds,
                            PositionX = sample.PositionMeters.x,
                            PositionY = sample.PositionMeters.y,
                            PositionZ = sample.PositionMeters.z,
                            JointAngleX = sample.JointAnglesDegrees.x,
                            JointAngleY = sample.JointAnglesDegrees.y,
                            JointAngleZ = sample.JointAnglesDegrees.z,
                            ForceX = sample.ForceNewtons.x,
                            ForceY = sample.ForceNewtons.y,
                            ForceZ = sample.ForceNewtons.z,
                            TorqueX = sample.TorqueNewtonMeters.x,
                            TorqueY = sample.TorqueNewtonMeters.y,
                            TorqueZ = sample.TorqueNewtonMeters.z,
                            CurrentAngleDegrees = sample.CurrentAngleDegrees,
                            SpeedMetersPerSecond = sample.SpeedMetersPerSecond
                        });
                    }
                    dbManager.InsertAll(rows);
                }
                dbManager.Commit();
                return true;
            }
            catch (Exception e)
            {
                dbManager.Rollback();
                Log.Info(e.Message);
                return false;
            }
        }

        public bool UpdateRehabReport(RehabReport report)
        {
            if (dbManager == null || report == null || report.ReportID <= 0) return false;
            try
            {
                dbManager.UpdateTable(report);
                return true;
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
        }

        public bool DeleteRehabReport(int reportID)
        {
            if (dbManager == null || reportID <= 0) return false;
            dbManager.BeginTransaction();
            try
            {
                dbManager.Execute("DELETE FROM RehabReportSample WHERE ReportID = ?", reportID);
                dbManager.Execute("DELETE FROM RehabReport WHERE ReportID = ?", reportID);
                dbManager.Commit();
                return true;
            }
            catch (Exception e)
            {
                dbManager.Rollback();
                Log.Info(e.Message);
                return false;
            }
        }

        public List<RehabReportSample> QueryRehabReportSamples(int reportID)
        {
            try
            {
                if (dbManager != null)
                {
                    return dbManager.Query<RehabReportSample>(
                        "SELECT * FROM RehabReportSample WHERE ReportID = ? ORDER BY SampleIndex ASC", reportID);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
            return null;
        }

        public List<RehabReport> QueryRehabReports(string patientID)
        {
            try
            {
                if (dbManager != null)
                {
                    return dbManager.Query<RehabReport>(
                        "SELECT * FROM RehabReport WHERE PatientID = ? ORDER BY CreateTime DESC, ReportID DESC", patientID);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
            return null;
        }
        #endregion


    }
}
