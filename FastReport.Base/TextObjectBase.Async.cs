using System;
using System.Threading;
using System.Threading.Tasks;

using FastReport.Utils;

namespace FastReport;

public partial class TextObjectBase
{
    #region Public Methods

    internal async Task<string> CalcAndFormatExpressionAsync(string expression, int expressionIndex, CancellationToken token)
    {
        try
        {
            return FormatValue(await Report.CalcAsync(expression, token), expressionIndex);
        }
        catch (Exception e)
        {
            throw new Exception(Name + ": " + Res.Get("Messages,ErrorInExpression") + ": " + expression,
                e.InnerException == null ? e : e.InnerException);
        }
    }

    #endregion
}