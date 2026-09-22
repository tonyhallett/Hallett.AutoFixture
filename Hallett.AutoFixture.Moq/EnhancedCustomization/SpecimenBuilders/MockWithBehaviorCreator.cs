using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Hallett.AutoFixture.Moq.EnhancedCustomization.Parameters;

namespace Hallett.AutoFixture.Moq.EnhancedCustomization.SpecimenBuilders
{
    internal class MockWithBehaviorCreator : ISpecimenBuilder
    {
        private readonly MockStrictBehaviorSpecimenBuilder mockStrictBehaviorSpecimenBuilder;
        private readonly MockConstructorWithBehaviorQuery mockConstructorWithBehaviorQuery;
        private readonly MockPostprocessor mockCreator;
        private readonly bool defaultIsStrict;

        public MockWithBehaviorCreator(bool defaultIsStrict, MockStrictBehaviorSpecimenBuilder mockStrictBehaviorSpecimenBuilder)
        {
            this.defaultIsStrict = defaultIsStrict;
            this.mockStrictBehaviorSpecimenBuilder = mockStrictBehaviorSpecimenBuilder;
            mockConstructorWithBehaviorQuery = new MockConstructorWithBehaviorQuery();
            mockCreator = new MockPostprocessor(
                // this creates the mock if the request was for a Mock<T>
                // the MockPostProcessor further configures
                new MethodInvoker(mockConstructorWithBehaviorQuery));
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is MockStrictRequest mockStrictRequest)
            {
                mockConstructorWithBehaviorQuery.IsStrict = true;
                return EnableStrict(() => mockCreator.Create(mockStrictRequest.MockType, context));
            }

            mockConstructorWithBehaviorQuery.IsStrict = defaultIsStrict;
            return mockCreator.Create(request, context);
        }

        private object EnableStrict(Func<object> createMock)
        {
            mockStrictBehaviorSpecimenBuilder.Enabled = true;
            var mock = createMock();
            mockStrictBehaviorSpecimenBuilder.Enabled = false;
            return mock;
        }
    }
}