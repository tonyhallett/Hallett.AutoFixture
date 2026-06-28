using AutoFixture;
using AutoFixture.AutoMoq;

namespace Hallett.AutoFixture.Tests.TestHelpers
{
    internal class MoqFixtureFactory : FixtureFactory
    {
        protected override IFixture CustomizeFixture(IFixture fixture)
        {
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });
            return SetUpFixture(fixture);
        }

        protected virtual IFixture SetUpFixture(IFixture fixture) { return fixture; }
    }
}
