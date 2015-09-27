using Minimum.Connection;
using Minimum.DataAccess;
using System.Collections.Generic;

namespace Examples
{
    class Example
    {
        static void Main(string[] args)
        {
            #region [ Repository Usage Example ]
            // - The Repository requires an IConnection which you can use a Minimum one or implement one yourself.
            // - Using SQLite in this example.
            SQLite sqliteConnection = new SQLite("App.data");

            Repository repository = new Repository(sqliteConnection);
            
            // - Selects ALL users from the database.
            IList<User> users = repository.Select<User>();
            
            // - Selects only the user with it's [Identity] equal to 1.
            User user = repository.Select<User>(1);
            
            // - Selects only the users which the company name is equal to "MyCompany".
            IList<User> someUsers = repository.Select<User>(u => u.Company.Name == "MyCompany");
            
            // - Selects only the users which the company name is either "MyCompany" or "AnotherCompany" and their name is like %Daniichi%, ordered by name.
            IList<User> fewUsers = repository.Select<User>(                    
                    // - Using a list of criterias to give you more commands/control over queries.
                    Criteria.Any(
                        Criteria.EqualTo("Company.Name", "MyCompany"),
                        Criteria.EqualTo("Company.Name", "AnotherCompany")
                    ),
                    Criteria.Like("Name", "%Daniichi%"),
                    Criteria.Order("Name", OrderBy.Ascending)
                );
            
            // - Returns the count of the users which the companyID is equal to 1.
            long totalUsers = repository.Count<User>(u => u.CompanyID == 1);
            
            // - Update the user.
            repository.Update<User>(user);            

            // - Insert the user and set it's [Identity] property to the new inserted ID.
            repository.Insert<User>(user);

            // - Delete the user.
            repository.Delete<User>(user);
            #endregion

            #region [ Repository Usage Example ]
            #endregion

            #region [ Repository Usage Example ]
            #endregion
        }
    }

    [Table("Users")]
    public class User
    {
        [Identity, Column("ROWID")] public int UserID { get; set; }
        public string Name { get; set; }

        public int CompanyID { get; set; }
        [On("CompanyID", "ROWID")] public Company Company { get; set; }
    }

    [Table("Companies")]
    public class Company
    {
        [Identity, Column("ROWID")] public int CompanyID { get; set; }
        public string Name { get; set; }
    }
}