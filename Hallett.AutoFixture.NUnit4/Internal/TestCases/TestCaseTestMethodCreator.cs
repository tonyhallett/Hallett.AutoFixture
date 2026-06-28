using AutoFixture;
using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Internal.CancelAfter;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCases
{
    internal class TestCaseTestMethodCreator : ITestCaseTestMethodCreator
    {
        internal ITestCaseArgumentFreezer TestCaseArgumentFreezer = new TestCaseArgumentFreezer();

        public TestMethod Create(
            Func<IFixture>? fixtureFactory,
            Type? InlineAutoDataAttributeType,
            IMethodInfo method,
            Test? suite,
            object?[] testCaseArguments)
        {
            if (InlineAutoDataAttributeType != null)
            {
                InlineAutoDataAttribute inlineAutoData = CreateInlineAutoDataAttributeApplyFrozen(InlineAutoDataAttributeType, testCaseArguments, method);
                var test = inlineAutoData.BuildFrom(method, suite).First();
                // alternative to this is setting the InlineAutoDataAttribute 's TestMethodBuilder to FixedNameTestMethodBuilderCancellationTokenAware
                test = test.ConsiderCancelAfterAttribute(suite);
                return test;
            }
            else
            {
                IFixture fixture = fixtureFactory!();
                TestCaseArgumentFreezer.ApplyFreezingAttributes(() => fixture, method, testCaseArguments);
                return new AutoTestCaseAttribute(() => fixture, testCaseArguments).BuildFrom(method, suite).First();
            }
        }

        public TestMethod Create<TFixtureFactory>(IMethodInfo method, Test? suite, object?[] testCaseArguments) where TFixtureFactory : IFixtureFactory, new()
        {
            var fixture = new TFixtureFactory().Create();
            TestCaseArgumentFreezer.ApplyFreezingAttributes(() => fixture, method, testCaseArguments);
            return new AutoTestCaseAttribute(() => fixture, testCaseArguments)
                .BuildFrom(method, suite).First();
        }

        private InlineAutoDataAttribute CreateInlineAutoDataAttributeApplyFrozen(Type inlineAutoDataAttributeType, object?[] arguments, IMethodInfo method)
        {
            var attribute = (InlineAutoDataAttribute)inlineAutoDataAttributeType.GetConstructors().First().Invoke([arguments]);
            TestCaseArgumentFreezer.ApplyFreezingAttributes(() => attribute.GetFixture(), method, arguments);
            return attribute;
        }
    }
}
