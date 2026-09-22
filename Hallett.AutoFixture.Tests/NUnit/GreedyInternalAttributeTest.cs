using Hallett.AutoFixture.NUnit4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hallett.AutoFixture.Tests.NUnit
{
    
    internal class GreedyInternalAttributeTest
    {
        public class TestClass
        {
            internal TestClass() => ThrowIfChosen();

            internal TestClass(int a) => ThrowIfChosen();

            internal TestClass(int a, int b)
            { }

            public TestClass(int a, int b, int c) => ThrowIfChosen();

            private static void ThrowIfChosen() => throw new InvalidOperationException("This constructor should not be chosen by GreedyInternalAttribute.");
        }

        [AutoTest]
        [Test]
#pragma warning disable IDE0060 // Remove unused parameter
        public void GreedyInternal_Should_Choose_The_Internal_Constructor_With_Most_Parameters([GreedyInternal] TestClass testClass)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            
        }
    }
}
