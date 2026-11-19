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
            try
            {
                if (dbManager != null)
                {
                    dbManager.Delete<PatientInfo>(patientInfo);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
        }
        public bool DeletePatientInfoByID(string patientID)
        {
            try
            {
                if (dbManager != null)
                {
                    dbManager.Execute("DELETE FROM PatientInfo WHERE ID = ?", patientID);
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
                return false;
            }
            return true;
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




    }
}
