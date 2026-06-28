using NUnit.Framework.Interfaces;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
    internal interface ITestCaseSource
    {
        IEnumerable<object[]>? GetCases(IMethodInfo method);
    }
}
