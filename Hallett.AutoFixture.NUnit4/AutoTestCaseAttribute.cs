using AutoFixture;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal.CancelAfter;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// CanceAfterAttribute aware version of AutoFixture's InlineAutoDataAttribute.
    /// </summary>
    public class AutoTestCaseAttribute : InlineAutoDataAttribute
    {
        public AutoTestCaseAttribute(params object?[] values) : base(values) 
            => TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();

        protected internal AutoTestCaseAttribute(Func<IFixture> fixtureFactory, params object?[] arguments) : base(fixtureFactory, arguments) 
            => TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }
}
