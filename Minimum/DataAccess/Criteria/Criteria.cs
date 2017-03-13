using System;
using System.Linq.Expressions;

namespace Minimum.DataAccess
{
    public abstract class Criteria
    {
        #region [ Properties ]
        public abstract CriteriaType Type { get; }
        #endregion
        
        #region [ Public ]
        public static Criteria Any(params Criteria[] criterias)
        {
            return new AnyCriteria()
            {
                Criterias = criterias,
                UseBrackets = true
            };
        }

        public static Criteria All(params Criteria[] criterias)
        {
            return new AllCriteria()
            {
                Criterias = criterias,
                UseBrackets = true
            };
        }

        public static Criteria EqualTo(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = value == null ? BinaryOperand.Is : BinaryOperand.Equal,
                RightValue = new ValueCriteria() { Value = value, ValueType = value == null ? typeof(string) : value.GetType(), UseBrackets = false }
            };

            //return new BinaryCriteria()
            //{
            //    UseBrackets = false,
            //    LeftValue = null,
            //    Operand = BinaryOperand.And,
            //    RightValue = new BinaryCriteria()
            //    {
            //        UseBrackets = true,
            //        LeftValue = EvaluatePath(propertyPath),
            //        Operand = BinaryOperand.Equal,
            //        RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            //    }
            //};
        }

        public static Criteria EqualTo<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = value == null ? BinaryOperand.Is : BinaryOperand.Equal,
                RightValue = new ValueCriteria() { Value = value, ValueType = value == null ? typeof(string) : value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria NotEqualTo(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = value == null ? BinaryOperand.IsNot : BinaryOperand.NotEqual,
                RightValue = new ValueCriteria() { Value = value, ValueType = value == null ? typeof(string) : value.GetType(), UseBrackets = false }
            };

            //return new BinaryCriteria()
            //{
            //    UseBrackets = false,
            //    LeftValue = null,
            //    Operand = BinaryOperand.And,
            //    RightValue = new BinaryCriteria()
            //    {
            //        UseBrackets = true,
            //        LeftValue = EvaluatePath(propertyPath),
            //        Operand = BinaryOperand.NotEqual,
            //        RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            //    }
            //};
        }

        public static Criteria NotEqualTo<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = value == null ? BinaryOperand.IsNot : BinaryOperand.NotEqual,
                RightValue = new ValueCriteria() { Value = value, ValueType = value == null ? typeof(string) : value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria GreaterThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = BinaryOperand.GreaterThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };

            //return new BinaryCriteria()
            //{
            //    UseBrackets = false,
            //    LeftValue = null,
            //    Operand = BinaryOperand.And,
            //    RightValue = new BinaryCriteria()
            //    {
            //        UseBrackets = true,
            //        LeftValue = EvaluatePath(propertyPath),
            //        Operand = BinaryOperand.GreaterThan,
            //        RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            //    }
            //};
        }

        public static Criteria GreaterThan<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = BinaryOperand.GreaterThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria GreaterEqualThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = BinaryOperand.GreaterEqualThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };

            //return new BinaryCriteria()
            //{
            //    UseBrackets = false,
            //    LeftValue = null,
            //    Operand = BinaryOperand.And,
            //    RightValue = new BinaryCriteria()
            //    {
            //        UseBrackets = true,
            //        LeftValue = EvaluatePath(propertyPath),
            //        Operand = BinaryOperand.GreaterEqualThan,
            //        RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            //    }
            //};
        }

        public static Criteria GreaterEqualThan<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = BinaryOperand.GreaterEqualThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria LowerThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = BinaryOperand.LowerThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };

            //return new BinaryCriteria()
            //{
            //    UseBrackets = false,
            //    LeftValue = null,
            //    Operand = BinaryOperand.And,
            //    RightValue = new BinaryCriteria()
            //    {
            //        UseBrackets = true,
            //        LeftValue = EvaluatePath(propertyPath),
            //        Operand = BinaryOperand.LowerThan,
            //        RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            //    }
            //};
        }

        public static Criteria LowerThan<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = BinaryOperand.LowerThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria LowerEqualThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(propertyPath),
                Operand = BinaryOperand.LowerEqualThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria LowerEqualThan<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Operand = BinaryOperand.LowerEqualThan,
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria Between(string propertyPath, object valueFrom, object valueTo)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.Between,
                LeftValue = EvaluatePath(propertyPath),
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = false,
                    Operand = BinaryOperand.And,
                    LeftValue = new ValueCriteria() { Value = valueFrom, ValueType = valueFrom.GetType(), UseBrackets = false },
                    RightValue = new ValueCriteria() { Value = valueTo, ValueType = valueTo.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria Between<T>(Expression<Func<T, object>> property, object valueFrom, object valueTo)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.Between,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = false,
                    Operand = BinaryOperand.And,
                    LeftValue = new ValueCriteria() { Value = valueFrom, ValueType = valueFrom.GetType(), UseBrackets = false },
                    RightValue = new ValueCriteria() { Value = valueTo, ValueType = valueTo.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria In(string propertyPath, object values)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.In,
                LeftValue = EvaluatePath(propertyPath),
                RightValue = new ValueCriteria() { Value = values, ValueType = values.GetType(), UseBrackets = true }
            };
        }

        public static Criteria In<T>(Expression<Func<T, object>> property, object values)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.In,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                RightValue = new ValueCriteria() { Value = values, ValueType = values.GetType(), UseBrackets = true }
            };
        }

        public static Criteria Limit(int count)
        {
            return new LimitCriteria()
            {
                Value = count
            };
        }

        public static Criteria Like(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.Like,
                LeftValue = EvaluatePath(propertyPath),
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria Like<T>(Expression<Func<T, object>> property, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = true,
                Operand = BinaryOperand.Like,
                LeftValue = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
            };
        }

        public static Criteria Order(string propertyPath, OrderBy orderBy)
        {
            return new OrderCriteria()
            {
                Member = EvaluatePath(propertyPath),
                Ascending = orderBy == OrderBy.Ascending ? true : false
            };
        }

        public static Criteria Order<T>(Expression<Func<T, object>> property, OrderBy orderBy)
        {
            return new OrderCriteria()
            {
                Member = EvaluatePath(MapExpression.MapPropertyToStringPath(property)),
                Ascending = orderBy == OrderBy.Ascending ? true : false
            };
        }

        public static Criteria Skip(int amount)
        {
            return new SkipCriteria()
            {
                Value = amount
            };
        }
        #endregion

        #region [ Private ]
        private static Criteria EvaluatePath(string propertyPath)
        {
            MemberCriteria criteria = new MemberCriteria();
            MemberCriteria currentCriteria = criteria;
            while (propertyPath.IndexOf('.') > -1)
            {
                string propertyName = propertyPath.Substring(0, propertyPath.IndexOf('.'));
                propertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);

                currentCriteria.Name = propertyName;
                currentCriteria.Member = new MemberCriteria();
                currentCriteria = (MemberCriteria)currentCriteria.Member;
            }
            currentCriteria.Name = propertyPath;

            return criteria;
        }
        #endregion
    }

    public enum OrderBy
    { Ascending, Descending }

    public class AllCriteria : Criteria
    {
        public Criteria[] Criterias { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.All; } }
    }

    public class AnyCriteria : Criteria
    {
        public Criteria[] Criterias { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Any; } }
    }

    public class ValueCriteria : Criteria
    {
        public object Value { get; set; }
        public Type ValueType { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Value; } }
    }

    public class MemberCriteria : Criteria
    {
        public Criteria Member { get; set; }
        public string Name { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Member; } }
    }

    public class BinaryCriteria : Criteria
    {
        public Criteria LeftValue { get; set; }
        public Criteria RightValue { get; set; }
        public BinaryOperand Operand { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Binary; } }
    }

    public class LimitCriteria : Criteria
    {
        public int Value { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Limit; } }
    }

    public class SkipCriteria : Criteria
    {
        public int Value { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Skip; } }
    }

    public class OrderCriteria : Criteria 
    {
        public Criteria Member { get; set; }
        public bool Ascending { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Order; } }
    }

    public enum CriteriaType
    { Any, All, Value, Member, Binary, Limit, Skip, Order }

    public enum BinaryOperand
    { Equal, NotEqual, GreaterThan, GreaterEqualThan, LowerThan, LowerEqualThan, And, Or, Between, In, Like, Is, IsNot }
}