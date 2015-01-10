using System;

namespace Minimum.DataAccess.V08
{
    public abstract class Criteria
    {
        public abstract CriteriaType Type { get; }

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

        private static Criteria EvaluatePath(string propertyPath)
        {
            MemberCriteria criteria = new MemberCriteria();
            if (propertyPath.IndexOf('.') > -1)
            {
                criteria.Name = propertyPath.Substring(0, propertyPath.IndexOf('.'));
                propertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);
            }
            else
            {
                criteria.Name = propertyPath;
                return criteria;
            }

            MemberCriteria currentCriteria = criteria;
            while (propertyPath.IndexOf('.') > -1)
            {
                string propertyName = propertyPath.Substring(0, propertyPath.IndexOf('.'));
                propertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);

                MemberCriteria member = new MemberCriteria();
                member.Name = propertyName;

                currentCriteria.Member = member;
                currentCriteria = member;
            }

            MemberCriteria property = new MemberCriteria();
            property.Name = propertyPath;
            currentCriteria.Member = property;

            return criteria;
        }
    }

    internal class ValueCriteria : Criteria
    {
        public object Value { get; set; }
        public Type ValueType { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Value; } }
    }

    internal class MemberCriteria : Criteria
    {
        public Criteria Member { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Member; } }
    }

    internal class BinaryCriteria : Criteria
    {
        public Criteria LeftValue { get; set; }
        public Criteria RightValue { get; set; }
        public BinaryOperand Operand { get; set; }
        public bool UseBrackets { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Binary; } }
    }

    internal class LimitCriteria : Criteria
    {
        public int Value { get; set; }
        public override CriteriaType Type { get { return CriteriaType.Limit; } }
    }

    public enum CriteriaType
    { Value, Member, Binary, Limit }

    public enum BinaryOperand
    { Equal, NotEqual, GreaterThan, GreaterEqualThan, LowerThan, LowerEqualThan, And, Or, Between, In }
}
