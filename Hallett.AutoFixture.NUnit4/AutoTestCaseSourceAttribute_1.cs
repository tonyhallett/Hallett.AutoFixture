using Hallett.AutoFixture.NUnit4.Internal.TestCases;
using Hallett.AutoFixture.NUnit4.Internal.TestCaseSource;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture version of NUnit's TestCaseSourceAttribute, with fixture factory support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AutoTestCaseSourceAttribute<TFixtureFactory>
        : Attribute, IImplyFixture, ITestBuilder, IFixtureTestCaseSourceBuilder where TFixtureFactory : IFixtureFactory, new()
    {
        internal ITestCaseTestMethodCreator TestCaseTestMethodCreator { get; set; } = new TestCaseTestMethodCreator();

        internal ITestCaseSourceTestBuilder TestCaseSourceTestBuilder { get; set; } = new TestCaseSourceTestBuilder();

        private readonly Lazy<ITestBuilder> fixtureTestCasesBuilderLazy;
        public AutoTestCaseSourceAttribute(string sourceName)

            => fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceName).WithTestCaseSourceBuilder(this));

        public AutoTestCaseSourceAttribute(Type sourceType, string sourceName, object?[]? methodParams)
            => fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, sourceName, methodParams).WithTestCaseSourceBuilder(this));

        public AutoTestCaseSourceAttribute(Type sourceType, string sourceName)
            => fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, sourceName).WithTestCaseSourceBuilder(this));

        public AutoTestCaseSourceAttribute(string sourceName, object?[]? methodParams)
            => fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceName, methodParams).WithTestCaseSourceBuilder(this));

        public AutoTestCaseSourceAttribute(Type sourceType, params object[] constructorParameters)
            => fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, constructorParameters).WithTestCaseSourceBuilder(this));

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
            => fixtureTestCasesBuilderLazy.Value.BuildFrom(method, suite);

        public TestMethod BuildFrom(IMethodInfo method, Test? suite, object?[] testCaseArguments)
        {
            var testMethod = TestCaseTestMethodCreator.Create<TFixtureFactory>(method, suite, testCaseArguments);
            if (Category is not null)
            {
                testMethod.Properties.Add(PropertyNames.Category, Category);
            }

            return testMethod;
        }

        public string? Category { get; set; }
    }
}
