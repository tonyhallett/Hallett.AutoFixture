using Hallett.AutoFixture.NUnit4;

namespace Hallett.AutoFixture.Tests.TestHelpers
{
    public class MoqTestCaseAttribute(params object?[] arguments) : AutoTestCaseAttribute(() => new MoqFixture(), arguments)
    {
    }
}
