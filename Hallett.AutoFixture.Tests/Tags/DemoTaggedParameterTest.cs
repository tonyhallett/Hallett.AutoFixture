using AutoFixture;
using AutoFixture.NUnit4;

namespace Hallett.AutoFixture.Tests.Tags
{
    public class DemoTaggedParameterTest
    {
        internal class TaggedCustomizationFixture : AutoDataAttribute
        {
            public TaggedCustomizationFixture() : base(CreateFixture) { }

            private static IFixture CreateFixture()
            {
                var fixture = new Fixture();
                return fixture.Customize(
                    new TaggedParameterCustomization(
                        new Dictionary<string, object> {
                        { "Tag1", new Tagged("Tag1") },
                        { "Tag2", new Tagged("Tag2") } }));
            }
        }

        public record class Tagged(string Id);

        [Test, TaggedCustomizationFixture]
        public void Should_Get_Tagged_From_Customization(
            [Frozen] string frozenString,
            [TaggedParameter("Tag2")] Tagged tagged2,
            [TaggedParameter("Tag1")] Tagged tagged1,
            Tagged fromMethodInvoker
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(tagged1.Id, Is.EqualTo("Tag1"));
                Assert.That(tagged2.Id, Is.EqualTo("Tag2"));
                Assert.That(fromMethodInvoker.Id, Is.EqualTo(frozenString));
            });
        }
    }
}
