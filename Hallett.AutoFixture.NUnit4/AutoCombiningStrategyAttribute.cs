using AutoFixture;
using Hallett.AutoFixture.NUnit4.Internal.Strategy;
using Hallett.AutoFixture.NUnit4.Internal.TestCases;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's CombiningStrategyAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class AutoCombiningStrategyAttribute(ICombiningStrategy strategy, IAutoParameterDataProvider provider, Func<IFixture>? fixtureFactory = null)
        : Attribute, ITestBuilder, IApplyToTest
    {
        internal IAutoCombiningStrategyHelper AutoCombiningStrategyHelper { get; set; } = new AutoCombiningStrategyHelper();
        internal ITestCaseTestMethodCreator TestCaseTestMethodCreator { get; set; } = new TestCaseTestMethodCreator();
        public Type? InlineAutoDataAttributeType { get; set; }

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
        {
            ThrowIfNoFixtureFactoryOrInlineAutoDataAttributeType();

            return AutoCombiningStrategyHelper.BuildFrom(method, suite, strategy, provider, (testCaseData) =>
            {
                return TestCaseTestMethodCreator.Create(fixtureFactory, InlineAutoDataAttributeType, method, suite, testCaseData.Arguments);
            });
        }

        private void ThrowIfNoFixtureFactoryOrInlineAutoDataAttributeType()
        {
            if (InlineAutoDataAttributeType == null && fixtureFactory == null)
            {
                throw new InvalidOperationException("InlineAutoDataAttributeType or fixtureFactory constructor argument must be set.");
            }
        }

        public void ApplyToTest(Test test) => AutoCombiningStrategyHelper.ApplyStrategyTypeNameToTest(test, strategy);
    }
}
