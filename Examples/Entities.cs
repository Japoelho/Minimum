using Minimum.DataAccess.Mapping;
using Minimum.XML.Mapping;
using System.Collections.Generic;
namespace Examples.Entities
{
    // - Used in Manual Mapping
    public class Address
    {
        public int AddressID { get; set; }
        public string Venue { get; set; }
        public string Number { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }

        public Address Address { get; set; }
    }

    // - Used in Auto Mapping

    [Table("tbAddresses")]
    public class AutoAddress
    {
        [Key] public int AddressID { get; set; }
        public string Venue { get; set; }
        public string Number { get; set; }
    }

    [Table("tbUsers")]
    public class AutoUser
    {
        [Key] public int UserID { get; set; }
        public string Name { get; set; }

        [Join(JoinType.InnerJoin)] public AutoAddress Address { get; set; }
    }

    // - Used in Custom Mapping

    [Table("tbUsers")]
    public class CustomUser
    {
        [Key] public int UserID { get; set; }
        public string Name { get; set; }

        [Join(JoinType.InnerJoin)] public CustomAddress Address { get; set; }

        public int TotalPoints { get; set; }
    }

    [Table("tbAddresses")]
    public class CustomAddress
    {
        [Key] public int AddressID { get; set; }
        public string Venue { get; set; }
    }

    [Table("tbPoints")]
    public class CustomPoint
    {
        [Key] public int PointID { get; set; }
        public int UserID { get; set; }
        public int Value { get; set; }
    }

    // - Used in Proxy

    public class VirtualUser
    {
        public virtual int UserID { get; set; }
        public string Name { get; set; }
        
        public virtual void ExecuteFunction() 
        { }
    }

    // - Used in Lazy Loading
    
    public class LazyUser
    {
        [Key] public int UserID { get; set; }
        public string Name { get; set; }

        [Join, Lazy] public virtual IList<LazyAddress> Addresses { get; set; }
        [Join, Lazy] public virtual LazyAddress MainAddress { get; set; }
    }

    public class LazyAddress
    {
        [Key] public int AddressID { get; set; }
        public string Venue { get; set; }
    }

    // - Used in Serialization

    [Node("MySerializedUser")]
    public class SerializedUser
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string[] LastNames { get; set; }

        public SerializedAddress MainAddress { get; set; }

        public SerializedUserAddressList Addresses { get; set; }

        public SerializedUser()
        {
            MainAddress = new SerializedAddress();
            Addresses = new SerializedUserAddressList();
            Addresses.Address = new List<SerializedAddress>();
        }
    }

    public class SerializedUserAddressList
    {
        public IList<SerializedAddress> Address { get; set; }
    }

    public class SerializedAddress
    {
        public int AddressID { get; set; }
        public string Venue { get; set; }
    }
}