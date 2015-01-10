
using System;
namespace Minimum.Proxy
{
    public interface IProxy
    {
        Type Original { get; }
        Interceptor Interceptor { get; }
    }

    public static class IProxyExtensions
    {
        public static void Add(this IProxy instance, string methodToIntercept, Func<object, object[], object> executeBefore, Run runTimes = Run.Once)
        {
            Execute function = new Execute();
            function.MethodName = methodToIntercept;
            function.Before = executeBefore;
            function.Run = runTimes;
            function.When = When.Before;

            instance.Interceptor.Add(function);
        }

        public static void Add(this IProxy instance, string methodToIntercept, Func<object, object[], object, object> executeAfter, Run runTimes = Run.Once)
        {
            Execute function = new Execute();
            function.MethodName = methodToIntercept;
            function.After = executeAfter;
            function.Run = runTimes;
            function.When = When.After;

            instance.Interceptor.Add(function);
        }
    }
}
