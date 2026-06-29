using NUnit.Framework.Interfaces;

namespace Hallett.AutoFixture.NUnit4
{
    internal static class AutoAttributeExtensions
    {
        public static bool IsAutoParameter(this IParameterInfo parameter)
            => parameter.GetCustomAttributes<AutoAttribute>(false).Length > 0;
    }
}
