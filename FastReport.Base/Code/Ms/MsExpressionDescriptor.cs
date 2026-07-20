using System.Reflection;

namespace FastReport.Code.Ms;

internal class MsExpressionDescriptor : ExpressionDescriptor
{
    private MethodInfo methodInfo;

    public override object Invoke(object[] parameters)
    {
        if (Assembly == null || Assembly.Instance == null)
            return null;

        if (methodInfo == null)
        {
            methodInfo = Assembly.Instance.GetType().GetMethod(MethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        if (methodInfo == null)
            return null;

        return methodInfo.Invoke(Assembly.Instance, parameters);
    }

    public MsExpressionDescriptor(MsAssemblyDescriptor assembly, string methodName) : base(assembly, methodName)
    {
    }
}