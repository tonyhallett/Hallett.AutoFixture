using AutoFixture;
using AutoFixture.NUnit4;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCases
{
    internal static class InlineAutoDataAttributeReflectionExtensions
    {
        private static readonly PropertyInfo inlineAutoDataAttributeFixtureProperty = typeof(InlineAutoDataAttribute).GetProperty("Fixture", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static IFixture GetFixture(this InlineAutoDataAttribute attribute)
        {
            return (IFixture)inlineAutoDataAttributeFixtureProperty.GetValue(attribute)!;
        }
    }
}
