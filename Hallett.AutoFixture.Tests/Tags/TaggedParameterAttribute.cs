using System.Reflection;

namespace Hallett.AutoFixture.Tests.Tags
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class TaggedParameterAttribute(string tag) : Attribute
    {
        public string Tag => tag;

        public static bool IsTaggedParameter(ParameterInfo parameterInfo) => GetAttribute(parameterInfo) != null;

        private static TaggedParameterAttribute? GetAttribute(ParameterInfo parameterInfo) => parameterInfo.GetCustomAttribute<TaggedParameterAttribute>();

        public static string GetTag(ParameterInfo parameterInfo)
        {
            var taggedParameterAttribute = parameterInfo.GetCustomAttribute<TaggedParameterAttribute>();
            return taggedParameterAttribute == null
                ? throw new InvalidOperationException($"Parameter {parameterInfo.Name} is not tagged with {nameof(TaggedParameterAttribute)}")
                : taggedParameterAttribute.Tag;
        }
    }

}
