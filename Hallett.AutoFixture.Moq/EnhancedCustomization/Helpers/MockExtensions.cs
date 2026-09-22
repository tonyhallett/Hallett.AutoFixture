using Moq;
using System.Reflection;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.Helpers
{
    public static class MockExtensions
    {
        internal static bool IsMock(this Type type)
        {
            return type != null
                   && type.GetTypeInfo().IsGenericType
                   && typeof(Mock<>).IsAssignableFrom(type.GetGenericTypeDefinition())
                   && !type.GetMockedType().IsGenericParameter;
        }

        internal static Type GetMockedType(this Type type) => type.GetTypeInfo().GetGenericArguments().Single();

        internal static ConstructorInfo GetDefaultConstructor(this Type type) => type.GetConstructor(Type.EmptyTypes)!;

        internal static IEnumerable<ConstructorInfo> GetPublicAndProtectedConstructors(this Type type)
            => type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(ctor => !ctor.IsPrivate);
    }
}