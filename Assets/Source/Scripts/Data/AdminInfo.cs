using System;
using System.ComponentModel;
using SimpleSQL;

namespace LB
{
    public enum Role
    {
        [Description("操作员")]
        Operator = 0,
        [Description("管理员")]
        Administrator,

    }


    public class AdminInfo
    {

        // [PrimaryKey, AutoIncrement]
        // public int ID { get; set; }
        [PrimaryKey, MaxLength(32), NotNull]
        public string Account { get; set; }

        [MaxLength(32), NotNull]
        public string Password { get; set; }

        [NotNull]
        public Role Role { get; set; }
        [MaxLength(16)]
        public string Name { get; set; }
        public Sex Sex { get; set; }
        [MaxLength(32)]
        public string Dept { get; set; }

        [MaxLength(11)]
        public string Telephone { get; set; }

        [NotNull]
        public bool IsForbidden { get; set; }
        [NotNull]
        public string Creator { get; set; }
        [NotNull]
        public DateTime CreateTime { get; set; }
        public AdminInfo Clone()
        {
            return (AdminInfo)MemberwiseClone();
        }

    }
}