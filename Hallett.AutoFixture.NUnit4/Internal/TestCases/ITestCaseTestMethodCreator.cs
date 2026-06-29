using AutoFixture;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCases
{
    internal interface ITestCaseTestMethodCreator
    {
        public TestMethod Create(
            Func<IFixture>? fixtureFactory,
            Type? InlineAutoDataAttributeType,
            IMethodInfo method,
            Test? suite,
            object?[] testCaseArguments);

        TestMethod Create<TFixtureFactory>(IMethodInfo method, Test? suite, object?[] testCaseArguments)
            where TFixtureFactory : IFixtureFactory, new();
    }
}
