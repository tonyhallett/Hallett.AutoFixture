using AutoFixture.Kernel;
using System.Reflection;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.SpecimenBuilders
{
    internal class AbstractMockBehaviorConstructorMethod : IMethod
    {
        private readonly ConstructorInfo mockCtor;

        internal AbstractMockBehaviorConstructorMethod(ConstructorInfo mockCtor, ParameterInfo[] paramInfos)
        {
            ArgumentNullException.ThrowIfNull(mockCtor);
            ArgumentNullException.ThrowIfNull(paramInfos);
            this.mockCtor = mockCtor;
            var paramsList = new List<ParameterInfo> { mockCtor.GetParameters()[0] };
            paramsList.AddRange(paramInfos);
            Parameters = paramsList;
        }

        public IEnumerable<ParameterInfo> Parameters { get; }

        public object Invoke(IEnumerable<object> parameters)
        {
            var parametersList = parameters.ToList();
            var mockBehaviorParameter = parametersList[0];
            var paramsParameters = parametersList.Skip(1).ToArray();
            var paramsArray = new object[] { mockBehaviorParameter, paramsParameters };
            return mockCtor.Invoke(paramsArray);
        }
    }
}