using AutoFixture;
using NUnit.Framework.Internal.Builders;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// AutoFixture enabled version of NUnit's TheoryAttribute.
    /// </summary>
    public class AutoTheoryAttribute : AutoCombiningStrategyAttribute
    {
        public AutoTheoryAttribute(bool searchInDeclaringTypes = false)
            : this(() => new Fixture(), searchInDeclaringTypes)
        {
        }

        protected AutoTheoryAttribute(Func<IFixture> fixtureFactory, bool searchInDeclaringTypes = false)
            : base(new CombinatorialStrategy(),
                   new AutoParameterDataProvider(
                       new ParameterDataProvider(
                           new DatapointProvider(searchInDeclaringTypes),
                           new ParameterDataSourceProvider()), false),
                   fixtureFactory)
        {
        }
    }
}
