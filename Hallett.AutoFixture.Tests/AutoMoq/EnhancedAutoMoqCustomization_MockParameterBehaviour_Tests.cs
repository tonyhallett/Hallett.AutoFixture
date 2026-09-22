using AutoFixture;
using AutoFixture.Kernel;
using Hallett.AutoFixture.Moq;
using Moq;

namespace Hallett.AutoFixture.Tests.AutoMoq
{
    public class EnhancedAutoMoqCustomization_MockParameterBehaviour_Tests
    {
        public abstract class MockedBase(int p1, int p2) { 
            public int P1 => p1;
            public int P2 => p2;
        }

        public interface IMocked { }

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
        internal class MockBehaviourNoClassAttribute
        {
            public void ParameterAttribute([MockParameterStrict] Mock<IMocked> mockParameterStrict)
            { }

            [MockParameterStrict]
            public void MethodAttributeOnly(Mock<IMocked> mockMethodStrict1, Mock<IMocked> mockMethodStrict2)
            { }

            [MockParameterStrict]
            public void ParameterOverride(Mock<IMocked> mockMethodStrict, [MockParameterLoose] Mock<IMocked> mockParameterLoose)
            { }

            public void NoAttribute(Mock<IMocked> mockNoAttributes)
            { }
        }

        [MockParameterStrict]
        internal class MockBehaviourStrictClass
        {
            public void NoAttribute(Mock<IMocked> mockClassStrict)
            { }

            [MockParameterLoose]
            public void MethodOverride(Mock<IMocked> mockMethodLoose)
            { }

            public void ParameterOverride([MockParameterLoose] Mock<IMocked> mockParameterLoose)
            { }
        }

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1822 // Mark members as static

        [Test]
        public void Should_Create_Strict_Mocks_When_MockBehaviourAttribute_Applied_To_Parameter_Is_Strict()
            => AssertStrict<MockBehaviourNoClassAttribute>(nameof(MockBehaviourNoClassAttribute.ParameterAttribute), [true]);

        [Test]
        public void Should_Create_Strict_Mocks_When_MockBehaviourAttribute_Applied_To_Method_Only_Is_Strict()
            => AssertStrict<MockBehaviourNoClassAttribute>(nameof(MockBehaviourNoClassAttribute.MethodAttributeOnly), [true, true]);

        [Test]
        public void Should_Override_Method_Attribute_When_MockBehaviourAttribute_Applied_To_Parameter()
            => AssertStrict<MockBehaviourNoClassAttribute>(nameof(MockBehaviourNoClassAttribute.ParameterOverride), [true, false]);

        [Test]
        public void Should_Create_Strict_Mocks_When_MockBehaviourAttribute_Applied_To_Class_Is_Strict()
            => AssertStrict<MockBehaviourStrictClass>(nameof(MockBehaviourStrictClass.NoAttribute), [true]);

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Create_Mocks_With_MockBehaviour_From_DefaultLoose_When_No_Attribute(bool defaultIsStrict)
            => AssertStrict<MockBehaviourNoClassAttribute>(nameof(MockBehaviourNoClassAttribute.NoAttribute), [defaultIsStrict], defaultIsStrict);

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Use_DefaultLoose_When_Mock_From_Type_Request_Interface(bool defaultIsStrict)
        {
            var resolved = GetFixture(defaultIsStrict).Create<IMocked>();
            var mock = Mock.Get(resolved);
            var mockIsStrict = mock.Behavior == MockBehavior.Strict;
            Assert.That(mockIsStrict, Is.EqualTo(defaultIsStrict));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Use_DefaultLoose_When_Mock_From_Type_Request_Abstract(bool defaultIsStrict)
        {
            var resolved = GetFixture(defaultIsStrict).Create<MockedBase>();
            var mock = Mock.Get(resolved);
            var mockIsStrict = mock.Behavior == MockBehavior.Strict;
            Assert.That(mockIsStrict, Is.EqualTo(defaultIsStrict));
        }


        private static void AssertStrict<T>(string methodName, IEnumerable<bool> strictExpectations, bool defaultIsStrict = false) where T : class
        {
            var specimenContext = GetFixtureSpecimenContext(defaultIsStrict);
            var parameters = typeof(T).GetMethod(methodName)!.GetParameters();
            var resolved = parameters.Select(p => specimenContext.Resolve(p)).ToList();
            var resolvedMocks = resolved.OfType<Mock<IMocked>>().ToList();
            var stricts = resolvedMocks.Select(m => m.Behavior == MockBehavior.Strict).ToList();
            Assert.That(stricts, Is.EqualTo(strictExpectations));
        }

        private static SpecimenContext GetFixtureSpecimenContext(bool defaultIsStrict) 
            => new(GetFixture(defaultIsStrict));

        private static Fixture GetFixture(bool defaultIsStrict)
        {
            var fixture = new Fixture();
            var autoMoqInterceptorCustomization = new EnhancedAutoMoqCustomization() { DefaultIsStrict = defaultIsStrict };
            fixture.Customize(autoMoqInterceptorCustomization);
            return fixture;
        }
    }
}
