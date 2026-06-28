using AutoFixture;
using Hallett.AutoFixture.NUnit4;

namespace Hallett.AutoFixture.Tests.TestHelpers
{
    internal class FixtureFactory : IFixtureFactory
    {
        public IFixture Create() => CustomizeFixture(new Fixture());

        protected virtual IFixture CustomizeFixture(IFixture fixture) => fixture;
    }
}
