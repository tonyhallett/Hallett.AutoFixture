using AutoFixture;

namespace Hallett.AutoFixture.Tests.Behaviour.Seed
{
    internal class DemoSeededRequestSpecimenBuilderTest
    {
        record SeededProperty(string Id) { }

        class Seeded
        {
            public SeededProperty? Property1 { get; set; }

            public SeededProperty? Property2 { get; set; }
        }



        [Test]
        public void Demo_SeededRequest_Handling()
        {
            var fixture = new Fixture();

            fixture.Customizations.Add(
                 new UpfrontSeededRequestSpecimenBuilder<SeededProperty>(
                     new Dictionary<object, SeededProperty> {
                        { nameof(Seeded.Property1), new SeededProperty("Value1") },
                        { nameof(Seeded.Property2), new SeededProperty("Value2") }
                 }));

            var seeded = fixture.Create<Seeded>();

            Assert.Multiple(() =>
            {
                Assert.That(seeded.Property1!.Id, Is.EqualTo("Value1"));
                Assert.That(seeded.Property2!.Id, Is.EqualTo("Value2"));
            });
        }
    }
}
