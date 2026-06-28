using AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4;
using Hallett.AutoFixture.NUnit4.Moq;
using Hallett.AutoFixture.Tests.TestHelpers;
using Moq;

namespace Hallett.AutoFixture.Tests.Frozen
{
    internal class FrozenAttribute_And_Custom_Tests
    {
        public class ForFreezeDependency { }

        public class DerivedForFreezeDependency : ForFreezeDependency { }

        public record FreezeDependencies(
            ForFreezeDependency Parameter1, ForFreezeDependency Parameter2,
            ForFreezeDependency Property1, ForFreezeDependency Property2,
            ForFreezeDependency Field1, ForFreezeDependency Field2);

        public class ForFreeze(ForFreezeDependency parameterDependency1, ForFreezeDependency parameterDependency2)
        {
            public ForFreezeDependency? FieldDependency1;
            public ForFreezeDependency? FieldDependency2;

            public ForFreezeDependency? PropertyDependency1 { get; set; }

            public ForFreezeDependency? PropertyDependency2 { get; set; }

            public FreezeDependencies GetDependencies()
            {
                return new FreezeDependencies(parameterDependency1, parameterDependency2, PropertyDependency1!, PropertyDependency2!, FieldDependency1!, FieldDependency2!);
            }
        }

        [Test, AutoData]
        public void DemoFreezeAttribute(
           [FrozenNamed(nameof(ForFreeze.PropertyDependency1))] DerivedForFreezeDependency firstPropertyDependency,
           [Frozen(Matching.PropertyName)] DerivedForFreezeDependency PropertyDependency2,

           [FrozenParameterIndex(typeof(ForFreeze))] DerivedForFreezeDependency p1Dependency,
           [Frozen(Matching.ParameterName)] DerivedForFreezeDependency parameterDependency2,

           [FrozenNamed(nameof(ForFreeze.FieldDependency1), false)] DerivedForFreezeDependency firstFieldDependency,
           [Frozen(Matching.FieldName)] DerivedForFreezeDependency fieldDependency2,

           ForFreeze forFreeze
           )
        {
            var (param1, param2, prop1, prop2, field1, field2) = forFreeze.GetDependencies();
            var allDependencies = new[] { param1, param2, prop1, prop2, field1, field2 };
            Assert.Multiple(() =>
            {
                Assert.That(param1, Is.SameAs(p1Dependency));
                Assert.That(param2, Is.SameAs(parameterDependency2));
                Assert.That(prop1, Is.SameAs(firstPropertyDependency));
                Assert.That(prop2, Is.SameAs(PropertyDependency2));
                Assert.That(field1, Is.SameAs(firstFieldDependency));
                Assert.That(field2, Is.SameAs(fieldDependency2));

                Assert.That(allDependencies, Is.Unique);
            });
        }

        public interface IInterface { }
        public interface IDerivedInterface : IInterface { }
        public class ImplementingInterface : IDerivedInterface { }

        public class BaseType { }
        public class DerivedType : BaseType { }
        public class FurtherDerivedType : DerivedType { }

        public class ExactType { }
        public class DerivedExactType : ExactType { }

        public class TypeDemo(
            BaseType baseType,
            DerivedType derivedType,
            FurtherDerivedType furtherDerivedType,
            IInterface @interface,
            ImplementingInterface implementingInterface,
            ExactType exactType,
            DerivedExactType derivedExactType
            )
        {
            public BaseType BaseType => baseType;
            public DerivedType DerivedType => derivedType;
            public FurtherDerivedType FurtherDerivedType => furtherDerivedType;
            public IInterface Interface => @interface;
            public ImplementingInterface ImplementingInterface => implementingInterface;

            public ExactType ExactType { get; } = exactType;
            public DerivedExactType DerivedExactType { get; } = derivedExactType;
        }

        [Test, AutoData]
        public void DemoFrozenAttributeMatchingTypeEnumMembers(
            [Frozen(Matching.DirectBaseType)] DerivedType derivedType,
            [Frozen(Matching.ImplementedInterfaces)] ImplementingInterface implementingInterface,
            [Frozen(Matching.ExactType)] ExactType exactType,
            TypeDemo typeDemo
            )
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeDemo.BaseType, Is.SameAs(derivedType));
                Assert.That(typeDemo.DerivedType, Is.Not.SameAs(derivedType));
                Assert.That(typeDemo.FurtherDerivedType, Is.Not.SameAs(derivedType));
                Assert.That(typeDemo.Interface, Is.SameAs(implementingInterface));
                Assert.That(typeDemo.ImplementingInterface, Is.Not.SameAs(implementingInterface));
                Assert.That(typeDemo.ExactType, Is.SameAs(exactType));
                Assert.That(typeDemo.DerivedExactType, Is.Not.SameAs(exactType));
            });
        }


        public interface IDependency { }
        public class SUT(IDependency dependency1, IDependency dependency2)
        {
            public IDependency Dependency1 => dependency1;
            public IDependency Dependency2 => dependency2;
        }

        // https://github.com/AutoFixture/AutoFixture/issues/1134 
        [Test, MoqTest]
        public void IncorrectMethod(
           [Frozen(Matching.ParameterName)] Mock<IDependency> dependency1,
           SUT sut
        )
        {
            Assert.That(sut.Dependency1, Is.Not.SameAs(dependency1.Object));
        }


        [Test, MoqTest]
        public void CorrectMethod(
           [Frozen(Matching.ParameterName)] IDependency dependency1,
           SUT sut
        )
        {
            Assert.That(sut.Dependency1, Is.SameAs(dependency1));
            Mock.Get(dependency1);
        }

        [Test, MoqTest]
        public void WithCustomAttribute(
            [FrozenMock] Mock<IDependency> mockDependency1,
            [FrozenMock] Mock<IDependency> mockDependency2,
            SUT sut
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(mockDependency1.Object, Is.SameAs(sut.Dependency1));
                Assert.That(mockDependency2.Object, Is.SameAs(sut.Dependency2));
                Assert.That(sut.Dependency1, Is.Not.SameAs(sut.Dependency2));
            });
        }
    }
}
