using AutoFixture.Kernel;
using Hallett.AutoFixture.Moq.EnhancedCustomization.Helpers;
using System.Reflection;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.Parameters
{
    internal class MockStrictParameterRelay(bool defaultIsStrict) : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is ParameterInfo parameterInfo)
            {
                var parameterType = parameterInfo.ParameterType;
                if (parameterType.IsMock() && IsStrict(parameterInfo))
                {
                    return context.Resolve(new MockStrictRequest(parameterType));
                }
            }

            // will be relayed to regular type request

            return new NoSpecimen();
        }

        private bool IsStrict(ParameterInfo parameterInfo)
        {
            var attribute = parameterInfo.GetCustomAttributes().OfType<IMockParameterBehavior>().FirstOrDefault()
            ?? parameterInfo.Member.GetCustomAttributes().OfType<IMockParameterBehavior>().FirstOrDefault()
            ?? parameterInfo.Member.DeclaringType?.GetCustomAttributes().OfType<IMockParameterBehavior>().FirstOrDefault();
            return attribute != null ? attribute.Strict : defaultIsStrict;
        }
    }
}