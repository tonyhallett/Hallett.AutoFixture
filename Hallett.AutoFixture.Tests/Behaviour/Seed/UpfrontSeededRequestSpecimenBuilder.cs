using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Behaviour.Seed
{
    public class UpfrontSeededRequestSpecimenBuilder<T>(Dictionary<object, T> seedValues) : ISpecimenBuilder where T : notnull
    {
        private readonly FilteringSpecimenBuilder filteringSpecimenBuilder = new(
                new SeededRequestDictionarySpecimenBuilder(
                    seedValues.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value)
                    ), new SeedRequestSpecification(typeof(T)));

        public object Create(object request, ISpecimenContext context)
        {
            return filteringSpecimenBuilder.Create(request, context);
        }
    }
}
