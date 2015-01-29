using System;
using System.Linq.Expressions;

namespace Minimum.DataAccess
{
    internal static class Parser
    {
        internal static Criteria Criteria(Expression expression)
        {
            if (expression == null) { return null; }

            BinaryCriteria criteria = new BinaryCriteria();
            criteria.Operand = BinaryOperand.And;
            criteria.RightValue = Parse(expression);

            return criteria;
        }

        private static Criteria Parse(Expression expression)
        {
            if (expression == null) { return null; }

            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    {
                        return ParseBinary((BinaryExpression)expression);
                    }
                case ExpressionType.Constant:
                    {
                        ValueCriteria criteria = new ValueCriteria();
                        criteria.Value = (expression as ConstantExpression).Value;
                        criteria.ValueType = (expression as ConstantExpression).Type;

                        return criteria;
                    }
                case ExpressionType.Parameter:
                    {
                        return new MemberCriteria();
                    }
                case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberExpression = (expression as MemberExpression);

                        if (memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            MemberCriteria criteria = new MemberCriteria();
                            criteria.Name = memberExpression.Member.Name;

                            return criteria;
                        }
                        else if (memberExpression.Expression != null)
                        {
                            MemberCriteria criteria = Parse(memberExpression.Expression) as MemberCriteria;
                            if (criteria != null)
                            {
                                MemberCriteria value = new MemberCriteria();
                                value.Name = memberExpression.Member.Name;

                                MemberCriteria final = criteria as MemberCriteria;
                                while (final.Member != null)
                                { final = final.Member as MemberCriteria; }
                                final.Member = value;

                                return criteria;
                            }
                            else
                            {
                                UnaryExpression unaryExpression = Expression.Convert((expression as MemberExpression), typeof(object));
                                Expression<Func<object>> delegateExpression = Expression.Lambda<Func<object>>(unaryExpression);
                                Func<object> function = delegateExpression.Compile();

                                object memberValue = function();

                                ValueCriteria value = new ValueCriteria()
                                {
                                    Value = memberValue,
                                    ValueType = memberValue != null ? memberValue.GetType() : null
                                };

                                return value;
                            }
                        }

                        return null;
                    }
                case ExpressionType.Lambda:
                    {
                        return Parse((expression as LambdaExpression).Body);
                    }
                //Not Implemented:
                case ExpressionType.TypeIs:
                //return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    //return this.VisitConditional((ConditionalExpression)exp);
                    return null;
                case ExpressionType.Call:
                    {
                        Expression result = Expression.Convert(Expression.Call((expression as MethodCallExpression).Object, (expression as MethodCallExpression).Method, (expression as MethodCallExpression).Arguments), typeof(Object));
                        Expression<Func<object>> delegateExpression = Expression.Lambda<Func<object>>(result);
                        Func<object> function = delegateExpression.Compile();

                        object value = function();

                        ValueCriteria criteria = new ValueCriteria();
                        criteria.Value = value;
                        criteria.ValueType = value.GetType();

                        return criteria;
                    }
                //return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.New:
                //return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                //return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                //return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                //return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                //return this.VisitListInit((ListInitExpression)exp);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                    return null;
                case ExpressionType.Convert:
                    {
                        Criteria criteria = Parse((expression as UnaryExpression).Operand);
                        //if (criteria.GetType().Equals(typeof(ValueCriteria)))
                        //{
                        //    UnaryExpression unaryExpression = Expression.Convert((expression as UnaryExpression), typeof(object));
                        //    Expression<Func<object>> delegateExpression = Expression.Lambda<Func<object>>(unaryExpression);
                        //    Func<object> function = delegateExpression.Compile();

                        //    object value = function();

                        //    (criteria as ValueCriteria).Value = value;
                        //}

                        return criteria;
                    }
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                //return this.ParseUnary((UnaryExpression)expression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return null;
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", expression.NodeType));
            }
        }

        private static Criteria ParseBinary(BinaryExpression expression)
        {
            BinaryCriteria criteria = new BinaryCriteria();
            criteria.UseBrackets = true; //TODO: Pensar?
            criteria.LeftValue = Parse(expression.Left);
            criteria.RightValue = Parse(expression.Right);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal: { criteria.Operand = BinaryOperand.Equal; break; }
                case ExpressionType.NotEqual: { criteria.Operand = BinaryOperand.NotEqual; break; }
                case ExpressionType.GreaterThan: { criteria.Operand = BinaryOperand.GreaterThan; break; }
                case ExpressionType.GreaterThanOrEqual: { criteria.Operand = BinaryOperand.GreaterEqualThan; break; }
                case ExpressionType.LessThan: { criteria.Operand = BinaryOperand.LowerThan; break; }
                case ExpressionType.LessThanOrEqual: { criteria.Operand = BinaryOperand.LowerEqualThan; break; }
                case ExpressionType.AndAlso: { criteria.Operand = BinaryOperand.And; break; }
                case ExpressionType.OrElse: { criteria.Operand = BinaryOperand.Or; break; }
                default: { throw new NotImplementedException(); }
            }

            return criteria;
        }

        private static Expression ParseUnary(UnaryExpression expression)
        {
            //Expression operand = ParseUnary(expression.Operand);
            return null;
        }
    }
}