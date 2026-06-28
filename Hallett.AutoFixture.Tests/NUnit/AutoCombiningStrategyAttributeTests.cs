using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4;
using Hallett.AutoFixture.Tests.TestHelpers;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.Tests.NUnit
{
    public abstract class DatapointsBase
    {
        [Datapoint]
        internal static int Value1 = 1;

        [Datapoint]
        internal int Value2 = 2;

        [Datapoints]
        public int[] Values { get; set; } = [3, 4];

        [Datapoints]
        public static IEnumerable<int> GetValues()
        {
            yield return 5;
            yield return 6;
        }
    }


    public class AutoCombiningStrategyAttributeTests : DatapointsBase
    {
        public class TestAutoCombiningStrategyAttribute : AutoCombiningStrategyAttribute
        {
            public TestAutoCombiningStrategyAttribute(ICombiningStrategy strategy, IParameterDataProvider provider, Type type)
                : base(strategy, provider)
            {
                this.InlineAutoDataAttributeType = type;
            }
        }

        public interface ITestInterface
        {
            int Value { get; set; }
        }

        public class SUT(ITestInterface testInterface)
        {
            public void DoSomething(int value)
            {
                testInterface.Value = value;
            }
            public void DoSomethingElse(SomeValues someValues)
            {
                testInterface.Value = (int)someValues;
            }
        }

        [Test, AutoCombinatorial(InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        public void AutoCombinatorial([Values(1, 2)] int value, [Auto][Frozen] ITestInterface testInterface, SUT sut)
        {
            sut.DoSomething(value);
            Assert.That(testInterface.Value, Is.EqualTo(value));
        }

        [Test, CancelAfter(1000), AutoCombinatorial(InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        public void NUnitCancellationTokenTest([Values(2, 4)] int value, [Auto][Frozen] ITestInterface testInterface, SUT sut, CancellationToken nUnitCancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(nUnitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
                Assert.That(testInterface, Is.Not.Null);
                Assert.That(sut, Is.Not.Null);
                Assert.That(value, Is.InRange(2, 4));
            });
        }

        [Test, AutoCombinatorial()]
        public void AutoCombiningStrategyAttribute_Can_Freeze([Frozen][Values(1, 2)] int value, [Values("One")] string str, [Auto] int fromAutoFixture, string fromAutoFixtureStr)
        {
            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo(fromAutoFixture));
                Assert.That(str, Is.Not.EqualTo(fromAutoFixtureStr));
            });
        }

        [Test, AutoCombinatorial<MoqFixtureFactory>()]
        public void AutoCombiningStrategyAttributeT_Can_Freeze([Frozen][Values(1, 2)] int value, [Values("One")] string str, [Auto] int fromAutoFixture, string fromAutoFixtureStr)
        {
            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo(fromAutoFixture));
                Assert.That(str, Is.Not.EqualTo(fromAutoFixtureStr));
            });
        }


        public enum SomeValues { Value1, Value2, Value3, Value4 }


        [Test, AutoTheory(InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        public void TheoryWithoutAttributesUsage(SomeValues value, [Auto][Frozen] ITestInterface testInterface, SUT sut)
        {
            sut.DoSomethingElse(value);
            Assert.That(testInterface.Value, Is.EqualTo((int)value));
        }

        [Test, AutoTheory(true, InlineAutoDataAttributeType = typeof(MoqTestCaseAttribute))]
        public void TheoryWithAttributesUsage(int datapoint, [Auto][Frozen] ITestInterface testInterface, SUT sut)
        {
            sut.DoSomething(datapoint);
            Assert.That(testInterface.Value, Is.EqualTo(datapoint));
        }
    }
}
