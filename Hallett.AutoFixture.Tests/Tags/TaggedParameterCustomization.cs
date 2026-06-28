using AutoFixture;
using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Tags
{
    class TaggedParameterCustomization(Dictionary<string, object> taggedObjects) : CompositeCustomization(
        new TaggedParameterRequestSpecimenBuilder(taggedObjects).ToCustomization(),
        new FilteringSpecimenBuilder(
                new TaggedParameterRelay(),
                new TaggedParameterSpecification()).ToCustomization()
            )
    {
    }
}
