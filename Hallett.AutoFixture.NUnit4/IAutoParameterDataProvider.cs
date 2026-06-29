using NUnit.Framework.Interfaces;
using System.Collections;

namespace Hallett.AutoFixture.NUnit4
{
    public interface IAutoParameterDataProvider
    {
        /// <summary>
        /// Retrieves a list of arguments which can be passed to the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter of a parameterized test.</param>
        IEnumerable GetDataFor(IParameterInfo parameter);

        /// <summary>
        /// The number of parameters to GetDataFor
        /// </summary>
        /// <param name="method"></param>
        /// <returns>The number of parameters to GetDataFor</returns>
        int NumberOfParameters(IMethodInfo method);
    }
}
