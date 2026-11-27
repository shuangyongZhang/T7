using System;
using System.ComponentModel;
using SimpleSQL;


namespace LB
{
    public enum Sex
    {
        [Description("女")]
        Female = 0,
        [Description("男")]
        Male,
    }

    public enum AffectedSide
    {
        [Description("左侧")]
        LEFT = 0,
        [Description("右侧")]
        RIGHT,
        [Description("左右侧")]
        LEFTRIGHT,
    }


    public class PatientInfo
    {
        [PrimaryKey, MaxLength(32)]
        public string ID { get; set; }

        [MaxLength(16), NotNull]
        public string Name { get; set; }
        [NotNull]
        public Sex Sex { get; set; }
        // //出生日期
        // [NotNull]
        // public DateTime BirthDate { get; set; }
        //年龄
        [NotNull]
        public int Age { get; set; }

        //身高
        [NotNull]
        public int Stature { get; set; }

        //体重
        [NotNull]
        public int Weight { get; set; }

        //患侧
        [NotNull]
        public AffectedSide AffectedSide { get; set; }

        [NotNull]
        [MaxLength(32)]
        public string CardNum { get; set; }

        //入院日期
        public DateTime AdmissionDate { get; set; }
        //医院
        // [MaxLength(32), NotNull] 
        // public string Hospital  { get; set; }	
        //医师
        [MaxLength(16), NotNull]
        public string Doctor { get; set; }
        //医师诊断
        [MaxLength(256)]
        public string Diagnose { get; set; }

        [NotNull]
        public bool IsCalibrated { get; set; }
        public DateTime CalibrationTime { get; set; }

        public DateTime CreateTime { get; set; }

        public PatientInfo Clone()
        {
            return (PatientInfo)MemberwiseClone();
        }
    }
}