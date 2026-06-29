using NUnit.Framework.Interfaces;
using System.Collections;

namespace Hallett.AutoFixture.NUnit4
{
    public class AutoParameterDataProvider(IParameterDataProvider nunitParameterDataProvider, bool useHasDataFor = true) : IAutoParameterDataProvider
    {
        public IEnumerable GetDataFor(IParameterInfo parameter) => nunitParameterDataProvider.GetDataFor(parameter);

        public int NumberOfParameters(IMethodInfo method)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.IsAutoParameter() || (useHasDataFor && !nunitParameterDataProvider.HasDataFor(parameter)))
                {
                    return i;
                }
            }

            return parameters.Length;
        }
    }
}
