using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;

namespace Hallett.AutoFixture
{
    public static class FixtureExtensions
    {
        /// <summary>
        /// Injects a value into the fixture and returns it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fixture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T InjectGetBack<T>(this IFixture fixture, T value)
        {
            fixture.Inject(value);
            return value;
        }

        /// <summary>
        /// Inject twice, once as the base type and once as the derived type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="fixture"></param>
        /// <param name="value"></param>
        public static void InjectAs<T, TInstance>(this IFixture fixture, TInstance value) where T : class where TInstance : T
        {
            fixture.Inject<T>(value);
            fixture.Inject(value);
        }

        // https://github.com/AutoFixture/AutoFixture/issues/988
        /// <summary>
        /// Customizes the fixture with a composer transformation and an action to perform after auto-properties are set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fixture"></param>
        /// <param name="composerTransformation"></param>
        /// <param name="doAfterAutoProperties"></param>
        /// <returns></returns>
        public static IFixture Customize<T>(
            this IFixture fixture,
            Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation,
            Action<T> doAfterAutoProperties)
        {
            var composed = composerTransformation(SpecimenBuilderNodeFactory.CreateComposer<T>().WithAutoProperties(!fixture.OmitAutoProperties));
            var customization = new Postprocessor(composed, new ActionSpecimenCommand<T>(doAfterAutoProperties));
            fixture.Customizations.Insert(0, customization);
            return fixture;
        }
    }
}
