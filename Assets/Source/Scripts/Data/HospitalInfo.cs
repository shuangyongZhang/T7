namespace LB
{
    using System;
    using SimpleSQL;


    public class HospitalInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Dept { get; set; }
        public string Code { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        public string Desc { get; set; }
        public DateTime ModifyTime { get; set; }
        [NotNull]
        public DateTime CreateTime { get; set; }
        public HospitalInfo Clone()
        {
            return (HospitalInfo)MemberwiseClone();
        }

    }
}