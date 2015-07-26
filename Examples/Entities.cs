using Minimum.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Examples.Entities
{
    // - [Table] sets the name of the table in the database. If unspecified, it'll assume the class name instead.
    [Table("MyTableName")]
    public class Person
    {
        // - [Column] sets the name of the column in the database. If unspecified, it'll assume the property name instead.
        // - Every class must have at least one [Identity] property.
        [Identity, Column("MyRowID")] public int PersonID { get; set; }
        public string Name { get; set; }

        // - Always set the "virtual" keyword for collections so it can use Lazy loading.
        public virtual IList<Address> Addresses { get; set; }

        // - If you don't want to, you can specify the [NoLazy] attribute.
        [NoLazy] public IList<Document> Documents { get; set; }

        // - Non-collection classes will always be joined unless you specify [Lazy] and "virtual".
        [Lazy] public virtual Account Account { get; set; }

        // - No subclasses are updated, inserted or deleted when using the Repository.
    }

    public class Account
    {
        [Identity] public int AccountID { get; set; }
        public string Name { get; set; }
        public int PersonID { get; set; }
    }

    public class Address
    {
        [Identity] public int AddressID { get; set; }
        public string Street { get; set; }
        public int PersonID { get; set; }
    }

    public class Document
    {
        [Identity] public int DocumentID { get; set; }
        public string Name { get; set; }
        public int PersonID { get; set; }
    }
}