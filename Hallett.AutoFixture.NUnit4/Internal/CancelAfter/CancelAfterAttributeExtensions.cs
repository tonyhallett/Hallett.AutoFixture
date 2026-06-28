using Hallett.AutoFixture.NUnit4;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.CancelAfter
{
    internal static class CancelAfterAttributeExtensions
    {
        private class RemoveCancellationTokenTestMethod : TestMethod
        {
            public RemoveCancellationTokenTestMethod(TestMethod testMethod, Test? parentSuite) : base(testMethod.Method, parentSuite)
            {
                this.Arguments = RemoveCancellationToken(testMethod.Arguments);
                RemoveCancellationTokenFromNames(testMethod.Name, parentSuite);
            }

            private static object?[] RemoveCancellationToken(object?[] arguments)
            {
                var newArguments = new object?[arguments.Length - 1];
                Array.Copy(arguments, newArguments, arguments.Length - 1);
                return newArguments;
            }

            private void RemoveCancellationTokenFromNames(string testName, Test? parentSuite)
            {
                testName = testName.Replace(",auto<CancellationToken>", string.Empty);
                Name = testName;
                FullName = parentSuite is null ? testName : $"{parentSuite.FullName}.{testName}";
            }

            public override object?[] Arguments { get; }

        }

        public static TestMethod ConsiderCancelAfterAttribute(this TestMethod test, Test? suite)
        {
            var useCancellationTokenFromNUnit = test.TestHasAllArguments()
                && test.LastParameterIsCancellationTokenAndNotForAutoFixture()
                && test.Method.HasCancelAfterAttribute();

            return useCancellationTokenFromNUnit ? new RemoveCancellationTokenTestMethod(test, suite) : test;
        }

        private static bool TestHasAllArguments(this TestMethod test)
            => test.Arguments.Length == test.Method.GetParameters().Length;

        private static bool HasCancelAfterAttribute(this IMethodInfo method)
            => method.GetCustomAttributes<CancelAfterAttribute>(true).Length > 0;

        private static bool LastParameterIsCancellationTokenAndNotForAutoFixture(this TestMethod test)
        {
            var parameters = test.Method.GetParameters();
            if (parameters.Length == 0)
            {
                return false;
            }

            var lastParameter = parameters[^1];
            return lastParameter.ParameterType == typeof(CancellationToken) && lastParameter.GetCustomAttributes<AutoAttribute>(true).Length == 0;
        }
    }
}
