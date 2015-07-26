using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Minimum.DataAccess
{
    internal static class MapExpression
    {
        private static string INVALID_MEMBER_CALL = "Invalid property call for the type.";
        private static string UNSUPPORTED_EXPRESSION_TYPE = "Invalid Expression for the call.";

        public static PropertyInfo MapProperty(Expression expression)
        {
            if (expression == null) { return null; }

            PropertyInfo propertyInfo = Parse(expression);

            return propertyInfo;
        }

        private static PropertyInfo Parse(Expression expression)
        {
            if (expression == null) { return null; }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberExpression = (expression as MemberExpression);
                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter)  { throw new ArgumentException(INVALID_MEMBER_CALL); }

                        return memberExpression.Expression.Type.GetProperty(memberExpression.Member.Name);
                    }
                case ExpressionType.Lambda:
                    {
                        return Parse((expression as LambdaExpression).Body);
                    }
                default:
                    throw new ArgumentException(UNSUPPORTED_EXPRESSION_TYPE);
            }
        }
    }
}
