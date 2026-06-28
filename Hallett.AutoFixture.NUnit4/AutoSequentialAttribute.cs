using AutoFixture;
using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's SequentialAttribute.
    /// </summary>
    public class AutoSequentialAttribute : AutoCombiningStrategyAttribute
    {
        public AutoSequentialAttribute()
            : this(() => new Fixture())
        {
        }

        protected AutoSequentialAttribute(Func<IFixture> fixtureFactory)
            : base(new SequentialStrategy(), new ParameterDataSourceProvider(), fixtureFactory)
        {
        }
    }
}
