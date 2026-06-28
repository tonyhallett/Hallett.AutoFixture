using AutoFixture;
using NUnit.Framework.Interfaces;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCases
{
    internal interface ITestCaseArgumentFreezer
    {
        void ApplyFreezingAttributes(Func<IFixture> getFixture, IMethodInfo method, object?[] testCaseArguments);
    }
}