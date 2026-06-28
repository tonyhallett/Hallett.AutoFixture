using AutoFixture;
using AutoFixture.AutoMoq;

namespace Hallett.AutoFixture.Tests.TestHelpers
{
    public class MoqFixture : Fixture
    {
        public MoqFixture(bool configureMembers = true, bool generateDelegates = true, bool omitAutoProperties = true)
        {
            Customize(new AutoMoqCustomization { ConfigureMembers = configureMembers, GenerateDelegates = generateDelegates });
            OmitAutoProperties = omitAutoProperties;
        }
    }
}
