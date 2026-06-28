namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    internal interface ITestCaseSourceBuilderFactory<TReturn>
    {
        TReturn FromSource(string sourceName);

        TReturn FromSource(string sourceName, object?[]? methodParams);

        TReturn FromSource(Type sourceType, object[] constructorParameters);

        TReturn FromSource(Type sourceType, string sourceName);

        TReturn FromSource(Type sourceType, string sourceName, object?[]? methodParams);
    }
}
