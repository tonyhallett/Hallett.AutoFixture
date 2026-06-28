using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's PairwiseAttribute, with fixture factory support.
    /// </summary>
    public class AutoPairwiseAttribute<TFixtureFactory> : AutoCombiningStrategyAttribute<TFixtureFactory>
        where TFixtureFactory : IFixtureFactory, new()
    {
        public AutoPairwiseAttribute()
            : base(new PairwiseStrategy(), new ParameterDataSourceProvider())
        {
        }
    }
}
