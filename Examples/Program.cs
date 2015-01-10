using System;
using Minimum.DataAccess.Mapping;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    #region [ Entities ]
    [Table("tbUsers")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string Name { get; set; }
        [Column("UserStatusID")]
        public UserStatus Status { get; set; }
        public DateTime RegisterDate { get; set; }
        [Join]
        public Address Address { get; set; }
    }

    public enum UserStatus
    { Active, Blocked }

    [Table("tbAddresses")]
    public class Address
    {
        [Key]
        public int AddressID { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
    }
    #endregion
}
