using AutoFixture;
using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Behaviour
{
    internal class DemoCustomMethodQueryTest
    {
        public class DecoratingParameterMethodQuery(
            IMethodQuery methodQuery,
            Func<IEnumerable<object>, IEnumerable<object>> decorator) : IMethodQuery
        {
            public class DecoratingMethod(IMethod method, Func<IEnumerable<object>, IEnumerable<object>> decorator) : IMethod
            {
                public IEnumerable<System.Reflection.ParameterInfo> Parameters => method.Parameters;

                public object Invoke(IEnumerable<object> parameters) => method.Invoke(decorator(parameters));
            }

            public IEnumerable<IMethod> SelectMethods(Type type) => methodQuery.SelectMethods(type).Select(m => new DecoratingMethod(m, decorator));
        }

        class MethodInvokerParameter
        {
            public virtual string Invoke()
            {
                return "123";
            }
        }

        class ForMethodInvoker(MethodInvokerParameter parameter)
        {
            public string InvokeParameter()
            {
                return parameter.Invoke();
            }
        }

        class DecoratingMethodInvokerParameter(MethodInvokerParameter decorated) : MethodInvokerParameter
        {
            public override string Invoke()
            {
                return "prefix" + decorated.Invoke();
            }
        }

        [Test]
        public void DemoMethodInvoker()
        {
            var methodInvoker = new MethodInvoker(
                new DecoratingParameterMethodQuery(
                    new ModestConstructorQuery(),
                    parameters => parameters.Select(p => p is MethodInvokerParameter toDecorate ? new DecoratingMethodInvokerParameter(toDecorate) : p)
            ));

            var fixture = new Fixture();
            fixture.Customizations.Add(new FilteringSpecimenBuilder(methodInvoker, new ExactTypeSpecification(typeof(ForMethodInvoker))));

            var forMethodInvoker = fixture.Create<ForMethodInvoker>();
            Assert.That(forMethodInvoker.InvokeParameter(), Is.EqualTo("prefix123"));
        }
    }
}
