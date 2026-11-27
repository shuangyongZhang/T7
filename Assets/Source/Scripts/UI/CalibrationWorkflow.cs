using System;
using Cysharp.Threading.Tasks;
using HT.Framework;

namespace LB
{
    public static class CalibrationWorkflow
    {
        public static bool MarkCalibrated(PatientInfo patient)
        {
            if (patient == null) return false;
            bool wasCalibrated = patient.IsCalibrated;
            DateTime previousCalibrationTime = patient.CalibrationTime;
            patient.IsCalibrated = true;
            patient.CalibrationTime = DateTime.Now;
            if (!DBManagerModule.Instance.UpdatePatientInfo(patient))
            {
                patient.IsCalibrated = wasCalibrated;
                patient.CalibrationTime = previousCalibrationTime;
                return false;
            }
            PatientInfo saved = DBManagerModule.Instance.QueryPatientInfoByID(patient.ID);
            if (saved == null || !saved.IsCalibrated)
            {
                patient.IsCalibrated = wasCalibrated;
                patient.CalibrationTime = previousCalibrationTime;
                return false;
            }
            patient.CalibrationTime = saved.CalibrationTime;
            Global.CurPatientInfo = patient;
            Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventPatientChanged>().Fill(patient));
            return true;
        }
    }
}
