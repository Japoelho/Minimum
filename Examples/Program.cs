using System;
using Minimum.DataAccess.Mapping;
using System.Drawing;
using System.Collections.Generic;
using Minimum.DataAccess;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            DataAccess dataAccessLayer = new DataAccess("ConnectionName_InTheConfigFile");
            
            User oneUser = dataAccessLayer.GetByID<User>(1);
            // - Produces:
            //  SELECT 
            //      T0.UserID AS T0UserID, T0.Name AS T0Name, T0.UserStatusID AS T0UserStatusID, T0.RegisterDate AS T0RegisterDate, 
            //      T0.InfoID AS T0InfoID, T0.AddressID AS T0AddressID, 
            //      T1.AddressID AS T1AddressID, T1.Description AS T1Description, T1.Number AS T1Number, T1.StateID AS T1StateID, 
            //      T2.StateID AS T2StateID, T2.Name AS T2Name, T2.CountryID AS T2CountryID, 
            //      T3.CountryID AS T3CountryID, T3.Name AS T3Name 
            //  FROM tbUsers AS T0 
            //  INNER JOIN tbAddresses AS T1 ON T1.AddressID = T0.AddressID 
            //  INNER JOIN tbStates AS T2 ON T2.StateID = T1.StateID 
            //  INNER JOIN tbCountries AS T3 ON T3.CountryID = T2.CountryID 
            //  WHERE 1 = 1 AND (T0.UserID = 1)
            // - Note the "T0.InfoID" and "T0.AddressID", even if not explicity declared, they're loaded since there's a class that joins on that criteria.

            IList<User> allUsers = dataAccessLayer.Get<User>();
            IList<User> someUsers = dataAccessLayer.GetBy<User>(u => u.Status == UserStatus.Active && u.UserID == 1);
            IList<User> moreUsers = dataAccessLayer.GetBy<User>(Criteria.EqualTo("Status", UserStatus.Active), Criteria.EqualTo("UserID", 1));

            User aUser = new User();
            aUser.Name = "Myself";
            dataAccessLayer.Add<User>(aUser);

            aUser.Name = "Oneself";
            dataAccessLayer.Set<User>(aUser);
        }
    }

    #region [ Entities ]
    // - "Table" is the name of the table in the database that this class represents.
    // In case of inheritance (not interface implementation), you must declare a "Join" with the criteria used for joining. Multiple joins are allowed.
    [Table("tbUsers")]
    public class User
    {
        // - "Key" is the unique identity of this class/table. All classes are required to have this unique identifier.
        [Key] public int UserID { get; set; }
        
        // - This will be assumed that there is a column "Name" in the table "tbUsers".
        public string Name { get; set; }
        
        // - "Column" can be used for setting a different property name than the name in the database.
        [Column("UserStatusID")] public UserStatus Status { get; set; }
        
        // - All ValueTypes are valid.
        public DateTime RegisterDate { get; set; }

        // - Classes as properties must be set to "Join" or nothing will be done.
        // "Join" without parameters and a non-collection class will assume that this table "tbUsers" has a column "AddressID" (the Address' class Key) as well, and will join on that criteria.
        [Join] public virtual UserInfo Info { get; set; }

        // - Collections will always be fetched via lazy-loading/proxies, regardless of the settings.
        // "Join" without parameters and a collection class will assume that this collection's type (Address) table (tbAddresses) has a column "UserID" (this class' key).
        [Join] public virtual IList<Address> Addresses { get; set; }

        // - "Join" by default will always use lazy-loading/proxies on classes, unless specified otherwise. Note that the property doesn't need to be marked as virtual since it won't be used as proxy.
        [Join(JoinType.InnerJoin)] public Address MainAddress { get; set; }                
    }

    public enum UserStatus
    { Active, Blocked }

    [Table("tbUserInfo")]
    public class UserInfo
    {
        [Key] public int InfoID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    [Table("tbAddresses")]
    public class Address
    {
        [Key] public int AddressID { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        [Join(JoinType.InnerJoin)] public State State { get; set; }
    }

    [Table("tbStates")]
    public class State
    {
        [Key] public int StateID { get; set; }
        public string Name { get; set; }
        [Join(JoinType.InnerJoin)] public Country Country { get; set; }
    }

    [Table("tbCountries")]
    public class Country
    {
        [Key] public int CountryID { get; set; }
        public string Name { get; set; }
    }
    #endregion
}
