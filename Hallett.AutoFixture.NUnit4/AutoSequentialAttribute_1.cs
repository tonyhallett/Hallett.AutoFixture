using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's SequentialAttribute, with fixture factory support.
    /// </summary>
    public class AutoSequentialAttribute<TFixtureFactory> : AutoCombiningStrategyAttribute<TFixtureFactory>
        where TFixtureFactory : IFixtureFactory, new()
    {
        public AutoSequentialAttribute()
            : base(new SequentialStrategy(), new ParameterDataSourceProvider())
        {
        }
    }
}
