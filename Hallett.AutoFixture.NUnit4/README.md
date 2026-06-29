# Hallett.AutoFixture.NUnit4

# Custom frozen attributes

The NUnit FrozenAttribute is reliant upon test method parameter names when `Matching.PropertyName`, `Matching.FieldName`, `Matching.ParameterName`.

The FrozenNamedAttribute allows for `nameof`.

The FrozenParameterIndexAttribute cannot use nameof so instead you specify a Type that has a method that will be found by an IMethodQuery ( default ModestConstructorQuery) containing a parameter that is a base type of the attributed test method parameter type.

If there is more than one parameter then you can specify which one with the index constructor parameter. If you want the second then the index is 1, the parameter position is irrelevant.

To change from ModestConstructorQuery set the MethodQueryType property

# NUnit parameterized tests with CancelAfterAttribute

Of the [NUnit parameterized tests](https://docs.nunit.org/articles/nunit/technical-notes/usage/Parameterized-Tests.html) only TestCaseAttribute has an AutoFixture version,
`InlineAutoDataAttribute`.

AutoFixture.xUnit3 on the other hand has `MemberAutoDataAttribute` and `ClassAutoDataAttribute` that together work similar to TestCaseSourceAttribute.

These two are on the [Version 5 roadmap for NUnit](https://github.com/AutoFixture/AutoFixture/discussions/1327)

The existing AutoFixture attributes for NUnit, `AutoDataAttribute` and `InlineAutoDataAttribute`, do not support the [CancelAfterAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/cancelafter.html).

These can be replaced with `AutoTestAttribute` and `AutoTestCaseAttribute` which support the CancelAfterAttribute.

There is also a generic version of these, so that the `IFixture` does not have to come from `Func<IFixture>`.

e.g

```
    public class AutoTestAttribute<TFixtureFactory>()
        : AutoTestAttribute(new TFixtureFactory().Create) where TFixtureFactory : IFixtureFactory, new()
    {
    }
```

The remaining parameterized tests are supported with :

A new AutoFixture, CancelAfterAttribute aware, version of `TestCaseSourceAttribute` - `AutoTestCaseSourceAttribute`

For the following NUnit attributes ( IParameterDataSource )

The [RandomAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/random.html)

The [RangeAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/range.html)

The [ValuesAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/values.html)

The [ValueSourceAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/valuesource.html)

Their functionality comes from the abstract `CombiningStrategyAttribute` with derivations supplying

`protected CombiningStrategyAttribute(ICombiningStrategy strategy, IParameterDataProvider provider)`

The derivations `CombinatorialAttribute`, `PairwiseAttribute`, `SequentialAttribute` all provide
`ParameterDataSourceProvider` that work with the `IParameterDataSource` attributes above.

Each provide a corresponding stategy - CombinatorialStrategy, PairwiseStrategy and SequentialStrategy.

The final NUnit parameterization is the [TheoryAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/theory.html?q=Theory),
whose DatapointProvider looks for `[DatapointAttribute]` and `[DatapointSourceAttribute]` on fields, properties and non void methods.

```
public TheoryAttribute(bool searchInDeclaringTypes = false) : base(
    new CombinatorialStrategy(),
    new ParameterDataProvider(new DatapointProvider(searchInDeclaringTypes), new ParameterDataSourceProvider()))
{
}
```

There are "Auto" versions of CombiningStrategyAttribute and derivations.

If you have your own `IParameterDataProvider` derivation note that the AutoCombiningStrategyAttribute accepts `IAutoParameterDataProvider`.

```C#
    public abstract class AutoCombiningStrategyAttribute(
        ICombiningStrategy strategy, 
        IAutoParameterDataProvider provider, 
        Func<IFixture>? fixtureFactory = null)
        : Attribute, ITestBuilder, IApplyToTest
    {
```

The difference being `int NumberOfParameters(IMethodInfo method);` instead of `public bool HasDataFor(IParameterInfo parameter)`.

There is a provided implementation where the presence of `[Auto]` marks the cut off point where AutoFixture provides the remaining parameters.
It is only necessary to mark if useDataFor is false.


```
    public class AutoParameterDataProvider(IParameterDataProvider nunitParameterDataProvider, bool useHasDataFor = true) : IAutoParameterDataProvider
    {
        public IEnumerable GetDataFor(IParameterInfo parameter) => nunitParameterDataProvider.GetDataFor(parameter);

        public int NumberOfParameters(IMethodInfo method)
        {
            var parameters = method.GetParameters();
            for(var i=0;i<parameters.Length; i++)
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
```



**Note that with NUnit and IParameterDataSource attributes it is not necessary to apply the
CombinatorialAttribute to the test method as it is the default. This is not true with AutoCombinatorialAttribute.**

**For AutoTheoryAttribute to work you need to apply** `[Auto]` **on the first parameter to be supplied by AutoFixture.**
( as internally the NUnit DatapointProvider is used and HasDataFor only applied when used by an NUnit TheoryAttribute )
