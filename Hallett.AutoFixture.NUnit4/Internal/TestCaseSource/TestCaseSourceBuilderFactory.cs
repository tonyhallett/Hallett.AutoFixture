namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    internal class TestCaseSourceBuilderFactory : ITestCaseSourceBuilderFactory<ITestCaseSource>
    {
        public ITestCaseSource FromSource(string sourceName) => new TestCaseSource(sourceName);

        public ITestCaseSource FromSource(Type sourceType, string sourceName, object?[]? methodParams) => new TestCaseSource(sourceType, sourceName, methodParams);

        public ITestCaseSource FromSource(Type sourceType, string sourceName) => new TestCaseSource(sourceType, sourceName);

        public ITestCaseSource FromSource(string sourceName, object?[]? methodParams) => new TestCaseSource(sourceName, methodParams);

        public ITestCaseSource FromSource(Type sourceType, object[] constructorParameters) => new TestCaseSource(sourceType, constructorParameters);
    }
}
