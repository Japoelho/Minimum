using System;

namespace Minimum.DataAccess
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Table : Attribute
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Database { get; set; }

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
        public string Name { get; set; }

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
    public class Cascade : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Lazy : Attribute 
    {
        public virtual bool IsLazy { get { return true; } }
    }

    public class NoLazy : Lazy
    {
        public override bool IsLazy { get { return false; } }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Join : Attribute
    {
        public JoinType JoinType { get; set; }

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
        public string PrimaryKey { get; set; }
        public string ForeignKey { get; set; }

        public On(string thisValue, string thatValue)
        {
            ForeignKey = thisValue;
            PrimaryKey = thatValue;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class Command : Attribute
    {
        public string Text { get; set; }

        public Command(string text)
        {
            Text = text;
        }
    }
}