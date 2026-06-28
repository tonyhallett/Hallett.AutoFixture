# AutoFixture

AutoFixture via its Fixture class, is a configurable container for object creation.

It serves two purposes :

as a Test Data Builder

and as a utility for creating a SUT without having to manually construct dependencies that do not affect the
execution path being tested. It has extensions, ICustomization implementations, for popular mocking frameworks.

By default it automatically sets properties on the objects it creates. When creating a SUT you should probably turn this off !

```C#
new Fixture().Create<SUT>();
```

The Fixture is capable of constructing concrete dependencies ( and collection interfaces ) but will throw ObjectCreationException for value types with no explicit parameterized constructors.

It can be configured to provide instances however you desire.

AutoFixture also provides [attributes](#attributes-for-testing-frameworks) for testing frameworks so that your test methods are provided arguments from a fixture.

```C#
[Test, AutoData]
public void SUTTest(SUT sut)
{

}
```

# How it works - the Fixture graph

The Fixture class has a **graph** of ISpecimenBuilderNode

```C#
public interface ISpecimenBuilderNode : ISpecimenBuilder, IEnumerable<ISpecimenBuilder>
{
    ISpecimenBuilderNode Compose(IEnumerable<ISpecimenBuilder> builders);
}

public interface ISpecimenBuilder
{
    object Create(object request, ISpecimenContext context);
}

public interface ISpecimenContext
{
    object Resolve(object request);
}
```

<details>
<summary>ISpecimenBuilderNode Compose</summary>

The only reason a user of AutoFixture would need to be concerned with ISpecimenBuilderNode and its Compose method is when creating a [behavior](#behaviors) as these return an ISpecimenBuilderNode.

The Compose method should retain its original behaviour and should return the implementation type.  
Compose is only used internally when the Fixture `Customizations` and `ResidueCollectors` are changed or when working with the `ICustomizationComposer<T>`

</details>

An ISpecimenBuilder will be passed a [request object](#requests) which it can look at to see if it is a request that
it can handle ( resolve ). If it cannot then it returns an instance of `NoSpecimen` ( NoSpecimen.Instance in version 5 ).

The ISpecimenContext allows routing additional requests through the graph, and there are ISpecimenBuilder implementations with names ending in Relay
that do this.

The first request originates from `fixture.Create<T>()` ( or `fixture.Freeze<T>()`).

<details>
<summary>Extension methods</summary>
AutoFixture uses extension methods for helper methods.

`Create` is an extension method of `ISpecimenBuilder`, that `IFixture` extends

```C#
public static T Create<T>(this ISpecimenBuilder builder)
{
    return builder.CreateContext().Create<T>();
}


private static ISpecimenContext CreateContext(this ISpecimenBuilder builder)
{
    return new SpecimenContext(builder);
}

public static T Create<T>(this ISpecimenContext context)
{
    return (T)context.Resolve(new SeededRequest(typeof(T), default(T)));
}
```

`SpecimenContext`

```C#
    public object Resolve(object request)
    {
        return Builder.Create(request, this);
    }
```

Freeze is an extension method of IFixture, as is Inject.

</details>

I have a couple of ISpecimenBuilder demos - [e.g](#ispecimenbuilder-reading-normal-parameter-attributes)

# Requests

# IRequestSpecification

This simple interface is used by multiple AutoFixture types as a condition to proceed with a request.
For instance, it is used by FilteringSpecimenBuilder and Postprocessor to determine if the ISpecimenBuilder or ISpecimenCommand should be invoked.
It has a single method `bool IsSatisfiedBy(object request);`.

There are simple implementations such as TrueRequestSpecification and FalseRequestSpecification,
ones that provide logic for others OrRequestSpecification, AndRequestSpecification, InverseRequestSpecification,
and specifc ones such as SeedRequestSpecification, ParameterSpecification, PropertySpecification and FieldSpecification.
These can be used when creating your own ISpecimenBuilder and many will be mentioned in the following sections.

## Singular requests

The request objects are typically of Type Type, ParameterInfo and there are PropertyInfo or FieldInfo requests when the fixture is in
[auto properties mode](#auto-properties-and-property-picker-configuration).
There are ISpecimenBuilder implementations for handling each of these.  
These requests may originate from a SeededRequest or a SeededRequest may be created from them.

### SeededRequest

SeededRequest requests are requests with additional context, from the seed object constructor argument, available from the Seed property.

**If a SeededRequest is not handled then there is the SeedIgnoringRelay, present in the graph, that will resolve the request as a normal request, thus ignoring the seed.**

There is also a `SeedRequestSpecification` that is satisfied by a request of type SeededRequest where the Request is a Type that equals the TargetType constructor argument.

SeededRequest requests are created by :

The relays that handle the normal requests for ParameterInfo, PropertyInfo and FieldInfo - relay a SeedRequest for a Type and the seed is the name member.

<details>
<summary>There are two other relays, not present in the graph</summary>

There are two other relays, not present in the graph, OmitArrayParameterRequestRelay and OmitEnumerableParameterRequestRelay that are both concerned with
ParameterInfo request where the parameter type is an array or `IEnumerable<T>` respectively. These send the usual SeededRequest for parameters but allow for OmitSpecimen return values.
When this occurs the seed will be empty.

</details>

`fixture.Create<T>()` will also result in `(T)context.Resolve(new SeededRequest(typeof(T), default(T)));`

<details>
<summary>for a fixture context.Resolve will go the graph Create.</summary>

```C#
public object Create(object request, ISpecimenContext context)
{
    if (request == null) throw new ArgumentNullException(nameof(request));
    if (context == null) throw new ArgumentNullException(nameof(context));

    return _graph.Create(request, context);
}
```

</details>

To resolve from an initial SeededRequest there is `Create<T>(T seed)` / `CreateMany<T>(T seed)` extension methods on ISpecimenBuilder and ISpecimenContext
and there is a `Freeze<T>(T seed)` extension method on IFixture that will `Create<T>(seed)` and then `Inject<T>(created)` ([See for inject](#adding-specimens-to-the-fixture).).

The graph only has one ISpecimenBuilder that handles SeededRequests and that is the StringSeedRelay that concatenates the seed string with the resolved string.

To handle a SeededRequest you can [create your own ISpecimentBuilder](#seededrequest-ispecimenbuilder-demo) or [See](#adding-specimens-to-the-fixture).

## Multiple requests

There is also MultipleRequest, these originate from relays that handle type requests for collection objects, ArrayRelay, EnumerableRelay, AsyncEnumerableRelay.

For dictionary types they are filled by MultipleRequest ( from a Postprocessor command ).

There is a `CreateMany<T>` extension method that sends a MultipleRequest through the graph and an overload with a count that sends a FiniteSequenceRequest.

MultipleRequest is handled by the MultipleRelay in the graph. The MultipleRelay has a Count property ( default 3 ), that can be set from the Fixture RepeatCount property. The MultipleRelay relays a FiniteSequenceRequest.

The FiniteSequenceRequest is handled by FiniteSequenceRelay, which relays each request from FiniteSequenceRequest.CreateRequests.
The FiniteSequenceRelay therefore returns an `IEnumerable<object>` from each except when the resolved value is OmitSpecimen.

( The Fixture.RepeatCount is also used for `public static IEnumerable<T> Repeat<T>(this IFixture fixture, Func<T> function)`)

# The ISpecimenBuilderNode / ISpecimenBuilder types in the graph

## The engine

The engine provides the default resolution, with the relays providing essential behaviour and the primitive builders providing primitive values.

<details>

<summary>The engine</summary>

( If you do not like the DefaultEngineParts you can create your own derivation and filter out by overriding GetEnumerator )

```C#
public Fixture()
: this(new DefaultEngineParts())
{
}

public class DefaultEngineParts : DefaultRelays
    public DefaultEngineParts()
    : this(new DefaultPrimitiveBuilders())
    {
    }

//-- DefaultRelays
yield return new LazyRelay();
yield return new MultidimensionalArrayRelay();
yield return new ArrayRelay();
yield return new ParameterRequestRelay();
yield return new PropertyRequestRelay();
yield return new FieldRequestRelay();
yield return new RangedSequenceRelay();
yield return new FiniteSequenceRelay();
yield return new SeedIgnoringRelay();
yield return new MethodInvoker(
    new CompositeMethodQuery(
        new ModestConstructorQuery(),
        new FactoryMethodQuery()));

//--- DefaultPrimitiveBuilders
yield return new StringGenerator(() => Guid.NewGuid());
yield return new ConstrainedStringGenerator();
yield return new StringSeedRelay();
yield return new RandomNumericSequenceGenerator();
yield return new RandomCharSequenceGenerator();
yield return new UriGenerator();
yield return new UriSchemeGenerator();
yield return new RandomRangedNumberGenerator();
yield return new RegularExpressionGenerator();
yield return new RandomDateTimeSequenceGenerator();

#if NET6_0_OR_GREATER
yield return new RandomDateOnlySequenceGenerator();
yield return new RandomTimeOnlySequenceGenerator();
#endif
yield return new BooleanSwitch();
yield return new GuidGenerator();
yield return new TypeGenerator();
yield return new DelegateGenerator();
yield return new TaskGenerator();
yield return new IntPtrGuard();
#if SYSTEM_NET_MAIL
yield return new MailAddressGenerator();
#endif
yield return new EmailAddressLocalPartGenerator();
yield return new DomainNameGenerator();
yield return new TimeZoneInfoGenerator();

```

```C#
public Fixture(DefaultRelays engineParts)
    : this(
        engineParts != null
            ? new CompositeSpecimenBuilder(engineParts)
            : throw new ArgumentNullException(nameof(engineParts)),
        new MultipleRelay())
{
}

public Fixture(ISpecimenBuilder engine, MultipleRelay multiple)
{
    Engine = engine ?? throw new ArgumentNullException(nameof(engine));

    ....

}
```

</details>

The essential parts of the graph

```C#
ISpecimenBuilderNode newGraph =
    new BehaviorRoot(
        new TerminatingWithPathSpecimenBuilder(new CompositeSpecimenBuilder(
            new CustomizationNode(
                new CompositeSpecimenBuilder(
                    // AutoFixture provides some - for providing the fixture itself, collections, data annotations
                    ),
            new AutoPropertiesTarget(
                new Postprocessor(
                    new CompositeSpecimenBuilder(
                        engine,
                        multiple),
                    new AutoPropertiesCommand(),
                    new AnyTypeSpecification())),
            new ResidueCollectorNode(
                new CompositeSpecimenBuilder(
                    // AutoFixture provides some - for collection interfaces and enumerables
                    ),
            new FilteringSpecimenBuilder(
                new MutableValueTypeWarningThrower(),
                new AndRequestSpecification(
                    new ValueTypeSpecification(),
                    new NoConstructorsSpecification())))));

```

The relays and primitive builders are wrapped in a CompositeSpecimenBuilder, the Engine, and this in turn is wrapped in another CompositeSpecimenBuilder ( with MultipleRelay ).
A CompositeSpecimenBuilder just returns from the first ISpecimenBuilder that does not return NoSpecimen.

Although the engine is in AutoPropertiesTarget the engine is not just for auto properties. What matters is its position between the CustomizationNode and ResidueCollectorNode.
Given that the three nodes are in a CompositeSpecimenBuilder, the ISpecimenBuilder instances in the CompositeSpecimenBuilder of the CustomizationNode get the first opportunity to resolve, then the engine will for concrete types and collection interfaces, then finally the ISpecimenBuilder instances in the CompositeSpecimenBuilder of the ResidueCollectorNode.

## Engine resolution

The default resolution of Type requests is done by the `public class MethodInvoker : ISpecimenBuilder`.

It uses an `IMethodQuery` ctor argument that will `public IEnumerable<IMethod> SelectMethods(Type type)` for the Type. The first IMethod where all parameters can be resolved
( not NoSpecimen or OmitSpecimen) will be used

```C#
public interface IMethod
{
    IEnumerable<ParameterInfo> Parameters { get; }

    object Invoke(IEnumerable<object> parameters);
}
```

The IMethodQuery of the MethodInvoker in the graph is a `CompositeMethodQuery`.

It combines the `IEnumerable<IMethod>` returned from all the `IMethodQuery` constructor parameters. The graph has two :

a) `ModestConstructorQuery` which returns `public class ConstructorMethod : IMethodInfo` that uses reflection, ConstructorInfo, to create the specimen.

The ConstructorMethod instances returned are ordered by the number of constructor parameters with the least first. There are other "Constructor" query types.

b) `FactoryMethodQuery` which returns `public class StaticMethod : IMethod` instances for each static method of T that returns T ( as long as the method has no parameters of T).

The StaticMethod Invoke again uses reflection to create the specimen.

There are other "ConstructorQuery" that are mentioned in the [attributes section](#iparametercustomizationsource-attributes).

There is a [Demo of IMethodQuery](#demo-of-imethodquery)

The parameters of course are resolved from the ISpecimenContext, with requests of type ParameterInfo of the IMethod.

After the instance has been constructed, the Postprocessor may Execute the AutoPropertiesCommand - [see](#auto-properties-and-property-picker-configuration)

The AutoPropertiesTarget is just a marker node for AutoFixture to internally find what is contained within.
The BehaviorRoot, CustomizationNode and ResidueCollectorNode are also marker nodes for the **three extension points**, the List properties, `Behaviors`, `Customizations` and `ResidueCollectors` respectively.

The ISpecimenBuilder instances in the graph not in CustomizationNode or ResidueCollectorNode are not expected to be manipulated by the user
as such the graph itself is not public but it is possible. For a new graph to be used by the Fixture this has to be done with a behaviour.
If you do not like the graph provided by Fixture then you can derive from Fixture and provide your own engine or derive from IFixture and implement your own graph.

<details>
<summary>Graph to lists</summary>

The Fixture constructor will call

```C#
private void UpdateGraphAndSetupAdapters(
    ISpecimenBuilderNode newGraph, IEnumerable<ISpecimenBuilderTransformation> existingBehaviors)
{
    _graph = newGraph;

    UpdateCustomizer();
    UpdateResidueCollector();
    UpdateBehaviors(existingBehaviors.ToArray());
}

private void UpdateCustomizer()
{
    _customizer =
        new SpecimenBuilderNodeAdapterCollection(
            _graph,
            n => n is CustomizationNode);
    _customizer.GraphChanged += (_, args) => UpdateGraphAndSetupAdapters(args.Graph);
}

private void UpdateResidueCollector()
{
    _residueCollector =
        new SpecimenBuilderNodeAdapterCollection(
            _graph,
            n => n is ResidueCollectorNode);
    _residueCollector.GraphChanged += (_, args) => UpdateGraphAndSetupAdapters(args.Graph);
}

private void UpdateBehaviors(ISpecimenBuilderTransformation[] existingTransformations)
{
    _behaviors =
        new SingletonSpecimenBuilderNodeStackAdapterCollection(
            _graph,
            n => n is BehaviorRoot,
            existingTransformations);
    _behaviors.GraphChanged += (_, args) => UpdateGraphAndSetupAdapters(args.Graph);
}


//SingletonSpecimenBuilderNodeStackAdapterCollection
private void UpdateGraph()
{
    ISpecimenBuilderNode g = Graph.FindFirstNode(_isWrappedGraph);
    ISpecimenBuilderNode builder = this.Aggregate(g, (b, t) => t.Transform(b));

    Graph = builder;

    OnGraphChanged(new SpecimenBuilderNodeEventArgs(Graph));
}
```

The `SpecimenBuilderNodeAdapterCollection` allows treating the specific ISpecimenBuilderNode as a list.

</details>

# Extension points

## Behaviors

Behaviors do not create specimens. As such they are probably of less interest as an extension point.

In most scenarios, the transformation is expected to maintain the behavior of the builder usually by applying a decorator.
They decorate the BehaviorRoot or decorate the decorators from behaviors earlier in the List.

```C#
public interface ISpecimenBuilderTransformation
{
    ISpecimenBuilderNode Transform(ISpecimenBuilder builder);
}
```

<details>
<summary>Additional details</summary>
The Fixture class adds one behaviour `Behaviors.Add(new ThrowingRecursionBehavior());` and AutoFixture provides some others.

For instance, TracingBehavior.

The transformed node, TracingWriter, Create invokes a TracingBuilder that decorates the original, raising events SpecimenRequested and SpecimenCreated that the TraceWriter writes to a TextWriter.
Although the TracingBehavior does not facilitate it, the TracingBuilder can have an IRequestSpecification that allows you to specify which requests to trace.

```C#
class TracingFilteringBehaviour : ISpecimenBuilderTransformation
{
    private readonly TextWriter writer;
    private readonly IRequestSpecification requestSpecification;

    public TracingFilteringBehaviour(TextWriter writer, IRequestSpecification requestSpecification)
    {
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
        this.requestSpecification = requestSpecification;
    }

    public ISpecimenBuilderNode Transform(ISpecimenBuilder builder)
    {
        var tracingBuilder = new TracingBuilder(builder) { Filter = requestSpecification };
        return new TraceWriter(writer, tracingBuilder);
    }
}
```

</details>

## Customizations and ResidueCollectors

Both lists are `IList<ISpecimenBuilder>`. Customizations execute before the engine and ResidueCollectors after the engine.

ResidueCollectors are the extension point for mocking libraries [Moq](#moq-icustomization).

## Customizations

_It is via the Customizations property that we are able to override the default MethodInvoker behaviour and supply our own instances for the fixture to resolve._

ISpecimenBuilder instances for requests for specific types are created and added to the Customizations list ( and to the graph) when using the methods on the Fixture class

`Inject`, `Register`,`Freeze`

`Customize` - **The above invoke this method** - [See](#adding-specimens-to-the-fixture)

## ICustomization

An ICustomization is an extension.

```C#
public interface ICustomization
{
    void Customize(IFixture fixture);
}
```

It can apply behaviors or handle unsuccessful resolution with ResidueCollectors but most likely will add to Customizations.

The mocking ICustomization implementations provided by AutoFixture also add to ResidueCollectors to handle interface and abstract type requests.

You can just pass the fixture to the ICustomization or use the

<details>
<summary>Fixture Customize method</summary>

```C#
public IFixture Customize(ICustomization customization)
{
    if (customization == null) throw new ArgumentNullException(nameof(customization));

    customization.Customize(this);
    return this;
}
```

</details>

AutoFixture provides a CompositeCustomization that will call Customize for all provided to the constructor.

There is also an extension method, `ToCustomization`, on `ISpecimenBuilder` that creates an `ICustomization` that will insert the specimen builder as the first customization.

It is also possible to provide an ICustomization and have them applied using [attributes](#attributes-for-testing-frameworks)

# Adding specimens to the fixture

The method below will create an ISpecimenBuilder for T, that can be customized ( exposed as `ICustomizationComposer<T>` ) before
being added as the first customization.

`void Customize<T>(Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation);`

<details>
<summary>What is configured</summary>

The `NodeComposer<T>` configures:

```C#
new FilteringSpecimenBuilder(
    new CompositeSpecimenBuilder(
        new NoSpecimenOutputGuard(
            new MethodInvoker(
                new ModestConstructorQuery()),
            new InverseRequestSpecification(
                new SeedRequestSpecification(
                    targetType))),
        new SeedIgnoringRelay()),
    new OrRequestSpecification(
        new SeedRequestSpecification(targetType),
        new ExactTypeSpecification(targetType)));
```

A FilteringSpecimenBuilder only asks the ISpecimenBuilder that it decorates if the IRequestSpecification.IsSatisfiedBy(request) is true.
Thus for the above, the CompositeSpecimenBuilder will only be asked when a request satisfies either of the SeedRequestSpecification or the ExactTypeSpecification.

The SeedIgnoringRelay resolves a SeededRequest into a regular request.

The NoSpecimenOutputGuard will throw ObjectCreationExpression if its builder returns NoSpecimen if the request satisfies.
For above, will throw unless it is a SeededRequest.

</details>

Instead of using Customize directly, the FixtureRegistrar class provides helper extension methods on IFixture that configure appropriately ( using `ICustomizationComposer<T>` `FromFactory` methods).

The `Register` method takes a `Func` that returns T and this Func can take 0-4 inputs that will be resolved from the fixture.

The `Inject` method invokes `Register` with a Func that always returns the same instance.

**The Register overloads will replace the MethodInvoker with an ISpecimenBuilder that invokes the Func.**

There is another method for registration and that is the `Freeze` extension method ( from FixtureFreezer )
This will `fixture.Create<T>()` and then `fixture.Inject<T>(created);`.

Note that the FixtureRegistrar does not provide a helper for the `FromSeed<T,T>` method.

This will create an ISpecimenBuilder that handles normal type requests for T as well as SeededRequest for T as long as the seed is also of type T,
invoking the Func with default(T) and the seed respectively. This aligns with `Create<T>(T seed)` and `Freeze<T>(T seed)`.

# `ICustomizationComposer<T>`

An `ICustomizationComposer<T>` can customize _for all requests_ on a fixture using `void Customize<T>(Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation);`

It is also possible to use `public ICustomizationComposer<T> Build<T>()` so as to not change the graph.

**Build does not insert**. After configuring you `Create()` from the `ICustomizationComposer<T>`. It has no effect on the Fixture graph, so does not affect how `fixture.Create<U>()` behaviour.

`Freeze` invokes `Build`, passing the `ICustomizationComposer<T>` to the `Func` then `Inject` the return from `Create`.

`public static T Freeze<T>(this IFixture fixture, Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation)`

[Demo of Build vs Customize](#build-vs-customize-demo)

```C#
public interface ICustomizationComposer<T> : IFactoryComposer<T>, IPostprocessComposer<T>
```

The `IFactoryComposer<T>` was discussed in the previous section, the `IPostprocessComposer<T>` allows for further configuration of the FilteringSpecimenBuilder
specific to T. This is mainly related to [auto properties](#auto-properties-and-property-picker-configuration) but there is also `Do(Action<T> action);` which will add a Postprocessor with an ISpecimenCommand that will execute the action on the created specimen of type T.

Note that `Do` _is not executed last._ See below for execution order.

<details>
    <summary>Github issue</summary>

(Issue - Do Behaviour)[https://github.com/AutoFixture/AutoFixture/discussions/1092]

> The order of callback invocations
> `Do()` callbacks
> `With` callbacks
> `AutoProperties` callbacks

(Issue - Callback after an object is created)[https://github.com/AutoFixture/AutoFixture/issues/988]

</details>

As such, the extension method below, available in Hallett.AutoFixture, facilitates a "Do" callback that does execute last.

<details>
<summary>Extension method</summary>

This has the same behaviour as the regular Customize method but wraps inside a Postprocessor that will invoke the callback after the
configured Do, With and AutoProperties.

```C#
public static class FixtureExtensions
{
    public static Fixture Customize<T>(
        this Fixture fixture,
        Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation,
        Action<T> doAfterAutoProperties)
    {
        var composed = composerTransformation(SpecimenBuilderNodeFactory.CreateComposer<T>().WithAutoProperties(!fixture.OmitAutoProperties));
        var customization = new Postprocessor(composed, new ActionSpecimenCommand<T>(doAfterAutoProperties));
        fixture.Customizations.Insert(0, customization);
        return fixture;
    }
}
```

</details>

Mocking frameworks use a Postprocessor to configure the created mock.

AutoFixture provides CompositeSpecimenCommand for multiple configuration behaviours.

# Auto Properties and property picker configuration

Auto properties is facilitated by a Postprocessor with ISpecimenCommand an AutoPropertiesCommand ( with a TrueRequestSpecification ).

The PostProcessor has an IRequestSpecification that when IsSatisfiedBy(request) will execute the ISpecimenCommand.
The Fixture class by default uses AnyTypeSpecification which is satisfied by a request of type Type.
This can be changed by Fixture.OmitAutoProperties = true ( FalseRequestSpecification).

It can also be changed for a specific type T using the `Customize<T>`, `Build<T>` or `Freeze<T>` as already mentioned.

The `IPostprocessComposer<T>` has methods related to auto properties:

`IPostprocessComposer<T> OmitAutoProperties();`

`IPostprocessComposer<T> WithAutoProperties();`

It also has overloaded `With` methods for selecting specific properties.

These allow _specifying the property with an expression_, and optionally _providing the value_ ( if you do not then the value is resolve(IPropertyInfo) - thus turning on auto properties for that property if turned off as a whole for T )

_via value, ISpecimenBuilder or Func._

There is also `Without` for excluding a property from auto properties.

The With / Without methods will set the AutoPropertiesCommand IRequestSpecification to be satisfied by or not satisfied by each of the PropertyInfo / FieldInfo requests
that will subsequently be resolved and used to set the field or property if satisfied by the request.

The other way that auto properties can be turned off is with the testing framework attributes.

NUnit has `NoAutoPropertiesAttribute` see next section.

# Attributes for testing frameworks

There are attributes for XUnit and NUnit for configuring the fixture and receiving test method arguments from the fixture.

This is what is provided for NUnit.

There is the `AutoDataAttribute` and the `InlineAutoDataAttribute`, both are similar.
`InlineAutoDataAttribute` allows providing arguments for test parameters at indices 0-n as attribute arguments, with the remainder coming from the fixture.
For Visual Studio to work with these your test methods must also have `[Test]` applied.

**Note that neither of these attributes supports** the NUnit [CancelAfterAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/cancelafter.html).

`InlineAutoDataAttribute` is the AutoFixture version of the NUnit `TestCaseAttribute`.

There are no AutoFixture versions of the other [parameterized test attributes](https://docs.nunit.org/articles/nunit/technical-notes/usage/Parameterized-Tests.html).

Due to these shortcomings, [see Hallett.AutoFixture.NUnit4](README.md#nunit-parameterized-tests-with-cancelafterattribute).
[For finer details](#hallettautofixturenunit4)

AutoDataAttribute and InlineAutoDataAttribute require an IFixture via a `Func<IFixture>` factory parameter, the default constructor creates a new Fixture, but you can provide your own factory to create a customized fixture.

The parameters for the method are resolved from the fixture with ParameterInfo requests, but prior to this

## Parameter attributes implementing IParameterCustomizationSource

get to provide ICustomization for that ParameterInfo, that is then applied with Fixture `Customize(ICustomization customization)`

```C#
public interface IParameterCustomizationSource
{
    ICustomization GetCustomization(ParameterInfo parameter);
}
```

## IParameterCustomizationSource attributes

AutoFixture provides a base attribute - `CustomizeAttribute` with abstract GetCustomization.

`NoAutoPropertiesAttribute`

This will just insert at position 0 an ICustomization for the parameter type that is what you would get by default from the graph.

There are also implementations for specifying how to construct the specimen for the parameter type.

`FavorArraysAttribute` => `new ConstructorCustomization(parameter.ParameterType, new ArrayFavoringConstructorQuery());`

`FavorEnumerablesAttribute` => `new ConstructorCustomization(parameter.ParameterType, new EnumerableFavoringConstructorQuery());`

`FavorListsAttribute` => `new ConstructorCustomization(parameter.ParameterType, new ListFavoringConstructorQuery());`

`GreedyAttribute` => `new ConstructorCustomization(parameter.ParameterType, new GreedyConstructorQuery());`

`ModestAttribute` => `new ConstructorCustomization(parameter.ParameterType, new ModestConstructorQuery());`

When the ConstructorCustomization is appied, it adds the same ISpecimenBuilder as Register but with a specific IMethodQuery.

_You could use the ConstructorCustomization directly outside of AutoDataAttribute usage._

---

`FrozenAttribute`

This creates a `FreezeOnMatchCustomization` that inserts in first position into Customizations a FilteringSpecimenBuilder for a `FixedBuilder` _that always returns the specimen that is *instantly resolved for the ParameterInfo*._

The filtering is determined by the FrozenAttribute enum argument of type `Matching` which defaults to `ExactType`.

Note that filtering is OrRequestSpecification that includes an EqualRequestSpecification that ensures that method parameter is what is frozen.

The `Matching` _flags_ enumeration has values that are :

a. type hierarchy based

ExactType - which is the normal filtering behaviour for a registered ISpecimenBuilder for a type.

DirectBaseType

ImplementedInterfaces

For both DirectBaseType and ImplementedInterfaces there are corresponding DirectBaseTypeSpecification and ImplementedInterfaceSpecification.

Note that these IRequestSpecifcation, when used alone, are lenient in that will be satisfied by exact type. The FrozenAttribute denies this though.

<details>
<summary>e.g</summary>

```C#
new AndRequestSpecification(
    new InverseRequestSpecification(
        new ExactTypeSpecification(type)),
    new ImplementedInterfaceSpecification(type))
```

</details>

DirectBaseType - the request Type has to be for the test parameter Type's base type.

ImplementedInterfaces - the request Type has to be for an interface of the test paramer Type.

<details>
<summary>Type matching demo</summary>

```C#
public class DemoTypeMatchingTests
{
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeDemo.BaseType, Is.SameAs(derivedType));
            Assert.That(typeDemo.DerivedType, Is.Not.SameAs(derivedType));
            Assert.That(typeDemo.FurtherDerivedType, Is.Not.SameAs(derivedType));
            Assert.That(typeDemo.Interface, Is.SameAs(implementingInterface));
            Assert.That(typeDemo.ImplementingInterface, Is.Not.SameAs(implementingInterface));
            Assert.That(typeDemo.ExactType, Is.SameAs(exactType));
            Assert.That(typeDemo.DerivedExactType, Is.Not.SameAs(exactType));
        }
    }
}
```

</details>

b. context based

ParameterName
FieldName
PropertyName

These are all of the form :

```C#
new ParameterSpecification(
    new ParameterTypeAndNameCriterion(
        DerivesFrom(type),
        IsNamed(name)))
```

The IRequestSpecification will be partly satisfied if the request object is ParameterInfo ( FieldInfo / PropertyInfo for FieldName / PropertyName ).
Then the type and name criterion will also need to be satisfied. That the test method parameter type derives from the request ParameterInfo.ParameterType, ( FieldInfo.FieldType / PropertyInfo.PropertyType ) and that the request ParameterInfo.Name ( FieldInfo.Name / PropertyInfo.Name ) is the same as the test method parameter name, ignoring case.

The name criterion requires that you ensure you select the method parameter name correctly and will not survive refactors. So I have created :
[FrozenNamedAttribute and FrozenParameterIndexAttribute](README.md#nunit-frozenattribute-additions)

<details>

<summary>Tests demonstrating frozen attributes</summary>

The test demonstrates :

a. The request for parameters, properties or fields is satisfied for test method parameter types that are derivations.

That AutoFixture supplies the same derived type for test method parameters as it does for the sut parameters and the fields and properties from the auto properties behaviour.

That each dependency is unique.

b. The custom frozen attribute alongside FrozenAttribute. The custom attribute not being test parameter name dependent.

```C#
internal class DemoFrozenTests
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

        [FrozenNamed(nameof(ForFreeze.FieldDependency1),false)] DerivedForFreezeDependency firstFieldDependency,
        [Frozen(Matching.FieldName)] DerivedForFreezeDependency fieldDependency2,

        ForFreeze forFreeze
    )
    {
        var (param1, param2, prop1, prop2, field1, field2) = forFreeze.GetDependencies();
        var allDependencies = new[] { param1, param2, prop1, prop2, field1, field2 };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(param1, Is.SameAs(p1Dependency));
            Assert.That(param2, Is.SameAs(parameterDependency2));
            Assert.That(prop1, Is.SameAs(firstPropertyDependency));
            Assert.That(prop2, Is.SameAs(PropertyDependency2));
            Assert.That(field1, Is.SameAs(firstFieldDependency));
            Assert.That(field2, Is.SameAs(fieldDependency2));

            Assert.That(allDependencies, Is.Unique);
        }
    }
}
```

</details>

<details>
<summary>Frozen and mocks</summary>

It is not possible to do the following :

Given

```C#
public interface IDependency { }

public class SUT(IDependency dependency1, IDependency dependency2)
{
    public IDependency Dependency1 => dependency1;
    public IDependency Dependency2 => dependency2;
}
```

There will be ParameterInfo requests for IDependency.

The AutoMoqCustomization ResidueCollector will relay a type request for `Mock<IDependency>` which will not be resolved ( satisfied ) by the
ISpecimenBuilder added by the FrozenAttribute ICustomization as that matches against ParameterInfo requests.

```C#
// https://github.com/AutoFixture/AutoFixture/issues/1134
[Test, MoqAutoData]
public void IncorrectMethod(
    [Frozen(Matching.ParameterName)] Mock<IDependency> dependency1,
    SUT sut
)
{
    Assert.That(sut.Dependency1, Is.Not.SameAs(dependency1.Object));
}
```

This will work though

```C#
[Test, MoqAutoData]
public void CorrectMethod(
    [Frozen(Matching.ParameterName)] IDependency dependency1,
    SUT sut
)
{
    Assert.That(sut.Dependency1, Is.SameAs(dependency1));
    var mock = Mock.Get(dependency1);
}
```

If you do want `[Frozen(Matching.ParameterName)] Mock<IDependency> dependency1` behaviour there is Hallett.AutoFixture.NUnit4.Moq FrozenMockAttribute.

</details>

Given that test method parameters originate from a ParameterInfo request, you can use any type of attribute applied to the parameter
to influence a request for that ParameterInfo if you write an ISpecimenBuilder that looks for that attribute. [Demo](#ispecimenbuilder-reading-normal-parameter-attributes)

# Moq ICustomization

`AutoMoqCustomization` adds a relay ISpecimenBuilder to the fixture ResidueCollectors that will relay ( context.Resolve ) a type request for type Mock<T> that will be handled by the ISpecimenBuilder it adds to Customizations, with the relay returning the `Mock<T>.Object`.

If AutoMoqCustomization.GenerateDelegates is true it adds the relay as a customization for delegate types as well.

( If so desired, the residue collectors relay can be set with the Relay property. The MockRelay class is public and works with an IRequestSpecification )

## The Mock customization ISpecimenBuilder - MockPostprocessor

This handles Type requests for `Mock<>` for which it will use MethodInvoker but with a IMethodQuery specific to Moq, MockConstructorQuery.
It then configures the Mock to CallBase and have DefaultValue ( unexpected invocations on loose mocks ) Mock.
If AutoMoqCustomization.ConfigureMembers is true then it is wrapped in a proper Postprocessor with 3 ISpecimenCommand for configuring the created mock.

StubPropertiesCommand - mock.SetUpAllProperties();

MockVirtualMethodsCommand - The overridable, non generic, non void or void with out parameters methods of the mocked type ( excluding writable property getters ) have expression trees
created and mock.Setup is invoked with the expression tree. If the mocked method returns a result it will be resolved from the fixture when it is invoked.
The setup will be It.IsAny for all parameters except for out parameters which are immediately resolved from the fixture. If any out parameter is resolved as OmitSpecimen then there will be no setup.

AutoMockPropertiesCommand - This uses AutoPropertiesCommand with a custom IRequestSpecification on the **mock.Object** that disallows setting fields generated by Castle's DynamicProxy.
**Thus the customization ignores the fixture OmitAutoProperties**. This is also true for the ICustomization for the other mocking libraries.

This can be rectified by creating a custom ICustomization based on the original code. [Hallett.AutoFixture.Moq.EnhancedAutoMoqCustomization](README.md#enhancedautomoqcustomization)

If you need a specific mock setup you could inject your own but if you want the moq customization behaviour and a specific setup :

a. Freeze a mock of type you want mocked. As this will Create and the moq customization will be applied.

b. Create a custom AutoMoqCustomization [Hallett.AutoFixture.Moq.EnhancedAutoMoqCustomization](README.md#enhancedautomoqcustomization)

<details>
    <summary>Some fixture extension methods and derivations for Moq</summary>

```C#
public static class FixtureExtensions
{
    public static T InjectGetBack<T>(this IFixture fixture, T value)
    {
        fixture.Inject(value);
        return value;
    }

    public static void InjectAs<T, TInstance>(this IFixture fixture, TInstance value) where T : class where TInstance : T
    {
        fixture.Inject<T>(value);
        fixture.Inject(value);
    }
}

public static class MockFixtureExtensions
{
    /// Bypass the AutoMoqCustomization
    public static Mock<T> InjectMock<T>(this IFixture fixture) where T : class => fixture.InjectGetBack(new Mock<T>());

    /// The AutoMoqCustomizatiion will configure the mock.
    public static Mock<T> FreezeMock<T>(this IFixture fixture) where T : class => fixture.Freeze<Mock<T>>();
}
```

```C#
public class MoqFixture : Fixture
{
    public MoqFixture(bool configureMembers = true, bool generateDelegates = true, bool omitAutoProperties = true)
    {
        Customize(new AutoMoqCustomization { ConfigureMembers = configureMembers, GenerateDelegates = generateDelegates });
        OmitAutoProperties = omitAutoProperties;
    }
}

public sealed class MoqAutoDataAttribute(
    bool configureMembers = true,
    bool generateDelegates = true,
    bool omitAutoProperties = true) : AutoDataAttribute(() => new MoqFixture(configureMembers, generateDelegates, omitAutoProperties)) { }
```

</details>

# Other helpers

SpecimenQuery adds Get extension methods to ISpecimenBuilder that `Create<T>()` and return the result type from the Func argument.
Similarly SpecimenCommand adds Do extension methods for Action arguments.

# Demos

## Build vs Customize demo

<details>
    <summary>A demo of Build vs Customize</summary>

```C#
public class DemoAutoFixtureTests
{
    [Test]
    public void DemoBuildVsCustomize()
    {
        var fixture = new Fixture();
        fixture.Inject(99);
        fixture.Inject("Injected String");
        var fromAutoPropeties = fixture.Create<BuildOrCustomize>();

        Assert.That(fromAutoPropeties.StringProperty, Is.EqualTo("Injected String"));
        Assert.That(fromAutoPropeties.IntProperty, Is.EqualTo(99));
        // demo Do extension method on ISpecimenBuilder
        fixture.Do<BuildOrCustomize>(buildOrCustomizeFromCreate => Assert.That(buildOrCustomizeFromCreate, Is.EqualTo(fromAutoPropeties)));

        // all composition has to be done in the callback.  Keeping the composer as a variable will not work as its methods produce new ISpecimenBuilder
        fixture.Customize<BuildOrCustomize>(composer =>
        {
            return composer.OmitAutoProperties().Do(buildOrCustomize => buildOrCustomize.Do());
        });
        AssertCustomizeOmitAutoProperties();

        var buildAutoProperties = fixture.Build<BuildOrCustomize>().Create();
        Assert.That(buildAutoProperties.StringProperty, Is.EqualTo("Injected String")); // Build gets the Customizations from the fixture graph
        Assert.That(buildAutoProperties.IntProperty, Is.EqualTo(99));
        // has not affected the fixture graph
        AssertCustomizeOmitAutoProperties();

        var buildOmitAutoProperties = fixture.Build<BuildOrCustomize>().OmitAutoProperties().Create();
        Assert.That(buildOmitAutoProperties.StringProperty, Is.Null);
        Assert.That(buildOmitAutoProperties.IntProperty, Is.Null);
        Assert.That(buildOmitAutoProperties.ForDo, Is.Null);

        var buildWithout = fixture.Build<BuildOrCustomize>().Without(buildOrCustomize => buildOrCustomize.IntProperty).Create();
        Assert.That(buildWithout.StringProperty, Is.EqualTo("Injected String"));
        Assert.That(buildWithout.IntProperty, Is.Null);

        var buildOmitAutoPropertiesWithResolvedProperty = fixture.Build<BuildOrCustomize>().OmitAutoProperties().With(buildOrCustomize => buildOrCustomize.StringProperty).Create();
        Assert.That(buildOmitAutoPropertiesWithResolvedProperty.StringProperty, Is.EqualTo("Injected String"));
        Assert.That(buildOmitAutoPropertiesWithResolvedProperty.IntProperty, Is.Null);

        var intFactoryInt = 0;
        Func<int?> intFactory = () => intFactoryInt++;
        var buildFactorySpecimenBuilder = fixture.Build<BuildOrCustomize>().With(buildOrCustomize => buildOrCustomize.IntProperty, intFactory);
        var firstBuildPropertyFromFactory = buildFactorySpecimenBuilder.Create();
        var secondBuildPropertyFromFactory = buildFactorySpecimenBuilder.Create();
        Assert.That(firstBuildPropertyFromFactory.IntProperty, Is.EqualTo(0));
        Assert.That(secondBuildPropertyFromFactory.IntProperty, Is.EqualTo(1));


        void AssertCustomizeOmitAutoProperties(){
            var customizeOmitAutoProperties = fixture.Create<BuildOrCustomize>();
            Assert.That(customizeOmitAutoProperties.StringProperty, Is.Null);
            Assert.That(customizeOmitAutoProperties.IntProperty, Is.Null);
            Assert.That(customizeOmitAutoProperties.ForDo, Is.EqualTo("Done"));
        }
    }
}
```

</details>

## SeededRequest ISpecimenBuilder Demo

<details>
<summary>Demo ISpecimenBuilder for SeededRequest requests</summary>

Here is an example of an ISpecimenBuilder for SeededRequest requests. Although you could achieve the same with Fixture Build / Customize.

```C#
public class UpfrontSeededRequestSpecimenBuilder<T>(Dictionary<object, T> seedValues) : ISpecimenBuilder where T : notnull
{
    private readonly FilteringSpecimenBuilder filteringSpecimenBuilder = new(
            new SeededRequestDictionarySpecimenBuilder(
                seedValues.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value)
                ), new SeedRequestSpecification(typeof(T)));

    public object Create(object request, ISpecimenContext context)
    {
        return filteringSpecimenBuilder.Create(request, context);
    }
}

public class SeededRequestDictionarySpecimenBuilder(Dictionary<object, object> seedValues) : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is SeededRequest seededRequest && seededRequest.Seed != null)
        {
            if (seedValues.TryGetValue(seededRequest.Seed, out var value))
            {
                return value;
            }
        }
        return new NoSpecimen();
    }
}
```

Demo

```C#
record SeededProperty(string Id) { }

class Seeded
{
    public SeededProperty? Property1 { get; set; }

    public SeededProperty? Property2 { get; set; }

}

public class DemoAutoFixtureTests
{
    [Test]
    public void Demo_SeededRequest_Handling()
    {
        var fixture = new Fixture();

        fixture.Customizations.Insert(0,
                new UpfrontSeededRequestSpecimenBuilder<SeededProperty>(
                    new Dictionary<object, SeededProperty> {
                    { nameof(Seeded.Property1), new SeededProperty("Value1") },
                    { nameof(Seeded.Property2), new SeededProperty("Value2") }
                }));

        var seeded = fixture.Create<Seeded>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(seeded.Property1!.Id, Is.EqualTo("Value1"));
            Assert.That(seeded.Property2!.Id, Is.EqualTo("Value2"));
        }
    }
}
```

</details>

## ISpecimenBuilder reading normal parameter attributes

<details>
<summary>ISpecimenBuilder reading "normal" parameter attributes.</summary>
The example below demonstrates a few concepts.

First, the behaviour is available as an ICustomization that can be applied to any fixture.

A CompositeCustomization is used as two ISpecimenBuilder instances are added to the fixture by the ICustomization returned from the ToCustomization extension method on ISpecimenBuilder.
There are two specimen builders as the FilteringSpecimenBuilder filters by requests by ParameterInfo and the presence of the TaggedParameterAttribute, and the TaggedParameterRelay creates a new TaggedRequest with the tag and parameter type information for the context to resolve.
Without the TaggedParameterRequestSpecimenBuilder the request will not be handled and no specimen will be returned.

```C#
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class TaggedParameterAttribute(string tag) : Attribute {
    public string Tag => tag;

    public static bool IsTaggedParameter(ParameterInfo parameterInfo) => GetAttribute(parameterInfo) != null;

    private static TaggedParameterAttribute? GetAttribute(ParameterInfo parameterInfo) => parameterInfo.GetCustomAttribute<TaggedParameterAttribute>();

    public static string GetTag(ParameterInfo parameterInfo)
    {
        var taggedParameterAttribute = parameterInfo.GetCustomAttribute<TaggedParameterAttribute>();
        return taggedParameterAttribute == null
            ? throw new InvalidOperationException($"Parameter {parameterInfo.Name} is not tagged with {nameof(TaggedParameterAttribute)}")
            : taggedParameterAttribute.Tag;
    }
}

public class TaggedParameterSpecification : ParameterSpecification {
    private class IsTaggedParameter : IEquatable<ParameterInfo>
    {
        public bool Equals(ParameterInfo? other)
        {
            return TaggedParameterAttribute.IsTaggedParameter(other!);
        }
    }
    public TaggedParameterSpecification() : base(new IsTaggedParameter()) { }
}

public record TaggedRequest(string Tag, Type TargetType) {
    public bool ValueIsAssignableTo(object value) => value.GetType().IsAssignableTo(TargetType);
}

public class TaggedParameterRelay : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var parameterInfo = (ParameterInfo)request;
        var tag = TaggedParameterAttribute.GetTag(parameterInfo);
        return context.Resolve(new TaggedRequest(tag, parameterInfo.ParameterType));
    }
}

public class TaggedParameterRequestSpecimenBuilder(Dictionary<string, object> parameters) : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is TaggedRequest taggedRequest &&
            parameters.TryGetValue(taggedRequest.Tag, out var value) &&
            taggedRequest.ValueIsAssignableTo(value))
        {
            return value;
        }

        return new NoSpecimen();
    }
}

class TaggedParameterCustomization : CompositeCustomization
{
    public TaggedParameterCustomization(Dictionary<string, object> taggedObjects) : base(
        new TaggedParameterRequestSpecimenBuilder(taggedObjects).ToCustomization(),
        new FilteringSpecimenBuilder(
            new TaggedParameterRelay(),
            new TaggedParameterSpecification()).ToCustomization()
        )
    {  }
}
```

usage

```C#
record class Tagged(string Id);

internal class TaggedCustomizationFixture : AutoDataAttribute
{
    public TaggedCustomizationFixture() : base(CreateFixture) { }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        return fixture.Customize(
            new TaggedParameterCustomization(
                new Dictionary<string, object> {
                    { "Tag1", new Tagged("Tag1") },
                    { "Tag2", new Tagged("Tag2") } }));
    }
}

internal class DemoAutoFixtureTests
{
    [Test, TaggedCustomizationFixture]
    public void Should_Get_Tagged_From_Customization(
        [Frozen] string frozenString,
        [TaggedParameter("Tag2")] Tagged tagged2,
        [TaggedParameter("Tag1")] Tagged tagged1,
        Tagged fromMethodInvoker
        )
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tagged1.Id, Is.EqualTo("Tag1"));
            Assert.That(tagged2.Id, Is.EqualTo("Tag2"));
            Assert.That(fromMethodInvoker.Id, Is.EqualTo(frozenString));
        }
    }
}
```

</details>

## Demo of IMethodQuery

<details>

<summary>Example of a custom IMethodQuery</summary>

```C#
public class DecoratingParameterMethodQuery(
    IMethodQuery methodQuery,
    Func<IEnumerable<object>, IEnumerable<object>> decorator) : IMethodQuery
{
    public class DecoratingMethod(IMethod method, Func<IEnumerable<object>, IEnumerable<object>> decorator) : IMethod
    {
        public IEnumerable<System.Reflection.ParameterInfo> Parameters => method.Parameters;

        public object Invoke(IEnumerable<object> parameters) => method.Invoke(decorator(parameters));
    }

    public IEnumerable<IMethod> SelectMethods(Type type) => methodQuery.SelectMethods(type).Select(m => new DecoratingMethod(m, decorator));
}
```

Demo

```C#
class MethodInvokerParameter
{
    public virtual string Invoke()
    {
        return "123";
    }
}

class ForMethodInvoker(DemoAutoFixtureTests.MethodInvokerParameter parameter)
{
    public string InvokeParameter()
    {
        return parameter.Invoke();
    }
}

class DecoratingMethodInvokerParameter(MethodInvokerParameter decorated) : MethodInvokerParameter
{
    public override string Invoke()
    {
        return "prefix" + decorated.Invoke();
    }
}

class DemoAutoFixtureTests
{
    [Test]
    public void DemoMethodInvoker()
    {
        var methodInvoker = new MethodInvoker(
            new DecoratingParameterMethodQuery(
                new ModestConstructorQuery(),
                parameters => parameters.Select(p => p is MethodInvokerParameter toDecorate ? new DecoratingMethodInvokerParameter(toDecorate) : p)
        ));

        var fixture = new Fixture();
        fixture.Customizations.Add(new FilteringSpecimenBuilder(methodInvoker, new ExactTypeSpecification(typeof(ForMethodInvoker))));

        var forMethodInvoker = fixture.Create<ForMethodInvoker>();
        Assert.That(forMethodInvoker.InvokeParameter(), Is.EqualTo("prefix123"));
    }
}
```

</details>

## Hallett.AutoFixture.NUnit4

<details>
<summary>Hallett.AutoFixture.NUnit4</summary>

<details>
<summary>How AutoFixture hooks into NUnit</summary>

[NUnit framework extensibility](https://docs.nunit.org/articles/nunit/extending-nunit/Framework-Extensibility.html)

[Custom Attributes](https://docs.nunit.org/articles/nunit/extending-nunit/Custom-Attributes.html)

> NUnit 3 implements a great deal of its functionality in its attributes. This functionality is accessed through a number of standard interfaces, which are implemented by the attributes. Users may create their own attributes by implementing these interfaces.

The interfaces of interest :

[IImplyFixture - Attributes used on a method to signal that the defining class should be treated as a fixture](https://docs.nunit.org/articles/nunit/extending-nunit/IImplyFixture-Interface.html)

`TestCaseSourceAttribute` implements this marker interface.

[IParameterDataSource - Attributes that supply values for a single parameter for use in generating test cases](https://docs.nunit.org/articles/nunit/extending-nunit/IParameterDataSource-Interface.html)

IParameterDataSource is mentioned when creating AutoCombiningStrategyAttribute later.

[ITestBuilder - Attributes that know how to build one or more parameterized test cases for a method](https://docs.nunit.org/articles/nunit/extending-nunit/ITestBuilder-Interface.html)

Both `AutoDataAttribute` and `InlineAutoDataAttribute` inherit `ITestBuilder`.

The AutoFixture IParameterCustomizationSource and ParameterInfo resolution has already been discussed earlier so the NUnit specific part is
`TestMethodBuilder.Build`. The only difference between the two attributes is the last argument.

```C#
// AutoDataAttribute
public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
{
    if (method == null) throw new ArgumentNullException(nameof(method));

    var test = TestMethodBuilder.Build(method, suite, GetParameterValues(method), 0);

    yield return test;
}

// InlineAutoDataAttribute
public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
{
    if (method == null) throw new ArgumentNullException(nameof(method));

    var test = TestMethodBuilder.Build(
        method, suite, GetParameterValues(method), _existingParameterValues.Length);

    yield return test;
}
```

The TestMethodBuilder is a public property so you could supply your own in a derivation. The default is `FixedNameTestMethodBuilder`.
The main responsibility of this class is to wrap the parameter values in TestCaseParameters and change how AutoFixture provided test parameters appear ([OriginalArguments](https://github.com/nunit/nunit/blob/c677533fc5fb44eeb814c0bde8c9429e5cd15722/src/NUnitFramework/framework/Internal/TestParameters.cs#L138)), for instance in Visual Studio Test Explorer. They are of the form `$"auto<{Type.Name}>"`.

It then uses the NUnit helper `NUnitTestCaseBuilder`

`public TestMethod BuildTestMethod(IMethodInfo method, Test? parentSuite, TestCaseParameters? parms)`

</details>

<details>
<summary>CancelAfter aware AutoDataAttribute and InlineAutoDataAttribute</summary>

<details>
<summary>How CancelAfterAttribute works</summary>

The [NUnitTestCaseBuilder](https://github.com/nunit/nunit/blob/c677533fc5fb44eeb814c0bde8c9429e5cd15722/src/NUnitFramework/framework/Internal/Builders/NUnitTestCaseBuilder.cs?plain=1#L210C14-L210C14) allows for one less parameter to be supplied.

As the [TestMethodCommand](https://github.com/nunit/nunit/blob/59ecf34a6f30a89742d875ec2d02311cb3bfabe6/src/NUnitFramework/framework/Internal/Commands/TestMethodCommand.cs#L90) will supply it.

The SimpleWorkItem [looks for PropertyNames.UseCancellation](https://github.com/nunit/nunit/blob/59ecf34a6f30a89742d875ec2d02311cb3bfabe6/src/NUnitFramework/framework/Internal/Execution/SimpleWorkItem.cs#L151) and wraps in a CancelAfterCommand that adds the CancellationToken to the test context.

```
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CancelAfterAttribute : PropertyAttribute, IApplyToContext
    {
        private readonly int _timeout;

        /// <summary>
        /// Construct a CancelAfterAttribute given a time in milliseconds
        /// </summary>
        /// <param name="timeout">The timeout value in milliseconds</param>
        public CancelAfterAttribute(int timeout)
            : base(PropertyNames.Timeout, timeout)
        {
            _timeout = timeout;
            Properties.Add(PropertyNames.UseCancellation, true);
        }

        #region IApplyToContext

        void IApplyToContext.ApplyToContext(TestExecutionContext context)
        {
            context.TestCaseTimeout = _timeout;
            context.UseCancellation = true;
        }

        #endregion
    }

```

</details>

To get the last parameter, a CancellationToken, to be provided by NUnit the default TestMethodBuilder is changed.

```C#
public class AutoTestAttribute : AutoDataAttribute
{
    public AutoTestAttribute()
    {
        TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }

    protected AutoTestAttribute(Func<IFixture> fixtureFactory) : base(fixtureFactory)
    {
        TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }
}

public class AutoTestCaseAttribute : InlineAutoDataAttribute
{
    public AutoTestCaseAttribute(params object?[] values) : base(values)
    {
        TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }

    internal AutoTestCaseAttribute(Func<IFixture> fixtureFactory, params object?[] arguments) : base(fixtureFactory, arguments)
    {
        TestMethodBuilder = new FixedNameTestMethodBuilderCancellationTokenAware();
    }
}

```

The FixedNameTestMethodBuilderCancellationTokenAware uses the default ITestMethodBuilder with the extension method `ConsiderCancelAfterAttribute`.

```C#
internal class FixedNameTestMethodBuilderCancellationTokenAware : ITestMethodBuilder
{
    private readonly FixedNameTestMethodBuilder fixedNameTestMethodBuilder = new();
    public virtual TestMethod Build(
        IMethodInfo method, Test suite, IEnumerable<object> parameterValues, int autoDataStartIndex)
    {
        return fixedNameTestMethodBuilder.Build(method, suite, parameterValues, autoDataStartIndex).ConsiderCancelAfterAttribute(suite);
    }
}
```

Tests

```C#
internal class DemoCancelAttributeTests
{
    [Test, CancelAfter(1000), AutoDataCancel]
    public void AutoDataCancelAttribute_Cancels_Test(
        IFixture fixture,
        CancellationToken nunitCancellationToken)
    {
        Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
    }

    [Test, CancelAfter(1000), InlineAutoDataCancel(1)]
    public void InlineAutoDataCancelAttribute_Cancels_Test(
        int inline,
        IFixture fixture,
        CancellationToken nunitCancellationToken)
    {
        Assert.That(nunitCancellationToken, Is.EqualTo(TestContext.CurrentContext.CancellationToken));
    }
}
```

`ConsiderCancelAttribute` is an extension method and not part of FixedNameTestMethodBuilderCancellationTokenAware as the behaviour is required for the
additional parameterized test attributes.

For a TestMethod attributed with CancelAfterAttribute with a final CancellationToken parameter not marked with AutoAttribute ( and with the CancellationToken argument provided ), it will create a new TestMethod ( from the InlineAutoDataAttribute TestMethod ) without the CancellationToken.

```C#
internal static class CancelAfterAttributeExtensions
{
    private class RemoveCancellationTokenTestMethod : TestMethod
    {
        public RemoveCancellationTokenTestMethod(TestMethod testMethod, Test? parentSuite) : base(testMethod.Method, parentSuite)
        {
            this.Arguments = RemoveCancellationToken(testMethod.Arguments);
            RemoveCancellationTokenFromNames(testMethod.Name, parentSuite);
        }

        private static object?[] RemoveCancellationToken(object?[] arguments)
        {
            var newArguments = new object?[arguments.Length - 1];
            Array.Copy(arguments, newArguments, arguments.Length - 1);
            return newArguments;
        }

        private void RemoveCancellationTokenFromNames(string testName, Test? parentSuite)
        {
            testName = testName.Replace(",auto<CancellationToken>", string.Empty);
            Name = testName;
            FullName = parentSuite is null ? testName : $"{parentSuite.FullName}.{testName}";
        }

        public override object?[] Arguments { get; }

    }

    public static TestMethod ConsiderCancelAfterAttribute(this TestMethod test, Test? suite)
    {
        var useCancellationTokenFromNUnit = test.TestHasAllArguments()
            && test.LastParameterIsCancellationTokenAndNotForAutoFixture()
            && test.Method.HasCancelAfterAttribute();

        return useCancellationTokenFromNUnit ? new RemoveCancellationTokenTestMethod(test, suite) : test;
    }

    private static bool TestHasAllArguments(this TestMethod test)
        => test.Arguments.Length == test.Method.GetParameters().Length;

    private static bool HasCancelAfterAttribute(this IMethodInfo method)
        => method.GetCustomAttributes<CancelAfterAttribute>(true).Length > 0;

    private static bool LastParameterIsCancellationTokenAndNotForAutoFixture(this TestMethod test)
    {
        var parameters = test.Method.GetParameters();
        if (parameters.Length == 0)
        {
            return false;
        }

        var lastParameter = parameters[^1];
        return lastParameter.ParameterType == typeof(CancellationToken) && lastParameter.GetCustomAttributes<AutoAttribute>(true).Length == 0;
    }
}


```

</details>

## AutoFixture NUnit Parameterized Tests

<details>
    <summary>AutoCombiningStrategyAttribute and derivations</summary>

The NUnit CombiningStrategyAttribute is an abstract base attribute.

It asks the `IParameterDataProvider`, from derivations, to provide values for each IParameterInfo of a test method.
The `ICombiningStrategy`, from derivations, then combines these parameter values to produce different combinations of parameters, thus creating multiple tests.

<details>
<summary>NUnit CombiningStrategyAttribute</summary>

```C#
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public abstract class CombiningStrategyAttribute : NUnitAttribute, ITestBuilder, IApplyToTest
{
    private readonly NUnitTestCaseBuilder _builder = new();

    private readonly ICombiningStrategy _strategy;
    private readonly IParameterDataProvider _dataProvider;

    protected CombiningStrategyAttribute(ICombiningStrategy strategy, IParameterDataProvider provider)
    {
        _strategy = strategy;
        _dataProvider = provider;
    }


    /// This constructor is provided for CLS compliance.
    protected CombiningStrategyAttribute(object strategy, object provider)
        : this((ICombiningStrategy)strategy, (IParameterDataProvider)provider)
    {
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
    {
        List<TestMethod> tests = new();

        IParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length > 0)
        {
            int parametersToSupply = parameters.LastParameterAcceptsCancellationToken() ?
                parameters.Length - 1 : parameters.Length;

            IEnumerable[] sources = new IEnumerable[parametersToSupply];

            try
            {
                for (int i = 0; i < parametersToSupply; i++)
                    sources[i] = _dataProvider.GetDataFor(parameters[i]);
            }
            catch (InvalidDataSourceException ex)
            {
                var parms = new TestCaseParameters();
                parms.RunState = RunState.NotRunnable;
                parms.Properties.Set(PropertyNames.SkipReason, ex.Message);
                tests.Add(_builder.BuildTestMethod(method, suite, parms));
                return tests;
            }

            foreach (var parms in _strategy.GetTestCases(sources))
                tests.Add(_builder.BuildTestMethod(method, suite, (TestCaseParameters)parms));
        }

        return tests;
    }

    public void ApplyToTest(Test test)
    {
        var joinType = _strategy.GetType().Name;
        if (joinType.EndsWith("Strategy", StringComparison.Ordinal))
            joinType = joinType.Substring(0, joinType.Length - 8);

        test.Properties.Set(PropertyNames.JoinType, joinType);
    }
}

```

</details>

_Note that HasDataFor is not used by NUnit_

```C#
public interface IParameterDataProvider
{
    bool HasDataFor(IParameterInfo parameter);

    IEnumerable GetDataFor(IParameterInfo parameter);
}
```

**Although this returns ITestCaseData NUnit casts** [to TestCaseParameters](https://github.com/nunit/nunit/blob/59ecf34a6f30a89742d875ec2d02311cb3bfabe6/src/NUnitFramework/framework/Attributes/CombiningStrategyAttribute.cs#L85) see [issue](https://github.com/nunit/nunit/issues/3803)

```C#
public interface ICombiningStrategy
{
    IEnumerable<ITestCaseData> GetTestCases(IEnumerable[] sources);
}
```

NUnit provides 3 derivations. From these we can see there are 3 different stategies and that NUnit has 2 proper IParameterDataProvider implementations ( the third is just a container ) where DatapointProvider is specific to the TheoryAttribute.

<details>
<summary>NUnit derivations</summary>

```C#
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CombinatorialAttribute : CombiningStrategyAttribute
{
    public CombinatorialAttribute() : base(new CombinatorialStrategy(), new ParameterDataSourceProvider())
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PairwiseAttribute : CombiningStrategyAttribute
{
    public PairwiseAttribute() : base(new PairwiseStrategy(), new ParameterDataSourceProvider())
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class SequentialAttribute : CombiningStrategyAttribute
{
    public SequentialAttribute() : base(new SequentialStrategy(), new ParameterDataSourceProvider())
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TheoryAttribute : CombiningStrategyAttribute, ITestBuilder, IImplyFixture
{
    public TheoryAttribute(bool searchInDeclaringTypes = false) : base(
        new CombinatorialStrategy(),
        new ParameterDataProvider(new DatapointProvider(searchInDeclaringTypes), new ParameterDataSourceProvider()))
    {
    }
}
```

</details>

<details>
<summary>ParameterDataSourceProvider</summary>

This just asks the IParameterDataSource attribute applied to the parameter to provide values

```C#
public class ParameterDataSourceProvider : IParameterDataProvider
{
    public bool HasDataFor(IParameterInfo parameter)
    {
        return parameter.IsDefined<IParameterDataSource>(false);
    }

    public IEnumerable GetDataFor(IParameterInfo parameter)
    {
        var data = new List<object?>();

        foreach (IParameterDataSource source in parameter.GetCustomAttributes<IParameterDataSource>(false))
        {
            foreach (object? item in source.GetData(parameter))
                data.Add(item);
        }

        return data;
    }
}

```

There are 4 implementations

[RangeAttribute](https://github.com/nunit/nunit/blob/main/src/NUnitFramework/framework/Attributes/RangeAttribute.cs) [docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/random.html)

[ValuesAttribute](https://github.com/nunit/nunit/blob/main/src/NUnitFramework/framework/Attributes/ValuesAttribute.cs) [docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/values.html)

[RandomAttribute](https://github.com/nunit/nunit/blob/main/src/NUnitFramework/framework/Attributes/RandomAttribute.cs) [docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/random.html)

[ValueSourceAttribute](https://github.com/nunit/nunit/blob/main/src/NUnitFramework/framework/Attributes/ValueSourceAttribute.cs) [docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/valuesource.html)

</details>

<details>
<summary>DatapointProvider</summary>

The [DatapointProvider](https://github.com/nunit/nunit/blob/main/src/NUnitFramework/framework/Internal/Builders/DatapointProvider.cs) also uses attributes to discover parameter values, but [DatapointAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/datapoint.html) and [DatapointSourceAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/datapointsource.html) are applied to members and not to the parameters.

The DatapointAttribute can only be applied to fields whereas DatapointSourceAttribute can be applied to fields, properties and parameterless methods.

For the provided IParameterInfo the DatapointProvider will combined all the :

DatapointAttribute attributed fields with type that exactly matches the IParameterInfo type.

DatapointSource attributed members, arrays or `IEnumerable<T>`, with element type that exactly matches the IParameterInfo type.

The TheoryAttribute has a single constructor parameter, `public TheoryAttribute(bool searchInDeclaringTypes = false)` that controls if the search goes through base types.

The members are found with `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy`

For instance members the DatapointProvider will create a single cached instance of the owning type via the default constructor.

</details>

AutoFixture can have the same behaviour by using the IParameterDataProvider and ICombiningStrategy with the same inheritance hierarchy.

Note that NUnit does not require a CombiningStrategyAttribute to be applied to a test with IParameterDataSource attributed parameters, a CombinatorialAttribute will be implicitly used. This is necessary when using an AutoCombiningStrategyAttribute.

<details>
<summary>NUnit DefaultTestCaseBuilder</summary>

[source](https://github.com/nunit/nunit/blob/59ecf34a6f30a89742d875ec2d02311cb3bfabe6/src/NUnitFramework/framework/Internal/Builders/DefaultTestCaseBuilder.cs#L82-L97)

```C#
        public Test BuildFrom(IMethodInfo method, Test? parentSuite)
        {
            var tests = new List<TestMethod>();

            try
            {
                var metadata = MethodInfoCache.Get(method);

                List<ITestBuilder> builders = new(metadata.TestBuilderAttributes);

                // See if we need to add a CombinatorialAttribute for parameterized data
                if (method.MethodInfo.GetParameters().Any(param => param.HasAttribute<IParameterDataSource>(false))
                    && !builders.Any(builder => builder is CombiningStrategyAttribute))
                {
                    builders.Add(new CombinatorialAttribute());
                }
```

</details>

As there are two inheritance hierarchies, `AutoCombiningStrategyAttribute` and `AutoCombiningStrategyAttribute<TFixtureFactory>` the common code
is in `AutoCombiningStrategyHelper`. This has similar code to NUnit CombiningStrategyAttribute with regards to the usage of IParameterDataProvider and ICombiningStrategy with the main difference being the requirement of `AutoAttribute` being applied to the first parameter that AutoFixture provides as we do not want the IParameterDataProvider to provide for these.

The helper argument `Func<ITestCaseData, TestMethod> testMethodFromTestCaseData` then creates the TestMethod from the combined provided data.
`AutoCombiningStrategyAttribute` and `AutoCombiningStrategyAttribute<TFixtureFactory>` both use another helper, `TestCaseTestMethodCreator`, to do this.
The TestCaseTestMethodCreator is common to AutoCombiningStrategyAttribute and AutoTestCaseSourceAttribute.

<details>
<summary>AutoCombiningStrategyHelper</summary>

```
internal class AutoCombiningStrategyHelper : IAutoCombiningStrategyHelper
{
    private readonly NUnitTestCaseBuilder _builder = new();

    public IEnumerable<TestMethod> BuildFrom(
        IMethodInfo method,
        Test? suite,
        ICombiningStrategy strategy,
        IParameterDataProvider provider,
        Func<ITestCaseData, TestMethod> testMethodFromTestCaseData)
    {
        List<TestMethod> tests = [];

        IParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length > 0)
        {
            int parametersToSupply = GetParametersToSupply(parameters);
            IEnumerable[] sources = new IEnumerable[parametersToSupply];

            try
            {
                for (int i = 0; i < parametersToSupply; i++)
                    sources[i] = provider.GetDataFor(parameters[i]);
            }
            catch (InvalidDataSourceException ex)
            {
                var parms = new TestCaseParameters
                {
                    RunState = RunState.NotRunnable
                };
                parms.Properties.Set(PropertyNames.SkipReason, ex.Message);
                tests.Add(_builder.BuildTestMethod(method, suite, parms));
                return tests;
            }

            foreach (var parms in strategy.GetTestCases(sources))
            {
                tests.Add(testMethodFromTestCaseData(parms));
            }
        }

        return tests;
    }

    public void ApplyStrategyTypeNameToTest(Test test, ICombiningStrategy strategy)
    {
        var joinType = strategy.GetType().Name;
        if (joinType.EndsWith("Strategy", StringComparison.Ordinal))
            joinType = joinType[..^8];

        test.Properties.Set(PropertyNames.JoinType, joinType);
    }

    private static int GetParametersToSupply(IParameterInfo[] parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.GetCustomAttributes<AutoAttribute>(false).Length > 0)
            {
                return i;
            }
        }

        throw new InvalidOperationException("No parameter marked with [Auto] found.");
    }
}
```

</details>

</details>

<details>
<summary>AutoTestCaseSourceAttribute</summary>

I original reused the NUnit TestCaseSourceAttribute, mirroring the constructors and getting NUnit to supply the test case arguments before adding those from AutoFixture. Unfortunately with earlier versions of NUnit the behaviour was different.

`AutoTestCaseSourceAttribute` and `AutoTestCaseSourceAttribute<TFixtureFactory>` have similar behaviour as such there is the TestCaseSourceTestBuilder.

This is fluent api, beginning with `FromSource` with the overload matching the constructor arguments.

e.g
`FromSource(Type sourceType, string sourceName).WithTestCaseSourceBuilder(IFixtureTestCaseSourceBuilder fixtureTestCaseSourceBuilder).GetCases(IMethodInfo method) : IEnumerable<object[]>?`

With the reflection code behind `TestCaseSource`. This allows for specifying the source in the way as AutoFixture.xUnit3 MemberDataSourceAttribute and ClassDataSource but is not so strict on the data source. These require `IEnumerable<object[]>` whereas AutoTestCaseSourceAttribute works with IEnumerable of IEnumerable.

<details>
<summary>TestCaseSource</TestCaseSource>

```C#
public class TestCaseSource : ITestCaseSource
{
    private readonly Type? sourceType;
    private readonly object[]? constructorParameters;
    private readonly string? sourceName;
    private readonly object?[]? methodParams;

    public TestCaseSource(string sourceName)
    {
        this.sourceName = sourceName;
    }

    public TestCaseSource(Type sourceType, string sourceName, object?[]? methodParams)
    {
        this.sourceType = sourceType;
        this.sourceName = sourceName;
        this.methodParams = methodParams;
    }

    public TestCaseSource(Type sourceType, string sourceName)
    {
        this.sourceType = sourceType;
        this.sourceName = sourceName;
    }

    public TestCaseSource(string sourceName, object?[]? methodParams)
    {
        this.sourceName = sourceName;
        this.methodParams = methodParams;
    }

    public TestCaseSource(Type sourceType, object[] constructorParameters)
    {
        this.sourceType = sourceType;
        this.constructorParameters = constructorParameters;
    }

    public IEnumerable<object[]>? GetCases(IMethodInfo testMethod)
    {
        var sourceType = this.sourceType ?? testMethod.TypeInfo.Type;
        if (sourceName is null)
        {
            try
            {
                var instance = Reflect.Construct(sourceType, constructorParameters);
                return ToIEnumerableOrThrow(instance, $"type {sourceType}");
            }
            catch (InvalidTestFixtureException)
            {
                throw new TestCaseSourceException($"Data source type {sourceType} cannot be constructed.");
            }
        }

        var members = sourceType.GetMemberIncludingFromBase(sourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (members.Length == 1)
        {
            var member = members[0];
            if (member is FieldInfo field)
            {
                ThrowIfMethodParametersForNonMethod(true);
                return ToIEnumerableOrThrow(field.GetValue(null), $"field {sourceName}");
            }

            if (member is PropertyInfo property)
            {
                ThrowIfMethodParametersForNonMethod(false);
                return ToIEnumerableOrThrow(property.GetValue(null, null), $"property {sourceName}");
            }

            if (member is MethodInfo method)
            {
                ThrowIfMethodParametersCountMismatch(method);
                return ToIEnumerableOrThrow(method.Invoke(null, methodParams), $"method {sourceName} return type");
            }
        }

        return null;
    }

    private static IEnumerable<object[]> ToIEnumerableOrThrow(object? shouldBeEnumerable, string errorContext)
    {
        var errorMessage = $"Data source {errorContext} should be IEnumerable of IEnumerable.";
        if (shouldBeEnumerable is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is IEnumerable enumerableItem)
                {
                    yield return enumerableItem.Cast<object>().ToArray();
                }
                else
                {
                    throw new TestCaseSourceException(errorMessage);
                }
            }
            yield break;
        }

        throw new TestCaseSourceException(errorMessage);
    }

    private void ThrowIfMethodParametersCountMismatch(MethodInfo method)
    {
        if (methodParams is not null && method.GetParameters().Length != methodParams.Length)
        {
            throw new TestCaseSourceException($"Method {sourceName} parameter count does not match the provided method parameters.");
        }
    }

    private void ThrowIfMethodParametersForNonMethod(bool memberIsField)
    {
        if (methodParams != null)
        {
            throw new TestCaseSourceException($"Method parameters are not allowed for {(memberIsField ? "field" : "property")} {sourceName}.");
        }
    }

}

```

</details>

The TestCaseSourceBuilder will for each test case ask the IFixtureTestCaseSourceBuilder for the TestMethod ( supplying the additional AutoFixture arguments).

```C#
public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
{
    try
    {
        var testCaseArguments = TestCaseSourceSource.GetCases(method);
        if (testCaseArguments == null)
        {
            return [];
        }

        return testCaseArguments.Select(testCaseArguments => FixtureTestCaseSourceBuilder.BuildFrom(method, suite, testCaseArguments));
    }
    catch (TestCaseSourceException exception)
    {
        var exceptionTestCaseParameters = new TestCaseParameters() { RunState = RunState.NotRunnable };
        exceptionTestCaseParameters.Properties.Set(PropertyNames.SkipReason, exception.Message);

        return [nunitTestCaseBuilder.BuildTestMethod(method, suite, exceptionTestCaseParameters)];
    }
    catch (Exception exception)
    {
        // don't expect this path
        return [nunitTestCaseBuilder.BuildTestMethod(method, suite, new TestCaseParameters(exception))];
    }
}

```

It handles reflection exceptions so as to supply the test explorer ui with the relevant information.

Like the AutoCombiningStrategyAttribute it uses the common `TestCaseTestMethodCreator`.

</details>

<details>
<summary>TestCaseTestMethodCreator</summary>

This class has the following responsibilities :

1. Allow freezing of test case parameter values provided by a source

This is the same behaviour as AutoFixture.xUnit3 MemberDataSourceAttribute and ClassDataSourceAttribute but has been extended so that a custom Frozen attribute implementing below can be used

```C#
public interface IFreezeTestCaseArgument
{
    IRequestSpecification GetRequestSpecification(ParameterInfo parameter);
}
```

2. Reusing AutoFixture InlineAutoData to supply any additional parameters from an IFixture.

This InlineAutoDataAttribute can either come from a type in which case it is necessary to `ConsiderCancelAfterAttribute` or it can come from `Func<IFixture>`.

```C#
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
```

</details>
</details>
