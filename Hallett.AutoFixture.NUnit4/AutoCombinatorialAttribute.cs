using AutoFixture;
using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's CombinatorialAttribute.
    /// </summary>
    public class AutoCombinatorialAttribute : AutoCombiningStrategyAttribute
    {
        public AutoCombinatorialAttribute()
            : this(() => new Fixture())
        {
        }

        protected AutoCombinatorialAttribute(Func<IFixture> fixtureFactory)
            : base(new CombinatorialStrategy(), new ParameterDataSourceProvider(), fixtureFactory)
        {
        }
    }
}
