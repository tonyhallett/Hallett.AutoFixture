using AutoFixture.Kernel;
using Moq;
using System.Reflection;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.SpecimenBuilders
{
    internal class MockStrictBehaviorSpecimenBuilder : ISpecimenBuilder
    {
        public bool Enabled { get; internal set; }

        public object Create(object request, ISpecimenContext context) => ReturnStrict(request) ? MockBehavior.Strict : new NoSpecimen();

        private bool ReturnStrict(object request) => Enabled && request is ParameterInfo parameterInfo && parameterInfo.ParameterType == typeof(MockBehavior);
    }
}