using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System.Collections;

namespace Hallett.AutoFixture.NUnit4.Internal.Strategy
{
    internal class AutoCombiningStrategyHelper : IAutoCombiningStrategyHelper
    {
        private readonly NUnitTestCaseBuilder _builder = new();

        public IEnumerable<TestMethod> BuildFrom(
            IMethodInfo method,
            Test? suite,
            ICombiningStrategy strategy,
            IAutoParameterDataProvider provider,
            Func<ITestCaseData, TestMethod> testMethodFromTestCaseData)
        {
            List<TestMethod> tests = [];

            IParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length > 0)
            {
                IEnumerable[] sources = new IEnumerable[provider.NumberOfParameters(method)];

                try
                {
                    for (int i = 0; i < sources.Length; i++)
                    {
                        sources[i] = provider.GetDataFor(parameters[i]);
                    }
                }
                catch (InvalidDataSourceException ex)
                {
                    var parms = new TestCaseParameters
                    {
                        RunState = RunState.NotRunnable
                    };
                    parms.Properties.Set(PropertyNames.SkipReason, ex.Message);
                    tests.Add(_builder.BuildTestMethod(method, suite, parms));
                    return tests;
                }

                foreach (var parms in strategy.GetTestCases(sources))
                {
                    tests.Add(testMethodFromTestCaseData(parms));
                }
            }

            return tests;
        }

        public void ApplyStrategyTypeNameToTest(Test test, ICombiningStrategy strategy)
        {
            var joinType = strategy.GetType().Name;
            if (joinType.EndsWith("Strategy", StringComparison.Ordinal))
                joinType = joinType[..^8];

            test.Properties.Set(PropertyNames.JoinType, joinType);
        }
    }
}
