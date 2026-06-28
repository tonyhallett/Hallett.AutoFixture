using AutoFixture.Kernel;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4
{
    /// <summary>
    /// Interface for attributes that can freeze a test case argument based on a request specification.
    /// </summary>
    public interface IFreezeTestCaseArgument
    {
        IRequestSpecification GetRequestSpecification(ParameterInfo parameter);
    }
}
