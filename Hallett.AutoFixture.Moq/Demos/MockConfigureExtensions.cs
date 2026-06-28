using AutoFixture;
using Moq;
using Moq.Language.Flow;
using System.Linq.Expressions;

namespace Hallett.AutoFixture.Moq.Demos
{
    // internal as an example to StackOverflow question
    internal static class MockConfigureExtensions
    {
        public class MockConfigure<T>(IEnumerable<T> mockedInstances) where T : class
        {
            public MockConfigure<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, Action<int, ISetupGetter<T, TProperty>> configure)
            {
                var index = 0;
                foreach (var mocked in mockedInstances)
                {
                    configure(index++, Mock.Get(mocked).SetupGet(expression));
                }
                return this;
            }

            public IEnumerable<T> AllWithPropertyValue<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
            {
                return SetupProperty(expression, (_, setup) => setup.Returns(value)).GetValues();
            }

            public IEnumerable<T> GetValues() => mockedInstances;
        }

        public static MockConfigure<T> CreateManyConfigureMocks<T>(this IFixture fixture, int count = 3) where T : class
        {
            var mocks = fixture.CreateMany<T>(count);
            return new MockConfigure<T>(mocks);
        }
    }
}
