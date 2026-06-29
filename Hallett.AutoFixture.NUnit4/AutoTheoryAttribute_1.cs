using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's TheoryAttribute, with fixture factory support.
    /// </summary>
    public class AutoTheoryAttribute<TFixtureFactory>(bool searchInDeclaringTypes = false)
        : AutoCombiningStrategyAttribute<TFixtureFactory>(
            new CombinatorialStrategy(),
            new AutoParameterDataProvider(
                new ParameterDataProvider(
                    new DatapointProvider(searchInDeclaringTypes),
                    new ParameterDataSourceProvider()), false))
        where TFixtureFactory : IFixtureFactory, new()
    {
    }
}
