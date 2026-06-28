using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's TheoryAttribute, with fixture factory support.
    /// </summary>
    public class AutoTheoryAttribute<TFixtureFactory> : AutoCombiningStrategyAttribute<TFixtureFactory>
        where TFixtureFactory : IFixtureFactory, new()
    {
        public AutoTheoryAttribute(bool searchInDeclaringTypes = false)
            : base(new CombinatorialStrategy(), new ParameterDataProvider(new DatapointProvider(searchInDeclaringTypes), new ParameterDataSourceProvider()))
        {
        }
    }
}
