using System;
using System.Linq;
using System.Collections.Generic;
using Minimum.Connection;
using Minimum.DataAccess;
using Minimum.DataAccess.Mapping;
using Minimum.Proxy;
using Examples.Entities;
using Minimum;
using System.Xml.Linq;

namespace Examples
{
    class Example
    {
        static void Main(string[] args)
        {
            // - Example of how mapping is done manually, useful if you ever think of implementing your custom mapper with your conventions.
            QueryExample_ManualMapping();

            // - Example of a ready to use auto-mapper using [Attribute] annotations in classes.
            QueryExample_AutomaticMapping();

            // - Example of the SCRUD class using the default auto-mapper.
            DataAccessExample();

            // - Example of a custom query.
            QueryCustomExample();

            // - Example of a custom mapper.
            QueryCustomMapperExample();

            // - Example of a proxy/interceptor.
            ProxyExample();

            // - Example of lazy loading.
            LazyLoadingExample();

            // - Example of the Serialization class.
            SerializationExample();
        }

        static void QueryExample_ManualMapping()
        {   
            // - Manually mapping the class [Address].
            QueryMap queryMap = new QueryMap(typeof(Address));
            
            // - Setting the source values.
            queryMap.Database = "MyDatabase";
            queryMap.Schema = "dbo";
            queryMap.Table = "tbAddresses";

            // - Setting the values to be retrieved.
            queryMap.Identity("intAddressID", "AddressID"); //The Column [intAddressID] refers to the Property [AddressID] and is an identity value.
                        
            queryMap.Column("strVenue", "Venue"); //The Column [strVenue] refers to the Property [Venue].
            queryMap.Column("strNumber", "Number"); //The Column [strNumber] refers to the Property [Number].
            
            // - Creating a queryMap of another type to another type.
            QueryMap anotherMap = new QueryMap(typeof(User));

            anotherMap.Database = "MyDatabase";
            anotherMap.Schema = "dbo";
            anotherMap.Table = "tbUsers";

            anotherMap.Identity("UserID", "UserID");
            anotherMap.Column("Name", "Name");

            // - Joining this query addresses' query, and assigning it to the Property [Address] on the criteria [tbUsers.AddressID] = [tbAddresses.AddressID]
            anotherMap.Join(queryMap, "Address", JoinType.InnerJoin, new On("AddressID", "AddressID"));

            // - Mapping done! Now, for actual use, you need to instantiate a query, which will provide the aliases and database logic.
            
            // - Queries requires the connection info which will determine the database type and syntax. "MyConnectionName" is the name of the connection in your web.config or app.config file.
            SQLInfo sqlInfo = new SQLInfo("MyConnectionName");
            // - Or you can fill the connectionString value yourself.
            sqlInfo.ConnectionString = "Data Source = SQLInstance\\Name; User = admin; Password = admin";

            // - Creating a new query instance of type SQL using the map of [Users].
            Query query = new Query(sqlInfo, anotherMap);
            
            // - Now we use the query. This will generate the following query:
            //  SELECT 
            //      U0.UserID AS U0_UserID, 
            //      U0.Name AS U0_Name, 
            //      A1.intAddressID AS A1_intAddressID, 
            //      A1.strVenue AS A1_strVenue, 
            //      A1.strNumber AS A1_strNumber 
            //  FROM MyDatabase.dbo.tbUsers AS U0 
            //  INNER JOIN MyDatabase.dbo.tbAddresses AS A1 ON A1.AddressID = U0.AddressID            
            IList<User> userList = query.Select<User>();

            // - Adding a criteria.
            query.Where(Criteria.EqualTo("UserID", 1)); // WHERE UserID = 1

            User user = new User();
            user.Name = "Myself";
            user.UserID = 1;

            //  INSERT INTO MyDatabase.dbo.tbUsers 
            //      (Name) 
            //  VALUES 
            //      ('Myself') 
            //  SELECT @@IDENTITY AS ID
            query.Insert<User>(user);

            user.Name = "Himself";

            //  UPDATE U0 SET 
            //      Name = 'Himself' 
            //  FROM MyDatabase.dbo.tbUsers AS U0 
            //  INNER JOIN MyDatabase.dbo.tbAddresses AS A1 ON A1.AddressID = U0.AddressID
            //  WHERE 1 = 1 AND (U0.UserID = 1)
            query.Update<User>(user);

            //  DELETE FROM MyDatabase.dbo.tbUsers AS U0 
            //  WHERE 1 = 1 AND (U0.UserID = 1)
            query.Delete<User>(user);
        }

        static void QueryExample_AutomaticMapping()
        {
            // - Using the default automapper to map [Address] and [User] using Attributes.
            Mapper mapper = new Mapper();
            
            // - The automap maps recursively, so [Address] is automapped as well.
            QueryMap userMap = mapper.Map(typeof(AutoUser));

            Query query = new Query(new SQLInfo("MyConnectionName"), userMap);

            //  SELECT 
            //      U0.UserID AS U0_UserID, 
            //      U0.Name AS U0_Name, 
            //      A1.intAddressID AS A1_intAddressID, 
            //      A1.strVenue AS A1_strVenue, 
            //      A1.strNumber AS A1_strNumber 
            //  FROM MyDatabase.dbo.tbUsers AS U0 
            //  INNER JOIN MyDatabase.dbo.tbAddresses AS A1 ON A1.AddressID = U0.AddressID            
            IList<AutoUser> userList = query.Select<AutoUser>();

            // - Adding a criteria.
            query.Where(Criteria.EqualTo("UserID", 1)); // WHERE UserID = 1

            AutoUser user = new AutoUser();
            user.Name = "Myself";
            user.UserID = 1;

            //  INSERT INTO MyDatabase.dbo.tbUsers 
            //      (Name) 
            //  VALUES 
            //      ('Myself') 
            //  SELECT @@IDENTITY AS ID
            query.Insert<AutoUser>(user);

            user.Name = "Himself";

            //  UPDATE U0 SET 
            //      Name = 'Himself' 
            //  FROM MyDatabase.dbo.tbUsers AS U0 
            //  INNER JOIN MyDatabase.dbo.tbAddresses AS A1 ON A1.AddressID = U0.AddressID
            //  WHERE 1 = 1 AND (U0.UserID = 1)
            query.Update<AutoUser>(user);

            //  DELETE FROM MyDatabase.dbo.tbUsers AS U0 
            //  WHERE 1 = 1 AND (U0.UserID = 1)
            query.Delete<AutoUser>(user);
        }

        static void DataAccessExample()
        {
            DataAccess dataAccess = new DataAccess(new SQLInfo("MyConnectionName"));

            //  SELECT 
            //      A0.UserID AS A0_UserID, A0.Name AS A0_Name, A1.AddressID AS A1_AddressID, A1.Venue AS A1_Venue, A1.Number AS A1_Number 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID
            IList<AutoUser> getAll = dataAccess.Get<AutoUser>();

            //  SELECT 
            //      A0.UserID AS A0_UserID, A0.Name AS A0_Name, A1.AddressID AS A1_AddressID, A1.Venue AS A1_Venue, A1.Number AS A1_Number 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID 
            //  WHERE 1 = 1 AND (A0.Name = 'Myself')
            IList<AutoUser> getAllFromExpression = dataAccess.GetBy<AutoUser>(u => u.Name == "Myself");

            //  SELECT 
            //      A0.UserID AS A0_UserID, A0.Name AS A0_Name, A1.AddressID AS A1_AddressID, A1.Venue AS A1_Venue, A1.Number AS A1_Number 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID 
            //  WHERE 1 = 1 AND ((A0.Name = 'Myself') AND (A1.Venue = 'Home'))
            IList<AutoUser> getAllFromExpressions = dataAccess.GetBy<AutoUser>(u => u.Name == "Myself" && u.Address.Venue == "Home");

            //  SELECT 
            //      A0.UserID AS A0_UserID, A0.Name AS A0_Name, A1.AddressID AS A1_AddressID, A1.Venue AS A1_Venue, A1.Number AS A1_Number 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID 
            //  WHERE 1 = 1 AND (A0.Name = 'Myself') AND (A1.Venue = 'Home')
            IList<AutoUser> getAllFromCriterias = dataAccess.GetBy<AutoUser>(
                Criteria.EqualTo("Name", "Myself"),
                Criteria.EqualTo("Address.Venue", "Home")
            );

            //  SELECT 
            //      A0.UserID AS A0_UserID, A0.Name AS A0_Name, A1.AddressID AS A1_AddressID, A1.Venue AS A1_Venue, A1.Number AS A1_Number 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID 
            //  WHERE 1 = 1 AND (A0.UserID = 1)
            AutoUser getByIdentity = dataAccess.GetByID<AutoUser>(1);

            AutoUser user = new AutoUser();
            user.Name = "Me";
            user.UserID = 2;

            //  INSERT INTO tbUsers 
            //      (Name) 
            //  VALUES 
            //      ('Me') 
            //  SELECT @@IDENTITY AS ID
            dataAccess.Add<AutoUser>(user);

            //  UPDATE A0 SET 
            //      Name = 'Me' 
            //  FROM tbUsers AS A0 
            //  INNER JOIN tbAddresses AS A1 ON A1.AddressID = A0.AddressID 
            //  WHERE 1 = 1 AND (A0.UserID = 2)
            dataAccess.Set<AutoUser>(user);

            //  DELETE FROM tbUsers AS A0 
            //  WHERE 1 = 1 AND (A0.UserID = 2)
            dataAccess.Del<AutoUser>(user);
        }

        static void QueryCustomExample()
        {
            // - Mapping and creating the Query object.
            Mapper mapper = new Mapper();
            QueryMap customUser = mapper.Map(typeof(CustomUser));
            Query query = new Query(new SQLInfo("MyConnectionName"), customUser);

            // - Mapping a new type.
            QueryMap newMap = mapper.Map(typeof(CustomPoint));
            
            // - Joining the query with this type. It won't fetch any data unless specified, but the join query will be generated. This returns the joined subquery.
            Query joinedQuery = query.Join(newMap, JoinType.LeftJoin, new On("UserID", "UserID"));

            // - Setting the Property "TotalPoints" to be fetched from the column "PointID" of the subquery.
            query.Count("PointID", "TotalPoints", joinedQuery);

            //  SELECT 
            //      C0.UserID AS C0_UserID, 
            //      C0.Name AS C0_Name, 
            //      COUNT(C2.PointID) AS C2_PointID, 
            //      C1.AddressID AS C1_AddressID, 
            //      C1.Venue AS C1_Venue 
            //  FROM tbUsers AS C0 
            //  INNER JOIN tbAddresses AS C1 ON C1.AddressID = C0.AddressID 
            //  LEFT JOIN tbPoints AS C2 ON C2.UserID = C0.UserID 
            //  WHERE 1 = 1 
            //  GROUP BY C0.UserID, C0.Name, C1.AddressID, C1.Venue
            IList<CustomUser> userList = query.Select<CustomUser>();
            
            // - Note that the original mapping isn't changed, only this instance of the query. The original QueryMap is unchanged, so this can be customized for reports.
            // - You can join any query with another query, and use the [Query] reference in the column of another [Query].
        }

        static void QueryCustomMapperExample()
        {
            // - The default mapper is an example of a custom mapper.
            Mapper mapper = new Mapper();
            
            // - Read the comments of this "Map" function, it assumes a mapping convention and creates the [QueryMap].
            QueryMap userMap = mapper.Map(typeof(User));

            // - To create a custom, inherit from the IMapper interface...
            MyCustomMapper myMapper = new MyCustomMapper();

            // - And pass it to the DataAccess constructor.
            DataAccess dataAccess = new DataAccess(new SQLInfo("MyConnectionName"), myMapper);

            // - This will be using the [QueryMap] generated from your "Map(Type type)" function in your custom map.
            User user = dataAccess.GetByID<User>(1);
        }

        static void ProxyExample()
        {
            // - Proxies use a singleton pattern to store proxies for future requests.
            Proxies instance = Proxies.GetInstance();

            // - TODO: Load proxies from .DLL
            //Proxies.GetInstance().SaveDynamic();

            // - Generates the proxy of the type [VirtualUser]. The generated proxy will be called "VirtualUserProxy", and it implements the [IProxy] interface.
            IProxy proxy = instance.GetProxy(typeof(VirtualUser));

            // - But "proxy" also inherits from it's original type, so you can safely cast it back to [VirtualUser].
            VirtualUser proxyUser = (VirtualUser)proxy;

            // - By using a proxy, you can intercept any method marked as virtual to run another code before or after the original method.
            proxy.Add("get_UserID", (object pInstance, object[] pArgs) => 
            {
                // - This is a function to run before the actual method executes. "get_UserID" is the method name of the property "UserID" getter.
                // - pInstance is the instance of the class executing the code.
                // - pArgs are the function arguments, if any, in declaring order.
                // - If a 3rd parameter is set, the interceptor will execute after the original. The 3rd parameter is the return object, if any.
                // - Note that value types are passed by copy, not reference, so valut types won't be set permanently in the instance, only reference types will!
                // - To set a value type, call it's set accessor.

                System.Diagnostics.Debug.WriteLine("Hello from the Interceptor!");
                return null;
            }, Run.Once); //And this is how many times this intercepting should run, either once or always when the function is called.

            // - The intercepting function above will be called in this line.
            int callingInterceptor = proxyUser.UserID;

            // - Note that if the method marked to intercept is not marked as virtual, the override from the proxy will NOT be called.
        }

        static void LazyLoadingExample()
        {
            Mapper mapper = new Mapper();
            Query query = new Query(new SQLInfo("MyConnectionName"), mapper.Map(typeof(LazyUser)));

            //  SELECT 
            //      L0.UserID AS L0_UserID, L0.Name AS L0_Name 
            //  FROM LazyUser AS L0 
            //  WHERE 1 = 1 AND (L0.UserID = 1)
            LazyUser lazyUser = query.Where<LazyUser>(u => u.UserID == 1).Select<LazyUser>().FirstOrDefault();

            // - Note that "MainAddress" wasn't loaded. Collections are also set to Lazy by default in the default mapper.

            // - This will trigger the proxy to call the select query for this instance only at the time of accessing the property.
            LazyAddress mainAddress = lazyUser.MainAddress;

            // - Same goes for the list property. The query is only executed now.
            IList<LazyAddress> addresses = lazyUser.Addresses;
        }

        static void SerializationExample()
        {
            // - Creating a random class filled with data.
            SerializedUser sUser = new SerializedUser();

            sUser.UserID = 1;
            sUser.Name = "Myself";
            sUser.LastNames = new string[] { "With", "Pie" };
            
            sUser.MainAddress.AddressID = 1;
            sUser.MainAddress.Venue = "Home";

            sUser.Addresses.Address.Add(new SerializedAddress() { AddressID = 2, Venue = "Resort" });
            sUser.Addresses.Address.Add(new SerializedAddress() { AddressID = 3, Venue = "Hotel" });
            sUser.Addresses.Address.Add(new SerializedAddress() { AddressID = 4, Venue = "Uncle's" });

            // - Serializing the class to a XDocument.
            XDocument xDoc = Loader.XML.Load(sUser);

            // - Note that all nodes are named after the properties. This way, you can have 100% of control of the resulting XML only from your class declarations.
            string resultingXML = xDoc.ToString();

            // - And it returns back.
            SerializedUser againUser = Loader.XML.Load<SerializedUser>(xDoc.Root);

            // - Since it's the exact representation of the class structure, it's easier to control the resulting XML from your class, even when you're requested some bizarre XML structure.
            // - From my tests it's also much faster than the default XmlSerialization tool in .NET, but not that it makes much of a difference anyway...            
        }
    }

    public class MyCustomMapper : IMapper
    {
        public QueryMap Map(System.Type type)
        {
            throw new System.NotImplementedException();
        }
    }
}
