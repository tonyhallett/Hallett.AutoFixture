using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit4;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4
{
    public class GreedyInternalAttribute : CustomizeAttribute
    {
        private class GreedyInternalConstructorQuery : IMethodQuery
        {
            public IEnumerable<IMethod> SelectMethods(Type type)
            {
                return from ci in type.GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                       let parameters = ci.GetParameters()
                       where parameters.All((ParameterInfo p) => p.ParameterType != type)
                       orderby parameters.Length descending
                       select (IMethod)new ConstructorMethod(ci);
            }
        }

        public override ICustomization GetCustomization(ParameterInfo parameter)
            => new ConstructorCustomization(parameter.ParameterType, new GreedyInternalConstructorQuery());
    }
}
