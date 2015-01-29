using System;

namespace Minimum.DataAccess.Mapping
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Table : Attribute
    {
        /// <summary>
        /// The name of the table.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The name of the schema.
        /// </summary>
        internal string Schema { get; set; }

        /// <summary>
        /// The name of the database.
        /// </summary>
        internal string Database { get; set; }

        /// <param name="name">The name of the table.</param>
        public Table(string name)
        {
            Name = name;
        }

        /// <param name="name">The name of the table.</param>
        /// <param name="schema">The name of the schema.</param>
        public Table(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }

        /// <param name="name">The name of the table.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="database">The name of the database.</param>
        public Table(string name, string schema, string database)
        {
            Name = name;
            Schema = schema;
            Database = database;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Column : Attribute
    {
        /// <summary>
        /// The name of the column.
        /// </summary>
        internal string Name { get; set; }

        /// <param name="name">The name of the column.</param>
        public Column(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Join : Attribute
    {
        internal JoinType JoinType { get; set; }

        public Join()
        {
            JoinType = JoinType.InnerJoin;
        }

        public Join(JoinType joinType)
        {
            JoinType = joinType;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Key : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Lazy : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Ignore : Attribute { }

    public enum JoinType
    {
        InnerJoin, LeftJoin, RightJoin
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class On : Attribute
    {
        internal string PrimaryKey { get; set; }
        internal string ForeignKey { get; set; }

        public On(string thisValue)
        {
            PrimaryKey = thisValue;
        }

        public On(string thisValue, string thatValue)
        {
            PrimaryKey = thisValue;
            ForeignKey = thatValue;
        }
    }
}