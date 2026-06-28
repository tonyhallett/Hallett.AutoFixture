# Hallett.AutoFixture

There are a couple of extension methods for fixtures.

In particular, [see AutoFixture issue](https://github.com/AutoFixture/AutoFixture/issues/988)

```
public static IFixture Customize<T>(
    this IFixture fixture,
    Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation,
    Action<T> doAfterAutoProperties)
```