using Examples.Entities;
using Minimum.Connection;
using Minimum.DataAccess;
using System.Collections.Generic;

namespace Examples
{
    class Example
    {
        static void Main(string[] args)
        {
            /*************** Repository usage ***************/

            // - Create a new connection. You can create your own connections by inheriting the IConnection interface and implementing it.            
            SQLite connection = new SQLite("App.data");            

            // - Create the repository using the connection.
            Repository repository = new Repository(connection);

            // - Select all "Person" in the database.
            IList<Person> persons = repository.Select<Person>();

            // - Select only one by it's [Identity] property.
            Person someone = repository.Select<Person>(1);

            // - Select a few based on a criteria.
            IList<Person> group = repository.Select<Person>(p => p.Name == "Dave");

            // - Update a record. This will be based on the [Identity] property.            
            someone.Name = "Usagi";
            repository.Update<Person>(someone);

            // - Insert a new record. The [Identity] property will be updated.
            Person newGuy = new Person();
            newGuy.Name = "Trainee";
            repository.Insert<Person>(newGuy); // - [Identity] of "newGuy" has been updated.

            // - Delete a record. This will be based on the [Identity] property.
            repository.Delete<Person>(newGuy);

            /* Notes:
             * - Check the Entities.cs file for more information on properties and actions.
             * 
             * - This Repository uses my AutoMapper class, which you can also implement your own with your own conventions.
             *   This AutoMapper has been done using [Attribute] classes.
             *   See the DataAccess\AutoMapper\AutoMapper.cs file for the example.
             *   
             * - You can also implement your own Repository if you want, or add new functions to the existing one.
             *   Check the DataAccess\Repository.cs file for the example.
             */
        }
    }
}