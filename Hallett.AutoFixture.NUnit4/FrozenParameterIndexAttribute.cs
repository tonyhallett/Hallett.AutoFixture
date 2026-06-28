using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// Similar to FrozenAttribute with Matching.ParameterName, but instead of matching by the test parameter name, it matches by the name of a parameter in the constructor of the specified typeWithParameter.
    /// </summary>
    /// <param name="typeWithParameter">The type to search for parameter and use the parameter name</param>
    /// <param name="index">When multiple matching parameters, the index of the parameter to use.  If the second is required provide 1, this is not the index of the parameter. </param>
    public class FrozenParameterIndexAttribute(Type typeWithParameter, int index = 0)
        : CustomizeAttribute, IFreezeTestCaseArgument
    {
        public Type MethodQueryType { get; set; } = typeof(ModestConstructorQuery);

        public override ICustomization GetCustomization(ParameterInfo parameter)
            => new FreezeOnMatchCustomization(parameter, GetRequestSpecification(parameter));

        private string GetParameterName(Type parameterType)
        {
            var parameters = GetFirstParametersOfType(parameterType);
            return parameters[index].Name ?? throw new InvalidOperationException($"{parameters.Count} found, index argument {index} too large.");
        }

        private List<ParameterInfo> GetFirstParametersOfType(Type parameterType)
        {
            List<ParameterInfo>? selectedParameters = null;
            foreach (var parameters in GetParametersOfTypeForMethods(parameterType))
            {
                if (parameters.Count > 0)
                {
                    selectedParameters = parameters;
                    break;
                }
            }

            return selectedParameters ?? throw new InvalidOperationException($"No methods with parameters of type {parameterType.Name}");
        }

        private IEnumerable<List<ParameterInfo>> GetParametersOfTypeForMethods(Type parameterType)
            => SelectMethods().Select(constructorMethod => constructorMethod.Parameters.Where(p => p.ParameterType.IsAssignableFrom(parameterType)).ToList());

        private IEnumerable<IMethod> SelectMethods() => ConstructMethodQuery().SelectMethods(typeWithParameter);

        private IMethodQuery ConstructMethodQuery() =>
            !MethodQueryType.IsAssignableTo(typeof(IMethodQuery))
                ? throw new InvalidOperationException("MethodQueryType property is not IMethodQuery")
                : (IMethodQuery)Activator.CreateInstance(MethodQueryType)!;

        public IRequestSpecification GetRequestSpecification(ParameterInfo parameter)
        {
            var name = GetParameterName(parameter.ParameterType);

            var requestSpecification = new ParameterSpecification(
               new ParameterTypeAndNameCriterion(
                   Criterions.DerivesFrom(parameter.ParameterType),
                   Criterions.IsNamedExactly(name)));

            return new OrRequestSpecification(requestSpecification, new EqualRequestSpecification(parameter));
        }
    }
}
