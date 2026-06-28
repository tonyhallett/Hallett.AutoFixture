using AutoFixture.Kernel;
using System.Reflection;

namespace Hallett.AutoFixture.Tests.Tags
{
    public class TaggedParameterSpecification : ParameterSpecification
    {
        private class IsTaggedParameter : IEquatable<ParameterInfo>
        {
            public bool Equals(ParameterInfo? other)
            {
                return TaggedParameterAttribute.IsTaggedParameter(other!);
            }
        }
        public TaggedParameterSpecification() : base(new IsTaggedParameter()) { }
    }
}
