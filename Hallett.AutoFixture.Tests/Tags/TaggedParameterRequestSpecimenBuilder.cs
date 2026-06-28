using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Tags
{
    public class TaggedParameterRequestSpecimenBuilder(Dictionary<string, object> parameters) : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is TaggedRequest taggedRequest &&
                parameters.TryGetValue(taggedRequest.Tag, out var value) &&
                taggedRequest.ValueIsAssignableTo(value))
            {
                return value;
            }

            return new NoSpecimen();
        }
    }
}
