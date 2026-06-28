using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4;
using NUnit.Framework.Interfaces;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCases
{
    internal class TestCaseArgumentFreezer : ITestCaseArgumentFreezer
    {
        internal class FrozenValueCustomization(IRequestSpecification specification, object? value) : ICustomization
        {
            private readonly IRequestSpecification _specification = specification ?? throw new ArgumentNullException(nameof(specification));

            public void Customize(IFixture fixture)
            {
                var builder = new FilteringSpecimenBuilder(
                    builder: new FixedBuilder(value),
                    specification: _specification);

                fixture.Customizations.Insert(0, builder);
            }
        }

        public void ApplyFreezingAttributes(Func<IFixture> getFixture, IMethodInfo method, object?[] testCaseArguments)
        {
            var frozenAttributeArgs = method.GetParameters()
                    .Zip(testCaseArguments, (param, testCaseArg) => (requestSpecification: GetRequestSpecification(param), testCaseArg, param))
                    .Where(a => a.requestSpecification != null)
                    .Select(a => (a.requestSpecification!, a.testCaseArg, a.param)).ToList();
            if (frozenAttributeArgs.Count > 0)
            {
                var fixture = getFixture();
                foreach (var (requestSpecification, testCaseArg, param) in frozenAttributeArgs)
                {
                    fixture.Customize(new FrozenValueCustomization(requestSpecification, testCaseArg));
                }
            }
        }

        private static IRequestSpecification? GetRequestSpecification(IParameterInfo param)
        {
            var frozenAttributes = param.GetCustomAttributes<FrozenAttribute>(false);
            if (frozenAttributes.Length == 1)
            {
                var freezeOnMatchCustomization = (FreezeOnMatchCustomization)frozenAttributes[0].GetCustomization(param.ParameterInfo);
                return freezeOnMatchCustomization.Matcher;
            }

            var freezeTestCaseArgumentAttributes = param.GetCustomAttributes<Attribute>(false).OfType<IFreezeTestCaseArgument>().ToArray();
            if (freezeTestCaseArgumentAttributes.Length == 1)
            {
                return freezeTestCaseArgumentAttributes[0].GetRequestSpecification(param.ParameterInfo);
            }

            return null;
        }
    }
}
