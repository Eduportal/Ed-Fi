using System;
using System.Linq.Expressions;

namespace EdFi.Ods.Common.Utils.Extensions
{
    public static class ExpressionExtensions
    {
        public static string MemberName(this LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            var methodExpression = expression.Body as MethodCallExpression;
            if (methodExpression != null)
            {
                return methodExpression.Method.Name;
            }

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var unaryMember = unaryExpression.Operand as MemberExpression;
                if (unaryMember == null)
                    throw new ArgumentException(string.Format("Strange operand in unary expression '{0}'", expression));

                return unaryMember.Member.Name;
            }

            throw new ArgumentException(string.Format("Expression '{0}' of type '{1}' is not handled", expression, expression.Body.GetType()));
        }
    }
}