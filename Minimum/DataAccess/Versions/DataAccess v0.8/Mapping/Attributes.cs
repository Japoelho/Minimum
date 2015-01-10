using System;

namespace Minimum.DataAccess.V08.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Table : Attribute
    {
        internal string Name { get; set; }

        public Table(string name)
        { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Column : Attribute
    {
        internal string Name { get; set; }

        public Column(string name)
        { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Identity : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Ignore : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class Join : Attribute
    {
        internal string PrimaryKey { get; set; }
        internal string ForeignKey { get; set; }
        internal JoinType JoinType { get; set; }

        public Join()
        { 
            DefaultValues(null, null);
        }

        public Join(JoinType joinType)
        {
            DefaultValues(null, null, joinType);
        }

        public Join(string thisColumn)
        {
            DefaultValues(thisColumn, null);
        }

        public Join(string thisColumn, string referenceColumn)
        {
            DefaultValues(thisColumn, referenceColumn);
        }

        public Join(string thisColumn, JoinType joinType)
        {
            DefaultValues(thisColumn, null, joinType);
        }

        public Join(string thisColumn, string referenceColumn, JoinType joinType)
        {
            DefaultValues(thisColumn, referenceColumn, joinType);
        }

        private void DefaultValues(string thisColumn, string referenceColumn, JoinType joinType = Mapping.JoinType.LazyJoin)
        {
            PrimaryKey = referenceColumn;
            ForeignKey = thisColumn;
            JoinType = joinType;
        }
    }

    public enum JoinType
    { InnerJoin, LeftJoin, LazyJoin }
}