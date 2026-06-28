using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    internal class TestCaseSourceTestBuilder(ITestCaseSourceBuilderFactory<ITestCaseSource> testCaseSourceBuilderFactory) :
        ITestCaseSourceTestBuilder,
        IWithTestCaseSourceBuilder,
        ITestBuilder
    {
        private ITestCaseSource? _testCaseSource;
        private IFixtureTestCaseSourceBuilder? fixtureTestCaseBuilder;
        private readonly NUnitTestCaseBuilder nunitTestCaseBuilder = new();

        private ITestCaseSource TestCaseSourceSource => _testCaseSource ?? throw new InvalidOperationException("TestCaseSourceBuilder is not set. Call FromSource() first.");
        private IFixtureTestCaseSourceBuilder FixtureTestCaseSourceBuilder => fixtureTestCaseBuilder ?? throw new InvalidOperationException("FixtureTestCaseSourceBuilder is not set. Call WithBuilder() first.");

        public TestCaseSourceTestBuilder() : this(new TestCaseSourceBuilderFactory())
        {
        }

        public IWithTestCaseSourceBuilder FromSource(string sourceName)
        {
            _testCaseSource = testCaseSourceBuilderFactory.FromSource(sourceName);
            return this;
        }

        public IWithTestCaseSourceBuilder FromSource(string sourceName, object?[]? methodParams)
        {
            _testCaseSource = testCaseSourceBuilderFactory.FromSource(sourceName, methodParams);
            return this;
        }

        public IWithTestCaseSourceBuilder FromSource(Type sourceType, object[] constructorParameters)
        {
            _testCaseSource = testCaseSourceBuilderFactory.FromSource(sourceType, constructorParameters);
            return this;
        }

        public IWithTestCaseSourceBuilder FromSource(Type sourceType, string sourceName)
        {
            _testCaseSource = testCaseSourceBuilderFactory.FromSource(sourceType, sourceName);
            return this;
        }

        public IWithTestCaseSourceBuilder FromSource(Type sourceType, string sourceName, object?[]? methodParams)
        {
            _testCaseSource = testCaseSourceBuilderFactory.FromSource(sourceType, sourceName, methodParams);
            return this;
        }

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
        {
            try
            {
                var testCaseArguments = TestCaseSourceSource.GetCases(method);
                if (testCaseArguments == null)
                {
                    return [];
                }

                return testCaseArguments.Select(testCaseArguments => FixtureTestCaseSourceBuilder.BuildFrom(method, suite, testCaseArguments));
            }
            catch (TestCaseSourceException exception)
            {
                var exceptionTestCaseParameters = new TestCaseParameters() { RunState = RunState.NotRunnable };
                exceptionTestCaseParameters.Properties.Set(PropertyNames.SkipReason, exception.Message);

                return [nunitTestCaseBuilder.BuildTestMethod(method, suite, exceptionTestCaseParameters)];
            }
            catch (Exception exception)
            {
                // don't expect this path
                return [nunitTestCaseBuilder.BuildTestMethod(method, suite, new TestCaseParameters(exception))];
            }
        }

        public ITestBuilder WithTestCaseSourceBuilder(IFixtureTestCaseSourceBuilder fixtureTestCaseSourceBuilder)
        {
            this.fixtureTestCaseBuilder = fixtureTestCaseSourceBuilder;
            return this;
        }
    }
}
