using System.Collections.Generic;
using HT.Framework;
using UnityEngine;

namespace LB
{
    [CustomModule("TrainModel", true)]
    public class TrainModel : CustomModuleBase
    {
        private static TrainModel _instance;
        public static TrainModel Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["TrainModel"].Cast<TrainModel>();
                return _instance;
            }
        }
        public Dictionary<string, string> extraData = new Dictionary<string, string>();
        public override void OnInit()
        {
            base.OnInit();
        }
        public Vector3 GetValue()
        {
            if (SerialPortModule.Instance.IsConnected())
            {
                var values = GetValueFromEvalAxis();
                return values;
            }
            else
            {
                return Vector3.zero;
            }
        }
        public Vector3 GetValueFromEvalAxis()
        {
            if (!SerialPortModule.Instance.IsConnected())
                return Vector3.zero;

            var data = ConfigDataManager.Instance.evalData;
            var valueAxis = data.valueAxis;

            Vector3 values = GetValues();
            Vector3 retval = values.Reorder(valueAxis.ToString());
            return retval;
        }
        public Vector3 GetValues()
        {
            Vector3 retval = Vector3.zero;
            if (SerialPortModule.Instance.IsConnected())
            {
                retval.x = GetValue(0);
                retval.y = GetValue(1);
                retval.z = GetValue(2);
            }

            return retval;
        }
        public float GetValue(int index)
        {
            if (SerialPortModule.Instance.Message == null)
                return 0f;

            return SerialPortModule.Instance.Message.GetValue(index);
        }
    }
}
