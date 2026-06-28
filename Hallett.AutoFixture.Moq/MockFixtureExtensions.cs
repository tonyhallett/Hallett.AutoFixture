using AutoFixture;
using Hallett.AutoFixture;
using Moq;

namespace Hallett.AutoFixture.Moq
{
    public static class MockFixtureExtensions
    {
        /// <summary>
        /// Injects a new mock of type <typeparamref name="T"/> into the fixture.
        /// Bypassing the AutoMoqCustomization
        /// </summary>
        /// <typeparam name="T">The type to mock.</typeparam>
        /// <param name="fixture">The fixture to inject the mock into.</param>
        /// <returns>The created mock.</returns>
        public static Mock<T> InjectMock<T>(this IFixture fixture) where T : class => fixture.InjectGetBack(new Mock<T>());

        /// <summary>
        /// Freezes a mock of type <typeparamref name="T"/> in the fixture.
        /// The AutoMoqCustomizatiion will configure the mock.
        /// </summary>
        /// <typeparam name="T">The type to mock.</typeparam>
        /// <param name="fixture">The fixture to freeze the mock in.</param>
        /// <returns>The frozen mock.</returns>
        public static Mock<T> FreezeMock<T>(this IFixture fixture) where T : class => fixture.Freeze<Mock<T>>();
    }
}
