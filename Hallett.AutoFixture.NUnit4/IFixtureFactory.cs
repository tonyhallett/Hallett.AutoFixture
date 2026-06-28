using AutoFixture;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// Defines a factory for creating <see cref="IFixture"/> instances.  Used by generic attributes, should pass where: TFixtureFactory : IFixtureFactory, new()
    /// </summary>
    public interface IFixtureFactory
    {
        IFixture Create();
    }
}
