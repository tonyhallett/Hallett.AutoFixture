using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Hallett.AutoFixture.Moq.EnhancedCustomization.Helpers;
using Moq;
using System.Reflection;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.SpecimenBuilders
{
    internal class MockConstructorWithBehaviorQuery : IMethodQuery
    {
        private readonly MockConstructorQuery mockConstructorQuery = new();

        public bool IsStrict { get; set; }

        public IEnumerable<IMethod> SelectMethods(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!type.IsMock())
            {
                return [];
            }

            Type mockedType = type.GetMockedType();

            if (!IsStrict)
            {
                return mockConstructorQuery.SelectMethods(type);
            }

            if (mockedType.GetTypeInfo().IsInterface)
            {
                var constructor = type.GetConstructor([typeof(MockBehavior)]);
                return GetSingleConstructorArray(new(constructor));
            }

            return from ci in mockedType.GetPublicAndProtectedConstructors()
                   let paramInfos = ci.GetParameters()
                   orderby paramInfos.Length
                   select (IMethod)new AbstractMockBehaviorConstructorMethod(GetMockBehaviorParamsConstructor(type), paramInfos);
        }

        private static ConstructorMethod[] GetSingleConstructorArray(ConstructorMethod constructorMethod) => [constructorMethod];

        private static ConstructorInfo GetMockBehaviorParamsConstructor(Type type)
            => type.GetConstructor([typeof(MockBehavior), typeof(object[])])!;
    }
}