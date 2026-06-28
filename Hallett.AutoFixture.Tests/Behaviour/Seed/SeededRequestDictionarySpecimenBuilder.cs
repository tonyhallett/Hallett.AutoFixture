using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Behaviour.Seed
{
    public class SeededRequestDictionarySpecimenBuilder(Dictionary<object, object> seedValues) : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is SeededRequest seededRequest && seededRequest.Seed != null)
            {
                if (seedValues.TryGetValue(seededRequest.Seed, out var value))
                {
                    return value;
                }
            }
            return new NoSpecimen();
        }
    }
}
