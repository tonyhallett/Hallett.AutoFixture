using Hallett.AutoFixture.NUnit4;

namespace Hallett.AutoFixture.Tests.TestHelpers
{
    public sealed class MoqTestAttribute(
        bool configureMembers = true,
        bool generateDelegates = true,
        bool omitAutoProperties = true) : AutoTestAttribute(() => new MoqFixture(configureMembers, generateDelegates, omitAutoProperties))
    { }
}
