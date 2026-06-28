using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal;
using Moq;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4.Moq
{
    public enum FrozenMockMatching
    {
        ParameterName,
        PropertyName,
        FieldName
    }

    /// <summary>
    /// This is mainly for demonstration purposes.
    /// </summary>
    /// <param name="matching"></param>
    public class FrozenMockAttribute(FrozenMockMatching matching = FrozenMockMatching.ParameterName) : CustomizeAttribute
    {
        class FrozenMockCustomization(ParameterInfo mockParameterInfo, Type mockedType, FrozenMockMatching matching) : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                var (specimenBuilder, mockedObject) = FreezeMock(fixture);
                fixture.Customizations.Add(new FilteringSpecimenBuilder(specimenBuilder, new EqualRequestSpecification(mockParameterInfo)));
                var parameterName = mockParameterInfo.Name!;
                if (parameterName.StartsWith("mock", StringComparison.OrdinalIgnoreCase))
                {
                    parameterName = parameterName[4..];
                }

                var isTypeCriterion = Criterions.IsType(mockedType);
                var isNamedCriterion = Criterions.IsNamed(parameterName);
                IRequestSpecification mockedObjectRequestSpecification = matching switch
                {
                    FrozenMockMatching.ParameterName => new ParameterSpecification(new ParameterTypeAndNameCriterion(isTypeCriterion, isNamedCriterion)),
                    FrozenMockMatching.PropertyName => new PropertySpecification(new PropertyTypeAndNameCriterion(isTypeCriterion, isNamedCriterion)),
                    FrozenMockMatching.FieldName => new FieldSpecification(new FieldTypeAndNameCriterion(isTypeCriterion, isNamedCriterion)),
                    _ => throw new InvalidOperationException($"Unknown matching type: {matching}")
                };

                fixture.Customizations.Add(new FilteringSpecimenBuilder(new FixedBuilder(mockedObject), mockedObjectRequestSpecification));
            }

            private (ISpecimenBuilder specimenBuilder, object mockedObject) FreezeMock(IFixture fixture)
            {
                var context = new SpecimenContext(fixture);
                var specimen = context.Resolve(mockParameterInfo);
                var fixedBuilder = new FixedBuilder(specimen);
                var mockedObject = (specimen as Mock)!.Object;
                return (fixedBuilder, mockedObject);
            }
        }

        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            var mockedType = GetMockedType(parameter);
            return new FrozenMockCustomization(parameter, mockedType, matching);
        }

        private static Type GetMockedType(ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Mock<>))
            {
                return parameterType.GetGenericArguments()[0];
            }

            throw new InvalidOperationException("FrozenMockAttribute applied to parameter that is not a Mock type");
        }
    }
}
