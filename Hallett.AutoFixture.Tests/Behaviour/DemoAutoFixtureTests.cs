using AutoFixture;

namespace Hallett.AutoFixture.Tests.Behaviour
{
    internal class DemoAutoFixtureTests
    {
        public class WithPrimitiveDependency(string primitiveDependency)
        {
            public string PrimitiveDependency => primitiveDependency;
        }

        [Test]
        public void DefaultEnginePartsAreNotJustForAutoProperties()
        {
            new Fixture().Do<WithPrimitiveDependency>(wpd => Assert.That(wpd.PrimitiveDependency, Is.Not.Null));
        }

        [Test]
        public void AutoFixture_Can_Provide_Collection_Interfaces()
        {
            var list = new Fixture().Create<IList<string>>();
            Assert.That(list, Has.Count.EqualTo(3));
        }

        class FactoryParameter
        {
            public string HelloWho { get; internal set; } = "World";
        }

        class WithFactory
        {
            private readonly FactoryParameter factoryParameter;

            private WithFactory(FactoryParameter factoryParameter)
            {
                this.factoryParameter = factoryParameter;
            }

            public string Hello()
            {
                return "Hello " + factoryParameter.HelloWho;
            }

            public static WithFactory Create(FactoryParameter factoryParameter)
            {
                return new WithFactory(factoryParameter);
            }

        }


        [Test]
        public void DemoStaticMethodInvoker()
        {
            Assert.That(new Fixture() { OmitAutoProperties = true }.Create<WithFactory>().Hello(), Is.EqualTo("Hello World"));
        }


        class BaseType
        {
            public string? BaseProperty { get; set; }
        }

        class DerivedType : BaseType
        {
            public string? DerivedProperty { get; set; }
        }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        class BuildOrCustomize
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            public string? StringProperty { get; set; }

            public int? IntProperty { get; set; }

            public string? ForDo { get; private set; }

            public void Do()
            {
                ForDo = "Done";
            }

            public override bool Equals(object? obj)
            {
                return obj is BuildOrCustomize customize &&
                       StringProperty == customize.StringProperty &&
                       IntProperty == customize.IntProperty &&
                       ForDo == customize.ForDo;
            }
        }

        [Test]
        public void DemoBuildVsCustomize()
        {
            var fixture = new Fixture();
            fixture.Inject(99);
            fixture.Inject("Injected String");
            var fromAutoPropeties = fixture.Create<BuildOrCustomize>();

            Assert.Multiple(() =>
            {
                Assert.That(fromAutoPropeties.StringProperty, Is.EqualTo("Injected String"));
                Assert.That(fromAutoPropeties.IntProperty, Is.EqualTo(99));
            });
            // demo Do extension method on ISpecimenBuilder
            fixture.Do<BuildOrCustomize>(buildOrCustomizeFromCreate => Assert.That(buildOrCustomizeFromCreate, Is.EqualTo(fromAutoPropeties)));

            // all composition has to be done in the callback.  Keeping the composer as a variable will not work as its methods produce new ISpecimenBuilder
            fixture.Customize<BuildOrCustomize>(composer =>
            {
                return composer.OmitAutoProperties().Do(buildOrCustomize => buildOrCustomize.Do());
            });
            AssertCustomizeOmitAutoProperties();

            var buildAutoProperties = fixture.Build<BuildOrCustomize>().Create();
            Assert.Multiple(() =>
            {
                Assert.That(buildAutoProperties.StringProperty, Is.EqualTo("Injected String")); // Build gets the Customizations from the fixture graph
                Assert.That(buildAutoProperties.IntProperty, Is.EqualTo(99));
            });
            // has not affected the fixture graph
            AssertCustomizeOmitAutoProperties();

            var buildOmitAutoProperties = fixture.Build<BuildOrCustomize>().OmitAutoProperties().Create();
            Assert.Multiple(() =>
            {
                Assert.That(buildOmitAutoProperties.StringProperty, Is.Null);
                Assert.That(buildOmitAutoProperties.IntProperty, Is.Null);
                Assert.That(buildOmitAutoProperties.ForDo, Is.Null);
            });

            var buildWithout = fixture.Build<BuildOrCustomize>().Without(buildOrCustomize => buildOrCustomize.IntProperty).Create();
            Assert.Multiple(() =>
            {
                Assert.That(buildWithout.StringProperty, Is.EqualTo("Injected String"));
                Assert.That(buildWithout.IntProperty, Is.Null);
            });

            var buildOmitAutoPropertiesWithResolvedProperty = fixture.Build<BuildOrCustomize>().OmitAutoProperties().With(buildOrCustomize => buildOrCustomize.StringProperty).Create();
            Assert.Multiple(() =>
            {
                Assert.That(buildOmitAutoPropertiesWithResolvedProperty.StringProperty, Is.EqualTo("Injected String"));
                Assert.That(buildOmitAutoPropertiesWithResolvedProperty.IntProperty, Is.Null);
            });

            var intFactoryInt = 0;
            int? intFactory() => intFactoryInt++;
            var buildFactorySpecimenBuilder = fixture.Build<BuildOrCustomize>().With(buildOrCustomize => buildOrCustomize.IntProperty, intFactory);
            var firstBuildPropertyFromFactory = buildFactorySpecimenBuilder.Create();
            var secondBuildPropertyFromFactory = buildFactorySpecimenBuilder.Create();
            Assert.Multiple(() =>
            {
                Assert.That(firstBuildPropertyFromFactory.IntProperty, Is.Zero);
                Assert.That(secondBuildPropertyFromFactory.IntProperty, Is.EqualTo(1));
            });


            void AssertCustomizeOmitAutoProperties()
            {
                var customizeOmitAutoProperties = fixture.Create<BuildOrCustomize>();
                Assert.Multiple(() =>
                {
                    Assert.That(customizeOmitAutoProperties.StringProperty, Is.Null);
                    Assert.That(customizeOmitAutoProperties.IntProperty, Is.Null);
                    Assert.That(customizeOmitAutoProperties.ForDo, Is.EqualTo("Done"));
                });
            }
        }
    }
}
