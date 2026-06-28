namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// CanceAfterAttribute aware version of AutoFixture's InlineAutoDataAttribute, with fixture factory support.
    /// </summary>
    public class AutoTestCaseAttribute<TFixtureFactory>(params object?[] arguments)
        : AutoTestCaseAttribute(new TFixtureFactory().Create, arguments) where TFixtureFactory : IFixtureFactory, new()
    {
    }
}
