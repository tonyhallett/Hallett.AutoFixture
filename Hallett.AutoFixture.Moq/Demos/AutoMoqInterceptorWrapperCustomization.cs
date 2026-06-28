using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Moq;

namespace Hallett.AutoFixture.Moq.Demos
{
    // not to be used. For knowledge demonstration purposes.
    internal class AutoMoqInterceptorWrapperCustomization(AutoMoqCustomization autoMoqCustomization) : ICustomization
    {
        class FakeFixture : IFixture
        {
            public IList<ISpecimenBuilderTransformation> Behaviors { get; } = [];
            public IList<ISpecimenBuilder> Customizations { get; } = [];
            public bool OmitAutoProperties { get; set; }
            public int RepeatCount { get; set; }
            public IList<ISpecimenBuilder> ResidueCollectors { get; } = [];

            public ICustomizationComposer<T> Build<T>()
            {
                throw new NotImplementedException();
            }

            public object Create(object request, ISpecimenContext context)
            {
                throw new NotImplementedException();
            }

            public IFixture Customize(ICustomization customization)
            {
                customization.Customize(this);
                return this;
            }

            public void Customize<T>(Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation)
            {
                throw new NotImplementedException();
            }
        }

        class MockSetupSpecimenBuilder(ISpecimenBuilder moqSpecimenBuilder, Action<Mock> setupMock) : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                var resolved = moqSpecimenBuilder.Create(request, context);
                if (resolved is not NoSpecimen)
                {
                    setupMock((Mock)resolved);
                }
                return resolved;
            }
        }

        private readonly Dictionary<Type, Action<Mock>> interceptors = [];

        public void Intercept<T>(Action<Mock<T>> interceptor) where T : class
        {
            var mockedType = GetGenericArgumentType(GetGenericArgumentType(interceptor.GetType()));
            interceptors.Add(mockedType, mock => interceptor((Mock<T>)mock));
        }

        private static Type GetGenericArgumentType(Type mockType)
        {
            return mockType.GetGenericArguments()[0];
        }

        public void Customize(IFixture fixture)
        {
            var fakeFixture = new FakeFixture();
            fakeFixture.Customize(autoMoqCustomization);
            var moqResidueCollectors = fakeFixture.ResidueCollectors;
            var moqCustomizations = fakeFixture.Customizations;
            var mockBuilder = moqCustomizations.First();
            foreach (var moqResidueCollector in moqResidueCollectors)
            {
                fixture.ResidueCollectors.Add(moqResidueCollector);
            }
            if (moqCustomizations.Count > 1)
            {
                foreach (var moqCustomization in moqCustomizations.Skip(1))
                {
                    fixture.Customizations.Add(moqCustomization);
                }
            }
            fixture.Customizations.Add(new MockSetupSpecimenBuilder(mockBuilder, mock =>
            {
                var mockedType = GetGenericArgumentType(mock.GetType());
                if (interceptors.TryGetValue(mockedType, out var interceptor))
                {
                    interceptor(mock);
                }
            }));
        }
    }
}

