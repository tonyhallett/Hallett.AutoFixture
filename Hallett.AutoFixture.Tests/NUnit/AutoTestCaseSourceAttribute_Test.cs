using AutoFixture;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4;
using Hallett.AutoFixture.Tests.TestHelpers;
using Moq;
using NUnit.Framework.Internal;
using System.Collections;

namespace Hallett.AutoFixture.Tests.NUnit
{
    internal class AutoTestCaseSourceAttribute_Test
    {
        class FixedIntFactory : IFixtureFactory
        {
            public IFixture Create()
            {
                var fixture = new Fixture();
                fixture.Inject(42);
                return fixture;
            }
        }

        public class TestCaseSourceWithParameters(int a, int b) : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { a, b };
                yield return new object[] { a + 1, b + 1 };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class SUT(IDivider divider)
        {
            public int DoDivide(int a, int b) => divider.Divide(a, b);
        }

        public interface IDivider
        {
            int Divide(int a, int b);
        }

        [Test]
        [AutoTestCaseSource(nameof(DivideCases), InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        [AutoTestCaseSource<MoqFixtureFactory>(nameof(DivideCases3))]
        public void Should_Work_With_IEnumerableT_Of_ObjectArray(int a, int b, int expected, [Frozen] Mock<IDivider> mockDivider, SUT sut)
        {
            mockDivider.Setup(x => x.Divide(a, b)).Returns(expected);
            var result = sut.DoDivide(a, b);
            Assert.That(result, Is.EqualTo(expected));
            mockDivider.Verify(x => x.Divide(a, b), Times.Once);
        }

        [Test]
        [AutoTestCaseSource<FixedIntFactory>(nameof(EnumerableOfEnumerableCases))]
        public void Should_Work_With_IEnumerable_Of_IEnumerable(int value, int fromFixture)
        {
            Assert.Multiple(() =>
            {
                Assert.That(value, Is.LessThan(3));
                Assert.That(fromFixture, Is.EqualTo(42));
            });
        }

        [Test]
        [AutoTestCaseSource<FixedIntFactory>(nameof(ListOfListCases))]
        public void Should_Work_With_List_Of_List(int value, int fromFixture)
        {
            Assert.Multiple(() =>
            {
                Assert.That(value, Is.LessThan(3));
                Assert.That(fromFixture, Is.EqualTo(42));
            });
        }


        [Test] // remove parameter to see RunState.NotRunnable with PropertyNames.SkipReason
        [AutoTestCaseSource<FixedIntFactory>(typeof(TestCaseSourceWithParameters), 1, 2)]
        public void Should_Work_With_Test_Case_Source_Type(int a, int b, int fromAutoFixture)
        {
            Assert.Multiple(() =>
            {
                Assert.That(a, Is.LessThan(1 + 2));
                Assert.That(b, Is.LessThan(2 + 2));
                Assert.That(fromAutoFixture, Is.EqualTo(42));
            });
        }

        [Test, AutoTestCaseSource<FixedIntFactory>(nameof(WonkyCases))]
        public void Should_Work_With_Different_TestCase_Argument_Lengths(int first, int maybeFromFixture)
            => Assert.That(maybeFromFixture, Is.EqualTo(first == 1 ? 42 : 3));

        [Test, CancelAfter(1000)]
        [AutoTestCaseSource(nameof(DivideCases), InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        [AutoTestCaseSource<MoqFixtureFactory>(nameof(DivideCases3))]
        public void Should_Work_CanceAfter(int a, int b, int expected, [Frozen] Mock<IDivider> mockDivider, SUT sut, CancellationToken nunitCancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));

                Assert.That(a / b, Is.EqualTo(expected));
                Assert.That(mockDivider, Is.Not.Null);
                Assert.That(sut, Is.Not.Null);
            });
        }

        [Test]
        [AutoTestCaseSource(nameof(DivideCases))]
        public void Should_Work_With_Factory(int a, int b, int expected, IFixture fixture)
        {
            var mockDivider = new Mock<IDivider>();
            mockDivider.Setup(x => x.Divide(a, b)).Returns(expected);
            fixture.Inject(mockDivider.Object);

            var sut = fixture.Create<SUT>();
            Assert.That(sut.DoDivide(a, b), Is.EqualTo(expected));
        }

        [Test, CancelAfter(1000)]
        [AutoTestCaseSource(nameof(DivideCases))]
        public void Should_Work_With_Factory_CancelAfter(int a, int b, int expected, IFixture fixture, CancellationToken nunitCancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
                Assert.That(a / b, Is.EqualTo(expected));
                Assert.That(fixture, Is.Not.Null);
            });
        }

        public class WithFrozenParameter(int frozenInt)
        {
            public int FrozenInt => frozenInt;
        }

        [Test, AutoTestCaseSource(nameof(FrozenParameterCases))]
        public void AutoTestCaseSourceCanFreezeTestCaseSourceParameters(
            [Frozen] int frozenTestCaseSourceParameter, WithFrozenParameter withFrozenParameter)
        {
            Assert.That(withFrozenParameter.FrozenInt, Is.EqualTo(frozenTestCaseSourceParameter));
        }

        [Test, AutoTestCaseSource(nameof(FrozenParameterCases))]
        public void AutoTestCaseSourceCanFreezeTestCaseSourceParameters_IFreezeTestCaseSourceArguments(
            [FrozenParameterIndex(typeof(WithFrozenParameter))] int frozenTestCaseSourceParameter,
            WithFrozenParameter withFrozenParameter)
        {
            Assert.That(withFrozenParameter.FrozenInt, Is.EqualTo(frozenTestCaseSourceParameter));
        }

        [Test, AutoTestCaseSource(nameof(FrozenParameterCases), InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        public void AutoTestCaseSourceCanFreezeTestCaseSourceParameters_Reflection(
            [Frozen] int frozenTestCaseSourceParameter, WithFrozenParameter withFrozenParameter)
        {
            Assert.That(withFrozenParameter.FrozenInt, Is.EqualTo(frozenTestCaseSourceParameter));
        }

        [Test, AutoTestCaseSource<FixtureFactory>(nameof(FrozenParameterCases))]
        public void AutoTestCaseSourceTCanFreezeTestCaseSourceParameters(
            [Frozen] int frozenTestCaseSourceParameter,
            WithFrozenParameter withFrozenParameter)
        {
            Assert.That(withFrozenParameter.FrozenInt, Is.EqualTo(frozenTestCaseSourceParameter));
        }

        internal static object[] EnumerableOfEnumerableCases =
        [
            new object[] { 1 },
            new object[] { 2 }
        ];

        internal static List<List<int>> ListOfListCases =
        [
            [1],
            [2]
        ];

        internal static IEnumerable<object[]> FrozenParameterCases =
        [
            [42],
            [99]
        ];

        internal static IEnumerable<object[]> DivideCases =
        [
            [12, 3, 4],
            [12, 2, 6],
            [12, 4, 3]
        ];

        internal static IEnumerable<object[]> DivideCases2 =
        [
            [16, 4, 4],
            [15, 3, 5],
            [18, 2, 9]
        ];

        internal static IEnumerable<object[]> DivideCases3 =
        [
            [21, 3, 7],
        ];

        internal static IEnumerable<object[]> WonkyCases = [
            [1],
            [2, 3]
        ];
    }
}
