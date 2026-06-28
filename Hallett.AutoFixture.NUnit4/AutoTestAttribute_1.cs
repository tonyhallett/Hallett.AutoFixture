namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// CanceAfterAttribute aware version of AutoFixture's AutoDataAttribute, with fixture factory support.
    /// </summary>
    public class AutoTestAttribute<TFixtureFactory>()
        : AutoTestAttribute(new TFixtureFactory().Create) where TFixtureFactory : IFixtureFactory, new()
    {
    }
}
