using AutoFixture;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal.CancelAfter;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// CanceAfterAttribute aware version of AutoFixture's AutoDataAttribute.
    /// </summary>
    public class AutoTestAttribute : AutoDataAttribute
    {
        public AutoTestAttribute() => TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();

        protected AutoTestAttribute(Func<IFixture> fixtureFactory) : base(fixtureFactory) 
            => TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }
}
