using AutoFixture;
using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Behaviour
{
    internal class DemoCustomEngineTest
    {
        record ResolvedInfo(object ResolvedValue, ISpecimenBuilder WrappedSpecimenBuilder);

        class WrappingEngineParts : DefaultEngineParts
        {
            private readonly Dictionary<object, ResolvedInfo> resolvedBys = [];

            public class WrapperSpecimenBuilder(ISpecimenBuilder inner) : ISpecimenBuilder
            {
                public object Create(object request, ISpecimenContext context)
                {
                    var resolved = inner.Create(request, context);
                    if (resolved is not NoSpecimen && resolved is not OmitSpecimen)
                    {
                        CreateCallback?.Invoke(resolved, inner);
                    }
                    return resolved;
                }

                public Action<object, ISpecimenBuilder>? CreateCallback { get; set; }
            }
            public WrappingEngineParts() : base(WrapPrimitive())
            {
                foreach (var wrapper in this.OfType<WrapperSpecimenBuilder>())
                {
                    wrapper.CreateCallback = Created;
                }
            }

            public void Created(object resolved, ISpecimenBuilder resolvedBy)
            {
                if (!resolvedBys.ContainsKey(resolved))
                {
                    resolvedBys.Add(resolved, new ResolvedInfo(resolved, resolvedBy));
                }
            }
            private static List<ISpecimenBuilder> WrapPrimitive()
                => [.. new DefaultPrimitiveBuilders().Select(pb => new WrapperSpecimenBuilder(pb) as ISpecimenBuilder)];

            public List<ResolvedInfo> GetResolved() => [.. resolvedBys.Values];
        }

        [Test]
        public void Demo_Which_Primitive_SpecimenBuilder_Resolves()
        {
            var wrappingEngineParts = new WrappingEngineParts();
            var fixture = new Fixture(wrappingEngineParts);
            var randomInt = fixture.Create<int>();
            var generatedString = fixture.Create<string>();
            var switchedBool = fixture.Create<bool>();

            var resolved = wrappingEngineParts.GetResolved();
            Assert.That(resolved, Has.Count.EqualTo(3));
            var booleanSwitchResolved = resolved.Single(r => r.WrappedSpecimenBuilder is BooleanSwitch);
            Assert.That(switchedBool, Is.EqualTo(booleanSwitchResolved.ResolvedValue));
            var randomNumericSequenceGeneratorResolved = resolved.Single(r => r.WrappedSpecimenBuilder is RandomNumericSequenceGenerator);
            Assert.That(randomInt, Is.EqualTo(randomNumericSequenceGeneratorResolved.ResolvedValue));
            var stringGeneratorResolved = resolved.Single(r => r.WrappedSpecimenBuilder is StringGenerator);
            Assert.That(generatedString, Is.SameAs(stringGeneratorResolved.ResolvedValue));
        }
    }
}
