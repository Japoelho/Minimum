using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minimum.DataAccess.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Table : Attribute
    {
        internal Table Base { get; set; }
        internal Join[] Joins { get; set; }
        internal Type Type { get; set; }
        internal IList<Column> Columns { get; set; }
        internal string Name { get; set; }

        internal Table()
        { }

        public Table(string name)
        {
            Name = name;
        }

        public Table(Type type)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Column : Attribute
    {
        //internal Table Table { get; set; }
        internal Join[] Joins { get; set; }
        //internal Aggregate Aggregate { get; set; }
        internal PropertyInfo Property { get; set; }
        internal string Name { get; set; }
        internal bool IsIgnore { get; set; }
        internal bool IsIdentity { get; set; }
        //internal bool IsAggregate { get; set; }
        internal bool IsClass { get; set; }
        internal bool IsCollection { get; set; }
        

        internal Column()
        { }

        public Column(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class Join : Attribute
    {
        internal string PrimaryKey { get; set; }
        internal string ForeignKey { get; set; }
        internal JoinType JoinType { get; set; }

        /// <summary>        
        /// Assumes this class' Identity property equals to the assigned property class' Identity.
        /// </summary>
        public Join(JoinType joinType = JoinType.LazyJoin)
        {
            JoinType = joinType;
        }        

        /// <summary>
        /// Assumes "thisColumn" to be equal the assigned property class' Identity.
        /// </summary>        
        public Join(string thisColumn, JoinType joinType = JoinType.LazyJoin)
        {
            ForeignKey = thisColumn;
            JoinType = joinType;
        }
        
        /// <summary>
        /// Assumes "thisColumn" to be equal to "referenceColumn" in the assigned property class.
        /// </summary>        
        public Join(string thisColumn, string referenceColumn, JoinType joinType = JoinType.LazyJoin)
        {
            PrimaryKey = referenceColumn;
            ForeignKey = thisColumn;
            JoinType = joinType;
        }
    }

    public enum JoinType
    { InnerJoin, LeftJoin, LazyJoin }

    public class Key : Attribute { }
    public class Ignore : Attribute { }

    public abstract class Aggregate : Attribute 
    {
        internal abstract string Command { get; set; }
    }
    
    public class Count : Aggregate
    {
        internal string Name { get; set; }
        internal override string Command { get { return "COUNT({0})"; } set { } }

        public Count()
        { }

        public Count(string column)
        {
            Name = column;
        }
    }

    //public class Max : Aggregate
    //{ }

    //public class Min : Aggregate
    //{ }
}
