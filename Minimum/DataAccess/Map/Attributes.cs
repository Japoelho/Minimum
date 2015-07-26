using System;

namespace Minimum.DataAccess
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Table : Attribute
    {
        internal string Name { get; set; }
        internal string Schema { get; set; }
        internal string Database { get; set; }

        public Table(string name)
        {
            Name = name;
        }

        public Table(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }

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
        internal string Name { get; set; }

        public Column(string name)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Identity : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Ignore : Attribute { }
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Lazy : Attribute 
    {
        internal virtual bool IsLazy { get { return true; } }
    }

    public class NoLazy : Lazy
    {
        internal override bool IsLazy { get { return false; } }
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

    public enum JoinType
    {
        InnerJoin, LeftJoin, RightJoin
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class On : Attribute
    {
        internal string PrimaryKey { get; set; }
        internal string ForeignKey { get; set; }

        public On(string thisValue, string thatValue)
        {
            ForeignKey = thisValue;
            PrimaryKey = thatValue;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class Command : Attribute
    {
        internal string Text { get; set; }

        public Command(string text)
        {
            Text = text;
        }
    }
}