using AutoFixture;
using Hallett.AutoFixture.NUnit4;

namespace Hallett.AutoFixture.Tests.NUnit
{
    internal class AutoTest_TestCase_Support_CancelAfter_Tests
    {
        [Test, CancelAfter(1000), AutoTest]
        public void AutoDataCancelAttribute_Cancels_Test(
            IFixture fixture,
            CancellationToken nunitCancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
                Assert.That(fixture, Is.Not.Null);
            });
        }

        [Test, CancelAfter(1000), AutoTestCase(1)]
        public void InlineAutoDataCancelAttribute_Cancels_Test(
            int inline,
            IFixture fixture,
            CancellationToken nunitCancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
                Assert.That(inline, Is.EqualTo(1));
                Assert.That(fixture, Is.Not.Null);
            });
        }
    }
}
