namespace Hallett.AutoFixture.NUnit4
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    /// <summary>
    /// Indicates that a parameter should be automatically supplied by the AutoFixture framework.
    /// This attribute is used with AutoCombinatorialAttribute, AutoPairwiseAttribute, AutoSequentialAttribute, and AutoTheoryAttribute 
    /// to mark the first parameter that should be automatically supplied. 
    /// It is also used to mark the last parameter, when of type CancellationToken, to indicate that AutoFixture should supply it and not NUnit.
    /// </summary>
    class AutoAttribute : Attribute { }
}
