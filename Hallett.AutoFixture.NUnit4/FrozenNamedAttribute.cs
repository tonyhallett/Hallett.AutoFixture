using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// Similar to FrozenAttribute with Matching.PropertyName or Matching.FieldName, but with a specific name to match not tied to the test parameter name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isProperty"></param>
    public class FrozenNamedAttribute(string name, bool isProperty = true)
        : CustomizeAttribute, IFreezeTestCaseArgument
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
            => new FreezeOnMatchCustomization(parameter, GetRequestSpecification(parameter));

        public IRequestSpecification GetRequestSpecification(ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            var derivesFromCriterion = Criterions.DerivesFrom(parameterType);
            var isNamedCriterion = Criterions.IsNamedExactly(name);

            IRequestSpecification requestSpecification = isProperty ? new PropertySpecification(
                new PropertyTypeAndNameCriterion(
                    derivesFromCriterion,
                    isNamedCriterion)) :

                new FieldSpecification(
                    new FieldTypeAndNameCriterion(
                    derivesFromCriterion,
                    isNamedCriterion));

            return new OrRequestSpecification(requestSpecification, new EqualRequestSpecification(parameter));
        }
    }
}
