using NUnit.Framework.Interfaces;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    internal interface IWithTestCaseSourceBuilder
    {
        // the IFixtureTestCaseSourceBuilder will be invoked if no issues with the test case source
        ITestBuilder WithTestCaseSourceBuilder(IFixtureTestCaseSourceBuilder fixtureTestCaseSourceBuilder);
    }
}
