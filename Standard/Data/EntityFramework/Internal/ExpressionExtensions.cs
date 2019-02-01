using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace TraceCore.Standard.Data.EntityFramework.Internal
{
    static class ExpressionExtensions
    {
        // System.Data.Entity.Utilities.ExpressionExtensions
        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

    }
}