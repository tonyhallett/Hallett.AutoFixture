using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    // the IFixtureTestCaseSourceBuilder will be invoked if no issues with the test case source
    internal interface IFixtureTestCaseSourceBuilder
    {
        TestMethod BuildFrom(IMethodInfo method, Test? suite, object?[] testCaseArguments);
    }
}
