# Hallett.AutoFixture.Moq 

A couple of fixture extension methods and

# EnhancedAutoMoqCustomization

The EnhancedAutoMoqCustomization adds three pieces of functionality to the AutoMoqCustomization:

When ConfigureMembers is true the AutoPropertiesBehaviour property controls whether the AutoMockPropertiesCommand, that AutoMoqCustomization always adds, is added to the Postprocessor.
This command does auto properties on mock.Object without consulting the Fixture.OmitAutoProperties.

`public enum AutoPropertiesBehaviour { FollowFixture, Omit, Enable }`

Secondly, EnhancedAutoMoqCustomization adds a final command to the Postprocessor.

This command allows for setting up mocks after they have been created.

`public void Intercept<T>(Action<Mock<T>> interceptor) where T : class`

Thirdly, EnhancedAutoMoqCustomization allows for the creation of mocks with a specific `MockBehavior`.

There are two ways of influencing this :

The first is to set the `EnhancedAutoMoqCustomization` `DefaultIsStrict` property to true.

For mocks created from Type requests this sets for all mocks created.

The second is to apply the `MockParameterStrictAttribute` or `MockParameterLooseAttribute` to parameters of type `Mock<T>`.
These can also be applied to containing methods or classes to influence all `Mock<T>` parameters within that scope unless overridden by a more specific attribute.

When there is a parameter request for a `Mock<T>`, such as for test methods attributed for AutoFixture resolution, these attributes specify the `MockBehavior` with a fallback to the `EnhancedAutoMoqCustomization` `DefaultIsStrict` property.



