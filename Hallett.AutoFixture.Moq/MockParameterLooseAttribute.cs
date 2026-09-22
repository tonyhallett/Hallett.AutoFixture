using Hallett.AutoFixture.Moq.EnhancedCustomization.Parameters;

namespace Hallett.AutoFixture.Moq
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class MockParameterLooseAttribute() : Attribute, IMockParameterBehavior
    {
        public bool Strict => false;
    }
}