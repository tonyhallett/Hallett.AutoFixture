using AutoFixture;
using AutoFixture.AutoMoq;
using Hallett.AutoFixture.Moq;
using Hallett.AutoFixture.Moq.Demos;
using Moq;

namespace Hallett.AutoFixture.Tests.AutoMoq
{
    internal class DemoAutoMoqCustomizationTests
    {
        public class SetupReturn { }
        public class SetupOut { };
        public class SetupGet { };
        public class GetSet
        {
            public string? Property { get; set; }
        }

        public interface IAlsoMoqqed { }


        public interface IMoqqed
        {
            SetupReturn MethodToMock(string str, out SetupOut setupOut);
            SetupGet Getter { get; }
            GetSet GetSet { get; set; }
            IAlsoMoqqed MethodReturningInterface();
        }

        private readonly string[] expectedAllABC = ["ABC", "ABC", "ABC"];

        [Test]
        public void Demo_AutoMoqCustomization_DefaultValue_Mock()
        {
            var fixture = new Fixture();
            var injected = new Mock<IAlsoMoqqed>().Object;
            fixture.Inject(injected);

            fixture.Customize(new AutoMoqCustomization());

            Assert.That(Mock.Get(fixture.Create<IMoqqed>().MethodReturningInterface()), Is.Not.SameAs(injected));
        }

        [Test]
        public void Demo_AutoMoqCustomization_ConfigureMembers_Delegates()
        {
            var fixture = new Fixture();

            var resolvedFuncReturn = false;
            fixture.Register(() =>
            {
                resolvedFuncReturn = true;
                return "return value";
            });

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true,
            });

            var mockedFunc = fixture.Create<Func<string>>();

            Assert.Multiple(() =>
            {
                Assert.That(resolvedFuncReturn, Is.False);
                Assert.That(mockedFunc(), Is.EqualTo("return value"));
            });
        }

        [Test]
        public void Demo_AutoMoqCustomization_ConfigureMembers()
        {
            // the customization ignores the fixture OmitAutoProperties
            var gs = AutoMoqCustomizationTest(new AutoMoqCustomization { ConfigureMembers = true, }, true)!;

            // The instance of the property is requested by AutoMoqCustomization
            // but that type will go through the normal resolution process and its properties will not be auto mocked.
            Assert.That(gs.Property, Is.Null);
        }

        [Test]
        public void Demo_AutoMoqWithOmitAutoPropertiesCustomization()
        {
            AutoMoqCustomizationTest(
                new EnhancedAutoMoqCustomization
                {
                    ConfigureMembers = true,
                    AutoPropertiesBehaviour = AutoPropertiesBehaviour.FollowFixture
                }, false);
        }

        private static GetSet? AutoMoqCustomizationTest(ICustomization customization, bool expectsAutoMocksProperties)
        {
            var resolvedMoqMethodReturn = false;
            var resolvedMoqGetter = false;
            var resolveMoqMethodOut = false;
            var fixture = new Fixture
            {
                OmitAutoProperties = true
            };
            fixture.Customize(customization);
            var setupReturn = new SetupReturn();
            fixture.Register(() =>
            {
                resolvedMoqMethodReturn = true;
                return setupReturn;
            });
            var setupOut = new SetupOut();
            fixture.Register(() =>
            {
                resolveMoqMethodOut = true;
                return setupOut;
            });
            var setupGet = new SetupGet();
            fixture.Register(() =>
            {
                resolvedMoqGetter = true;
                return setupGet;
            });
            var moqqed = fixture.Create<IMoqqed>();

            Assert.Multiple(() =>
            {
                Assert.That(resolvedMoqMethodReturn, Is.False);
                Assert.That(resolvedMoqGetter, Is.False);
                Assert.That(resolveMoqMethodOut, Is.True);
            });

            var resolved = moqqed.MethodToMock("any string", out var setupOutFromMoq);
            var resolvedGet = moqqed.Getter;
            Assert.Multiple(() =>
            {
                Assert.That(setupOutFromMoq, Is.SameAs(setupOut));
                Assert.That(resolved, Is.SameAs(setupReturn));
                Assert.That(resolvedGet, Is.SameAs(setupGet));
            });


            Assert.That(moqqed.GetSet, expectsAutoMocksProperties ? Is.Not.Null : Is.Null);
            return moqqed.GetSet;
        }

        public interface IItem
        {
            string Name { get; }

            int Other { get; }
        }

        public class WithItem(IItem item)
        {
            public IItem Item => item;
        }

        // https://stackoverflow.com/questions/58998834/how-to-use-ifixture-buildt-with-automoqcustomization-when-t-is-an-interface
        [Test]
        public void AdditionalSetUpWithExtensionMethod()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });

            var items = fixture.CreateManyConfigureMocks<IItem>().AllWithPropertyValue(item => item.Name, "ABC");

            Assert.That(items.Select(i => i.Name), Is.EqualTo(expectedAllABC));
        }


        [Test]
        public void AdditionalSetupWithInterceptionWrapper()
        {
            var fixture = new Fixture();
            // in reality you would use the extension method above, or use AutoMoqInterceptorCustomization
            var autoMoqInterceptorCustomization = new AutoMoqInterceptorWrapperCustomization(new AutoMoqCustomization { ConfigureMembers = true });
            autoMoqInterceptorCustomization.Intercept<IItem>(mock => mock.SetupGet(i => i.Name).Returns("ABC"));
            fixture.Customize(autoMoqInterceptorCustomization);

            var items = fixture.CreateMany<IItem>();

            Assert.That(items.Select(i => i.Name), Is.EqualTo(expectedAllABC));
        }

        [Test]
        public void AdditionalSetupWithInterception()
        {
            var fixture = new Fixture();
            var autoMoqInterceptorCustomization = new EnhancedAutoMoqCustomization { ConfigureMembers = true };
            autoMoqInterceptorCustomization.Intercept<IItem>(mock => mock.SetupGet(i => i.Name).Returns("ABC"));
            fixture.Customize(autoMoqInterceptorCustomization);

            var items = fixture.CreateMany<IItem>();

            Assert.That(items.Select(i => i.Name), Is.EqualTo(expectedAllABC));
        }

        [Test]
        public void AdditionalSetupWithFreezeMock()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
            fixture.FreezeMock<IItem>().Setup(item => item.Name).Returns("ABC");

            Assert.That(fixture.Create<WithItem>().Item.Name, Is.EqualTo("ABC"));
        }
    }
}
