namespace Hallett.AutoFixture.Tests.Tags
{
    public record TaggedRequest(string Tag, Type TargetType)
    {
        public bool ValueIsAssignableTo(object value) => value.GetType().IsAssignableTo(TargetType);
    }
}
