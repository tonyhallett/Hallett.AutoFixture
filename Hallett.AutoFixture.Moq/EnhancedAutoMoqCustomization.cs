using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;

namespace Hallett.AutoFixture.Moq
{
    public enum AutoPropertiesBehaviour { FollowFixture, Omit, Enable }

    /// <summary>
    /// An enhanced version of AutoMoqCustomization that allows for more control over mock configuration, including the ability to intercept mock creation and control auto-properties behavior on the created Mock.Object
    /// </summary>
    public class EnhancedAutoMoqCustomization : ICustomization
    {
        private ISpecimenBuilder _relay;
        private readonly Dictionary<Type, Action<Mock>> interceptors = [];

        public EnhancedAutoMoqCustomization() => _relay = new MockRelay();

        public bool ConfigureMembers { get; set; }

        public AutoPropertiesBehaviour AutoPropertiesBehaviour { get; set; } = AutoPropertiesBehaviour.FollowFixture;

        public bool GenerateDelegates { get; set; }

        public ISpecimenBuilder Relay
        {
            get => _relay;
            set => _relay = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Intercepts the creation of a mock of type T and allows for custom configuration of the mock before it is returned by AutoFixture. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="interceptor"></param>
        public void Intercept<T>(Action<Mock<T>> interceptor) where T : class
        {
            var mockedType = GetGenericArgumentType(GetGenericArgumentType(interceptor.GetType()));
            interceptors.Add(mockedType, mock => interceptor((Mock<T>)mock));
        }

        public void Customize(IFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);

            ISpecimenBuilder mockBuilder = new MockPostprocessor(
                new MethodInvoker(
                    new MockConstructorQuery()));

            if (ConfigureMembers)
            {
                mockBuilder = new Postprocessor(
                    builder: mockBuilder,
                    command: GetConfigureMembersCommand(fixture));
            }

            fixture.Customizations.Add(mockBuilder);
            fixture.ResidueCollectors.Add(Relay);

            if (GenerateDelegates)
            {
                fixture.Customizations.Add(new MockRelay(new DelegateSpecification()));
            }
        }

        private CompositeSpecimenCommand GetConfigureMembersCommand(IFixture fixture)
        {
            var setupMockCommand = new ActionSpecimenCommand<Mock>(mock =>
            {
                var mockedType = GetGenericArgumentType(mock.GetType());
                if (interceptors.TryGetValue(mockedType, out var interceptor))
                {
                    interceptor(mock);
                }
            });

            if (ShouldAutoMockProperties(fixture))
            {
                return new CompositeSpecimenCommand(
                        new StubPropertiesCommand(),
                        new MockVirtualMethodsCommand(),
                        new AutoMockPropertiesCommand(),
                        setupMockCommand
                        );
            }

            return new CompositeSpecimenCommand(
                new StubPropertiesCommand(),
                new MockVirtualMethodsCommand(),
                setupMockCommand
                );
        }

        private bool ShouldAutoMockProperties(IFixture fixture) => AutoPropertiesBehaviour == AutoPropertiesBehaviour.FollowFixture ?
                !fixture.OmitAutoProperties : AutoPropertiesBehaviour != AutoPropertiesBehaviour.Omit;

        private static Type GetGenericArgumentType(Type mockType) => mockType.GetGenericArguments()[0];
    }
}