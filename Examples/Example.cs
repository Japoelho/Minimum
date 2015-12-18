using Minimum.Connection;
using Minimum.DataAccess;
using System.Collections.Generic;

namespace Examples
{
    class Example
    {
        static void Main(string[] args)
        {
            BasicSCRUD();

            Queries();
        }

        private static void BasicSCRUD()
        {
            // - The [Repository] requires an [IConnection] to work with. Using [SQLite] in this example, check the interface if you need to implement your own.            
            SQLite sqliteConnection = new SQLite("App.data");
            Repository repository = new Repository(sqliteConnection);
            
            // - Selects ALL [User] from the database.
            IList<User> users = repository.Select<User>();
            
            // - Selects the [User] with it's [Identity] equal to 1.
            User user = repository.Select<User>(1);
            
            // - Selects all [User] which the company name is "MyCompany" using an expression.
            IList<User> someUsers = repository.Select<User>(u => u.Company.Name == "MyCompany");
            
            // - Selects all [User] which the company name is either "MyCompany" or "AnotherCompany" and their name is like %Daniichi%, ordered by name.
            IList<User> fewUsers = repository.Select<User>(                    
                    // - Using a list of criterias to give you more commands/control over queries.
                    Criteria.Any(
                        Criteria.EqualTo("Company.Name", "MyCompany"),
                        Criteria.EqualTo("Company.Name", "AnotherCompany")
                    ),
                    Criteria.Like("Name", "%Daniichi%"),
                    Criteria.Order("Name", OrderBy.Ascending)
                );
            
            // - Returns the count of the [User] which the companyID is equal to 1.
            long totalUsers = repository.Count<User>(u => u.CompanyID == 1);
            
            // - Update the [User].
            repository.Update<User>(user);            

            // - Insert the [User] and set it's [Identity] property to the new inserted ID.
            repository.Insert<User>(user);

            // - Delete the [User].
            repository.Delete<User>(user);
        }

        private static void Queries()
        {
            Repository repository = new Repository(new SQLite("App.data"));

            // - Executes the query and returns the number of affected rows.
            int rows = repository.Execute("SELECT 1");

            // - Executes the query and the repository will use the [User] mapping to attempt to retrieve the values.
            IList<User> users = repository.Execute<User>("SELECT 1 AS UserID, 'NonExistentUser' AS Name, 1 AS CompanyID");

            // - Gets the [User] with ID 1.
            User user = repository.Select<User>(1);
            user.Comments.Add(new Comment() { Text = "Nice Comment" });
            user.Comments.Add(new Comment() { Text = "Bad Comment" });
            user.Name = "Changed Name";

            // - This function will insert or update the user, depending on it's [Identity]. If it already exists in the database, it's an INSERT, else, an UPDATE.
            // - After this it'll look for properties with [Cascade], and it'll compare against the database values. If they don't exist, they're INSERTED, else UPDATED. Values not found remaining in the database are DELETED.
            repository.Cascade<User>(user);

            // - This property is marked as [Virtual] and it'll be Lazy-loaded in this line. 
            // - To prevent Lazy loading, set the property to [NoLazy].
            IList<Comment> comments = user.Comments;
        }
    }
}