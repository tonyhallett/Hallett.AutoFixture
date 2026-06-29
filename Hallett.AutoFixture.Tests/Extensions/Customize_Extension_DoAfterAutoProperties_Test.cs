using AutoFixture;

namespace Hallett.AutoFixture.Tests.Extensions
{
    internal class Customize_Extension_DoAfterAutoProperties_Test
    {
        public class Contact
        {
            public long Id { get; set; }

            public string FirstName { get; set; } = "";

            public IReadOnlyDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        [Test]
        public void TestAutoPropertiesThenDo()
        {
            var fixture = new Fixture();
            fixture.Customize<Contact>(composer => composer.With(contact => contact.FirstName, "First name"), (contact) =>
            {
                contact.Properties = new Dictionary<string, object> { ["firstname"] = contact.FirstName };
            });

            Assert.That(fixture.Create<Contact>().Properties["firstname"], Is.EqualTo("First name"));
        }
    }
}
