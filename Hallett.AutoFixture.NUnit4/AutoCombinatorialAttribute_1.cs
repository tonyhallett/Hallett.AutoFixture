using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's CombinatorialAttribute, with fixture factory support.
    /// </summary>
    public class AutoCombinatorialAttribute<TFixtureFactory> : AutoCombiningStrategyAttribute<TFixtureFactory>
        where TFixtureFactory : IFixtureFactory, new()
    {
        public AutoCombinatorialAttribute()
            : base(new CombinatorialStrategy(), new ParameterDataSourceProvider())
        {
        }
    }
}
