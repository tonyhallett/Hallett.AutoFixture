using Hallett.AutoFixture.NUnit4.Internal.Strategy;
using Hallett.AutoFixture.NUnit4.Internal.TestCases;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's CombiningStrategyAttribute, with fixture factory support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class AutoCombiningStrategyAttribute<TFixtureFactory>(ICombiningStrategy strategy, IParameterDataProvider provider)
        : Attribute, ITestBuilder, IApplyToTest where TFixtureFactory : IFixtureFactory, new()
    {
        internal IAutoCombiningStrategyHelper AutoCombiningStrategyHelper { get; set; } = new AutoCombiningStrategyHelper();
        internal ITestCaseTestMethodCreator TestCaseTestMethodCreator { get; set; } = new TestCaseTestMethodCreator();
        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
            => AutoCombiningStrategyHelper.BuildFrom(
                    method,
                    suite,
                    strategy,
                    provider,
                    (testCaseData) => TestCaseTestMethodCreator.Create<TFixtureFactory>(method, suite, testCaseData.Arguments));

        public void ApplyToTest(Test test) => AutoCombiningStrategyHelper.ApplyStrategyTypeNameToTest(test, strategy);
    }
}
