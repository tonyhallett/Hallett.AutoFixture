using AutoFixture;
using Hallett.AutoFixture.NUnit4.Internal.TestCases;
using Hallett.AutoFixture.NUnit4.Internal.TestCaseSource;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture version of NUnit's TestCaseSourceAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AutoTestCaseSourceAttribute : Attribute, IImplyFixture, ITestBuilder, IFixtureTestCaseSourceBuilder
    {
        internal ITestCaseTestMethodCreator TestCaseTestMethodCreator { get; set; } = new TestCaseTestMethodCreator();

        internal ITestCaseSourceTestBuilder TestCaseSourceTestBuilder { get; set; } = new TestCaseSourceTestBuilder();

        private readonly Func<IFixture> fixtureFactory;
        private readonly Lazy<ITestBuilder> fixtureTestCasesBuilderLazy;

        public AutoTestCaseSourceAttribute(string sourceName) : this(() => new Fixture(), sourceName) { }

        protected AutoTestCaseSourceAttribute(Func<IFixture> fixtureFactory, string sourceName)
        {
            this.fixtureFactory = fixtureFactory;
            fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceName).WithTestCaseSourceBuilder(this));
        }

        public AutoTestCaseSourceAttribute(Type sourceType, string sourceName, object?[]? methodParams)
            : this(() => new Fixture(), sourceType, sourceName, methodParams) { }

        protected AutoTestCaseSourceAttribute(Func<IFixture> fixtureFactory, Type sourceType, string sourceName, object?[]? methodParams)
        {
            this.fixtureFactory = fixtureFactory;
            fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, sourceName, methodParams).WithTestCaseSourceBuilder(this));
        }

        public AutoTestCaseSourceAttribute(Type sourceType, string sourceName) : this(() => new Fixture(), sourceType, sourceName) { }

        protected AutoTestCaseSourceAttribute(Func<IFixture> fixtureFactory, Type sourceType, string sourceName)
        {
            this.fixtureFactory = fixtureFactory;
            fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, sourceName).WithTestCaseSourceBuilder(this));
        }

        public AutoTestCaseSourceAttribute(string sourceName, object?[]? methodParams) : this(() => new Fixture(), sourceName, methodParams) { }

        protected AutoTestCaseSourceAttribute(Func<IFixture> fixtureFactory, string sourceName, object?[]? methodParams)
        {
            this.fixtureFactory = fixtureFactory;
            fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceName, methodParams).WithTestCaseSourceBuilder(this));
        }

        public AutoTestCaseSourceAttribute(Type sourceType, params object[] constructorParameters) : this(() => new Fixture(), sourceType, constructorParameters) { }

        protected AutoTestCaseSourceAttribute(Func<IFixture> fixtureFactory, Type sourceType, params object[] constructorParameters)
        {
            this.fixtureFactory = fixtureFactory;
            fixtureTestCasesBuilderLazy = new Lazy<ITestBuilder>(() => TestCaseSourceTestBuilder.FromSource(sourceType, constructorParameters).WithTestCaseSourceBuilder(this));
        }

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite) => fixtureTestCasesBuilderLazy.Value.BuildFrom(method, suite);

        public TestMethod BuildFrom(IMethodInfo method, Test? suite, object?[] testCaseArguments)
        {
            var testMethod = TestCaseTestMethodCreator.Create(fixtureFactory, InlineAutoDataAttributeType, method, suite, testCaseArguments);

            if (Category is not null)
            {
                testMethod.Properties.Add(PropertyNames.Category, Category);
            }

            return testMethod;
        }

        public string? Category { get; set; }

        /// <summary>
        /// Supply to reuse the IFixture from a custom InlineAutoDataAttribute type with single constructor that takes a params object[] arguments parameter. 
        /// </summary>
        public Type? InlineAutoDataAttributeType { get; set; }
    }
}
