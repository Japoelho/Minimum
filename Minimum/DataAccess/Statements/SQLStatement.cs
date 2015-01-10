using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Minimum.DataAccess.Mapping;

namespace Minimum.DataAccess.Statement
{
    internal class SQLStatement : IStatement
    {
        private Maps _maps;
        private Alias _alias;
        private IList<Criteria> _criterias = null;

        public SQLStatement(Maps maps)
        {
            _maps = maps;
            _criterias = new List<Criteria>();
        }

        public string Select(Type type)
        {
            StringBuilder query = new StringBuilder();

            Table table = _maps.Map(type);
            _alias = new Alias("T", table, null);

            //IList<QueryColumn> queryColumns = new List<QueryColumn>();

            string where = SelectWhere(_alias);
            string joins = SelectTables(_alias);
            string columns = SelectColumns(_alias);

            query.Append(" SELECT ");
            query.Append(columns);
            query.Append(" FROM ");
            query.Append(table.Name);
            query.Append(" AS ");
            query.Append(_alias.Name);
            query.Append(joins);
            query.Append(where);

            //if (queryColumns.Any(c => c.Column.IsAggregate))
            //{
            //    query.Append(" GROUP BY ");

            //    bool comma = false;
            //    foreach (QueryColumn queryColumn in queryColumns)
            //    {
            //        if (comma) { query.Append(", "); }
                    
            //        query.Append(queryColumn.Alias.Name);
            //        query.Append(".");
            //        query.Append(queryColumn.Column.Name);
                    
            //        queryColumn.IsResolved = true;
            //        comma = true;
            //    }
            //}

            return query.ToString(); 
        }

        private string SelectColumns(Alias alias)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < alias.Table.Columns.Count; i++)
            {
                if (alias.Table.Columns[i].IsIgnore) { continue; }

                if (alias.Table.Columns[i].IsClass == false && alias.Table.Columns[i].IsCollection == false)
                {
                    //queryColumns.Add(new QueryColumn()
                    //{
                    //    Alias = alias,
                    //    Column = alias.Table.Columns[i]
                    //});

                    //if (alias.Table.Columns[i].IsAggregate) { continue; }
                    if (comma == true) { columns.Append(", "); }

                    columns.Append(alias.Name);
                    columns.Append(".");
                    columns.Append(alias.Table.Columns[i].Name);
                    columns.Append(" AS ");
                    columns.Append(alias.Name);
                    columns.Append(alias.Table.Columns[i].Name);

                    comma = true;
                }
                else if (alias.Table.Columns[i].IsClass)
                {
                    Join[] joins = alias.Table.Columns[i].Joins;
                    for (int j = 0; j < joins.Length; j++)
                    {
                        string joinReference = joins[j].ForeignKey ?? _maps.Map(alias.Table.Columns[i].Property.PropertyType).Columns.First(c => c.IsIdentity).Name;

                        if (columns.ToString().Contains(alias.Name + joinReference)) { continue; }

                        if (!alias.Table.Columns.Any(p => p.Name == joinReference || p.Property.Name == joinReference))
                        {
                            //queryColumns.Add(new QueryColumn()
                            //{
                            //    Alias = alias,
                            //    Column = alias.Table.Columns[i]
                            //});

                            if (comma == true) { columns.Append(", "); }

                            columns.Append(alias.Name);
                            columns.Append(".");
                            columns.Append(joinReference);
                            columns.Append(" AS ");
                            columns.Append(alias.Name);
                            columns.Append(joinReference);

                            comma = true;
                        }
                    }

                    Alias joinAlias = alias.Aliases.FirstOrDefault(a => a.Column == alias.Table.Columns[i]);
                    if (joinAlias != null)
                    {
                        if (comma == true) { columns.Append(", "); }

                        columns.Append(SelectColumns(joinAlias));

                        comma = true;
                    }
                }
            }

            if (alias.Table.Base != null)
            {
                Alias joinAlias = alias.Aliases.FirstOrDefault(a => a.Table == alias.Table.Base && a.Column == null);
                if (joinAlias != null)
                {
                    if (comma == true) { columns.Append(", "); }
                    columns.Append(SelectColumns(joinAlias));
                }
            }

            return columns.ToString();
        }

        private string SelectTables(Alias alias)
        {
            StringBuilder tables = new StringBuilder();

            //// - FROM clause
            //if (alias.Parent == null)
            //{                
            //    tables.Append(alias.Table.Name);
            //    tables.Append(" AS ");
            //    tables.Append(alias.Name);
            //}

            // - JOIN base
            if (alias.Table.Base != null)
            {
                if (alias.Table.Joins.Length > 0)
                {
                    Alias joinAlias = alias.MapAlias(alias.Table.Base, null);

                    tables.Append(" INNER JOIN ");
                    tables.Append(alias.Table.Base.Name);
                    tables.Append(" AS ");
                    tables.Append(joinAlias.Name);

                    Column thisIdentity = alias.Table.Columns.FirstOrDefault(c => c.IsIdentity);
                    Column joinIdentity = alias.Table.Base.Columns.FirstOrDefault(c => c.IsIdentity);
                    for (int i = 0; i < alias.Table.Joins.Length; i++)
                    {
                        tables.Append(i == 0 ? " ON " : " AND ");
                        tables.Append(joinAlias.Name);
                        tables.Append(".");
                        tables.Append(!String.IsNullOrEmpty(alias.Table.Joins[i].PrimaryKey) ? alias.Table.Joins[i].PrimaryKey : joinIdentity.Name);

                        tables.Append(" = ");
                        tables.Append(alias.Name);
                        tables.Append(".");

                        tables.Append(!String.IsNullOrEmpty(alias.Table.Joins[i].ForeignKey) ? alias.Table.Joins[i].ForeignKey : thisIdentity.Name);
                    }

                    tables.Append(SelectTables(joinAlias));
                }
            }

            // - JOIN properties
            for (int i = 0; i < alias.Table.Columns.Count; i++)
            {
                if (alias.Table.Columns[i].IsIgnore || alias.Table.Columns[i].IsCollection || !alias.Table.Columns[i].IsClass) { continue; }

                if (alias.Table.Columns[i].Joins.Length == 0) { continue; }

                Alias joinAlias = null;
                switch (alias.Table.Columns[i].Joins.Last().JoinType)
                {
                    case JoinType.InnerJoin:
                        {
                            joinAlias = alias.MapAlias(_maps.Map(alias.Table.Columns[i].Property.PropertyType), alias.Table.Columns[i]);
                            tables.Append(" INNER JOIN ");
                            break;
                        }
                    case JoinType.LeftJoin:
                        {
                            joinAlias = alias.MapAlias(_maps.Map(alias.Table.Columns[i].Property.PropertyType), alias.Table.Columns[i]);
                            tables.Append(" LEFT JOIN ");
                            break;
                        }
                    case JoinType.LazyJoin:
                        {
                            joinAlias = alias.Aliases.FirstOrDefault(a => a.Column == alias.Table.Columns[i]);
                            if (joinAlias == null) { return null; }
                            tables.Append(" LEFT JOIN ");
                            break;
                        }
                }

                tables.Append(joinAlias.Table.Name);
                tables.Append(" AS ");
                tables.Append(joinAlias.Name);

                Column thisIdentity = alias.Table.Columns.FirstOrDefault(c => c.IsIdentity);
                Column joinIdentity = joinAlias.Table.Columns.FirstOrDefault(c => c.IsIdentity);
                for (int j = 0; j < alias.Table.Columns[i].Joins.Length; j++)
                {
                    tables.Append(j == 0 ? " ON " : " AND ");
                    tables.Append(joinAlias.Name);
                    tables.Append(".");
                    tables.Append(!String.IsNullOrEmpty(alias.Table.Columns[i].Joins[j].PrimaryKey) ? alias.Table.Columns[i].Joins[j].PrimaryKey : joinIdentity.Name);

                    tables.Append(" = ");
                    tables.Append(alias.Name);
                    tables.Append(".");

                    tables.Append(!String.IsNullOrEmpty(alias.Table.Columns[i].Joins[j].ForeignKey) ? alias.Table.Columns[i].Joins[j].ForeignKey : joinIdentity.Name);
                }

                tables.Append(SelectTables(joinAlias));
            }

            return tables.ToString();
        }

        private string SelectWhere(Alias alias)
        {
            StringBuilder where = new StringBuilder();
            where.Append(" WHERE 1 = 1");

            for (int i = 0; i < _criterias.Count; i++)
            {
                where.Append(EvaluateCriteria(_criterias[i], alias));
            }

            return where.ToString();
        }

        public string Update(object element, Type type) 
        {
            StringBuilder query = new StringBuilder();

            Table table = _maps.Map(type);
            Column identity = table.Columns.FirstOrDefault(p => p.IsIdentity);

            StringBuilder queryBuilder = new StringBuilder();

            query.Append("UPDATE ");
            query.Append(table.Name);
            query.Append(" SET ");
            query.Append(UpdateColumnValues(table, element));
            query.Append(" WHERE ");
            query.Append(identity.Name);
            query.Append(" = ");
            query.Append(FormatWriteValue(identity.Property.GetValue(element, null), identity.Property.PropertyType));

            return query.ToString();
        }

        private string UpdateColumnValues(Table table, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].IsIdentity || table.Columns[i].IsIgnore || table.Columns[i].IsClass || table.Columns[i].IsCollection) { continue; }

                object value = table.Columns[i].Property.GetValue(element, null);

                if (comma == true) { columns.Append(", "); }
                columns.Append(table.Columns[i].Name);
                columns.Append(" = ");
                columns.Append(FormatWriteValue(value, table.Columns[i].Property.PropertyType));
                comma = true;
            }

            return columns.ToString();
        }

        public string Insert(object element, Type type)
        {
            StringBuilder query = new StringBuilder();

            Table table = _maps.Map(type);
            Column identity = table.Columns.FirstOrDefault(p => p.IsIdentity);

            query.Append("INSERT INTO ");
            query.Append(table.Name);
            query.Append(" (");
            query.Append(InsertColumns(table, element));
            query.Append(") VALUES (");
            query.Append(InsertValues(table, element));
            query.Append(") ");
            query.Append("SELECT @@IDENTITY AS ID");

            return query.ToString();
        }
        
        private string InsertColumns(Table table, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].IsIdentity || table.Columns[i].IsIgnore || table.Columns[i].IsClass || table.Columns[i].IsCollection) { continue; }

                if (comma == true) { columns.Append(", "); }
                columns.Append(table.Columns[i].Name);
                comma = true;
            }

            return columns.ToString();
        }

        private string InsertValues(Table table, object element)
        {
            StringBuilder values = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].IsIdentity || table.Columns[i].IsIgnore || table.Columns[i].IsClass || table.Columns[i].IsCollection) { continue; }

                if (comma == true) { values.Append(", "); }
                values.Append(FormatWriteValue(table.Columns[i].Property.GetValue(element, null), table.Columns[i].Property.PropertyType));
                comma = true;
            }

            return values.ToString();
        }

        public string Delete(object element, Type type) 
        {
            StringBuilder query = new StringBuilder();

            Table table = _maps.Map(type);
            Column identity = table.Columns.FirstOrDefault(p => p.IsIdentity);

            query.Append("DELETE FROM ");
            query.Append(table.Name);
            query.Append(" WHERE ");
            query.Append(identity.Name);
            query.Append(" = ");
            query.Append(FormatWriteValue(identity.Property.GetValue(element, null), identity.Property.PropertyType));

            return query.ToString();
        }
        
        public IStatement AddCriteria(params Criteria[] criterias)
        {
            for (int i = 0; i < criterias.Length; i++)
            {
                _criterias.Add(criterias[i]);
            }

            return this;
        }
        
        private string EvaluateCriteria(Criteria criteria, Alias alias)
        {
            if (criteria == null) { return null; }

            StringBuilder condition = new StringBuilder();

            switch (criteria.Type)
            {
                case CriteriaType.Value:
                    {
                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(FormatWriteValue((criteria as ValueCriteria).Value, (criteria as ValueCriteria).ValueType));

                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append(")"); }

                        break;
                    }
                case CriteriaType.Member:
                    {
                        Alias currentAlias = alias;

                        Column propertyMap = null;
                        while ((criteria as MemberCriteria) != null)
                        {
                            propertyMap = null;
                            while (propertyMap == null)
                            {
                                propertyMap = currentAlias.Table.Columns.FirstOrDefault(c => c.Property.Name == (criteria as MemberCriteria).Name);
                                if (propertyMap == null && currentAlias.Table.Base == null)
                                {
                                    throw new ArgumentException(String.Format(Maps.CoultNotResolveProperty, (criteria as MemberCriteria).Name, currentAlias.Table.Type.Name));
                                }
                                else if (propertyMap == null && currentAlias.Table.Base != null)
                                {
                                    currentAlias = currentAlias.MapAlias(currentAlias.Table.Base, null);
                                }
                                else if ((criteria as MemberCriteria).Member != null)
                                {
                                    currentAlias = currentAlias.MapAlias(_maps.Map(propertyMap.Property.PropertyType), propertyMap);
                                }
                            }

                            criteria = (criteria as MemberCriteria).Member;
                        }

                        condition.Append(currentAlias.Name + "." + propertyMap.Name);
                        break;
                    }
                case CriteriaType.Binary:
                    {
                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).LeftValue, alias));
                        switch ((criteria as BinaryCriteria).Operand)
                        {
                            case BinaryOperand.Equal: { condition.Append(" = "); break; }
                            case BinaryOperand.NotEqual: { condition.Append(" <> "); break; }
                            case BinaryOperand.GreaterThan: { condition.Append(" > "); break; }
                            case BinaryOperand.GreaterEqualThan: { condition.Append(" >= "); break; }
                            case BinaryOperand.LowerThan: { condition.Append(" < "); break; }
                            case BinaryOperand.LowerEqualThan: { condition.Append(" <= "); break; }
                            case BinaryOperand.And: { condition.Append(" AND "); break; }
                            case BinaryOperand.Or: { condition.Append(" OR "); break; }
                            case BinaryOperand.In: { condition.Append(" IN "); break; }
                            case BinaryOperand.Between: { condition.Append(" BETWEEN "); break; }
                        }
                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).RightValue, alias));

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
            }

            return condition.ToString();
        }

        private string FormatWriteValue(object value, Type valueType)
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = valueType.GetGenericArguments()[0];
            }

            if (value == null) { return "NULL"; }
            if (valueType.IsEnum) { return Convert.ToInt32(value).ToString(); }

            switch (valueType.Name)
            {
                case "Boolean":
                case "bool": { return Convert.ToBoolean(value) ? "1" : "0"; }
                case "Single":
                case "Decimal": { return value.ToString().Replace(',', '.'); }
                case "Int64":
                case "Int32":
                case "Int16":
                case "int": { return value.ToString(); }
                case "DateTime": { return "CONVERT(DATETIME, '" + Convert.ToDateTime(value).ToString("dd/MM/yyyy HH:mm:ss") + "', 103)"; }
                default: { return "'" + value.ToString().Replace("'", "''") + "'"; }
            }
        }

        public Alias Alias()
        {
            return _alias;
        }
        
        private class QueryColumn
        {
            public bool IsResolved { get; set; }
            public bool IsAggregate { get; set; }
            public Column Column { get; set; }
            public Alias Alias { get; set; }
        }
    }
}
