using System;

namespace Minimum.DataAccess
{
    public abstract class Criteria
    {
        #region [ Properties ]
        internal abstract CriteriaType Type { get; }
        #endregion

        #region [ Public ]
        public static Criteria EqualTo(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.Equal,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria NotEqualTo(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.NotEqual,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria GreaterThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.GreaterThan,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria GreaterEqualThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.GreaterEqualThan,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria LowerThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.LowerThan,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria LowerEqualThan(string propertyPath, object value)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                LeftValue = null,
                Operand = BinaryOperand.And,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    LeftValue = EvaluatePath(propertyPath),
                    Operand = BinaryOperand.LowerEqualThan,
                    RightValue = new ValueCriteria() { Value = value, ValueType = value.GetType(), UseBrackets = false }
                }
            };
        }

        public static Criteria Between(string propertyPath, object valueFrom, object valueTo)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                Operand = BinaryOperand.And,
                LeftValue = null,
                RightValue = new BinaryCriteria()
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
                }
            };
        }

        public static Criteria In(string propertyPath, object values)
        {
            return new BinaryCriteria()
            {
                UseBrackets = false,
                Operand = BinaryOperand.And,
                LeftValue = null,
                RightValue = new BinaryCriteria()
                {
                    UseBrackets = true,
                    Operand = BinaryOperand.In,
                    LeftValue = EvaluatePath(propertyPath),
                    RightValue = new ValueCriteria() { Value = values, ValueType = values.GetType(), UseBrackets = true }
                }
            };
        }

        public static Criteria Limit(int count)
        {
            return new LimitCriteria()
            {
                Value = count
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

    internal class ValueCriteria : Criteria
    {
        internal object Value { get; set; }
        internal Type ValueType { get; set; }
        internal bool UseBrackets { get; set; }
        internal override CriteriaType Type { get { return CriteriaType.Value; } }
    }

    internal class MemberCriteria : Criteria
    {
        internal Criteria Member { get; set; }
        internal string Name { get; set; }
        internal override CriteriaType Type { get { return CriteriaType.Member; } }
    }

    internal class BinaryCriteria : Criteria
    {
        internal Criteria LeftValue { get; set; }
        internal Criteria RightValue { get; set; }
        internal BinaryOperand Operand { get; set; }
        internal bool UseBrackets { get; set; }
        internal override CriteriaType Type { get { return CriteriaType.Binary; } }
    }

    internal class LimitCriteria : Criteria
    {
        internal int Value { get; set; }
        internal override CriteriaType Type { get { return CriteriaType.Limit; } }
    }

    internal class OrderCriteria : Criteria 
    {
        internal Criteria Member { get; set; }
        internal bool Ascending { get; set; }
        internal override CriteriaType Type { get { return CriteriaType.Order; } }
    }

    internal enum CriteriaType
    { Value, Member, Binary, Limit, Order }

    internal enum BinaryOperand
    { Equal, NotEqual, GreaterThan, GreaterEqualThan, LowerThan, LowerEqualThan, And, Or, Between, In }
}