using AutoFixture.NUnit4;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.CancelAfter
{
    // keeping in case it is discovered that there is a problem with the extension method
    // not that this would need to change to match the full behaviour of the extension method.
    //public class FixedNameTestMethodBuilderCancellationTokenAwareOld : ITestMethodBuilder
    //{
    //    public virtual TestMethod Build(
    //        IMethodInfo method, Test suite, IEnumerable<object> parameterValues, int autoDataStartIndex)
    //    {
    //        var testCaseParameters = AdjustParametersForCancellationTokenIfNeeded(
    //            GetParametersForMethod(method, parameterValues, autoDataStartIndex), method, autoDataStartIndex);
    //        return new NUnitTestCaseBuilder()
    //            .BuildTestMethod(method, suite, testCaseParameters);
    //    }


    //    private static TestCaseParameters GetParametersForMethod(
    //        IMethodInfo method, IEnumerable<object> parameterValues, int autoDataStartIndex)
    //    {
    //        var getParametersForMethodMethod = typeof(FixedNameTestMethodBuilder).GetMethod("GetParametersForMethod", BindingFlags.NonPublic | BindingFlags.Static, [typeof(IMethodInfo), typeof(IEnumerable<object>), typeof(int)]);
    //        return (TestCaseParameters)getParametersForMethodMethod?.Invoke(null, [method, parameterValues, autoDataStartIndex])!;
    //    }

    //    private static TestCaseParameters AdjustParametersForCancellationTokenIfNeeded(TestCaseParameters parameters, IMethodInfo method, int autoDataStartIndex)
    //    {
    //        if(RemoveLastParameter(method))
    //        {
    //            var adjustedParameters = new object?[parameters.Arguments.Length - 1];
    //            Array.Copy(parameters.Arguments, adjustedParameters, parameters.Arguments.Length - 1);

    //            var adjustedTestCaseParameters = new TestCaseParameters(adjustedParameters);
    //            for(var i=autoDataStartIndex;i<parameters.OriginalArguments.Length - 1;i++)
    //            {
    //                adjustedTestCaseParameters.OriginalArguments[i] = parameters.OriginalArguments[i];
    //            }
    //            return adjustedTestCaseParameters;
    //        }

    //        return parameters;
    //    }

    //    public static bool RemoveLastParameter(IMethodInfo method)
    //    {
    //        return LastParameterAcceptsCancellationToken(method.GetParameters()) && MethodHasCancelAfterAttribute(method);
    //    }

    //    public static bool MethodHasCancelAfterAttribute(IMethodInfo method)
    //    {
    //        return method.GetCustomAttributes<CancelAfterAttribute>(true).Length > 0;
    //    }

    //    public static bool LastParameterAcceptsCancellationToken(IParameterInfo[] parameters)
    //    {
    //        return parameters.Length > 0 && parameters[^1].ParameterType == typeof(CancellationToken);
    //    }
    //}

    internal class FixedNameTestMethodBuilderCancellationTokenAware : ITestMethodBuilder
    {
        private readonly FixedNameTestMethodBuilder fixedNameTestMethodBuilder = new();
        public virtual TestMethod Build(
            IMethodInfo method, Test suite, IEnumerable<object> parameterValues, int autoDataStartIndex)
        {
            return fixedNameTestMethodBuilder.Build(method, suite, parameterValues, autoDataStartIndex).ConsiderCancelAfterAttribute(suite);
        }
    }
}
