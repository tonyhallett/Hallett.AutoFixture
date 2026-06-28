using AutoFixture;
using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's PairwiseAttribute.
    /// </summary>
    public class AutoPairwiseAttribute : AutoCombiningStrategyAttribute
    {
        public AutoPairwiseAttribute()
            : this(() => new Fixture())
        {
        }

        protected AutoPairwiseAttribute(Func<IFixture> fixtureFactory)
            : base(new PairwiseStrategy(), new ParameterDataSourceProvider(), fixtureFactory)
        {
        }
    }
}
