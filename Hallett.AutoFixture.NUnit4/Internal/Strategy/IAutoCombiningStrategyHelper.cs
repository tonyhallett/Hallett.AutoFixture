using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.Strategy
{
    public interface IAutoCombiningStrategyHelper
    {
        IEnumerable<TestMethod> BuildFrom(
            IMethodInfo method,
            Test? suite,
            ICombiningStrategy strategy,
            IParameterDataProvider provider,
            Func<ITestCaseData, TestMethod> testMethodFromTestCaseData);

        void ApplyStrategyTypeNameToTest(Test test, ICombiningStrategy strategy);
    }
}
