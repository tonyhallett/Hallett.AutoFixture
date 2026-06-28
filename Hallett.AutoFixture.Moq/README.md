# Hallett.AutoFixture.Moq 

A couple of fixture extension methods and

# EnhancedAutoMoqCustomization

The EnhancedAutoMoqCustomization adds two pieces of functionality to the AutoMoqCustomization:

When ConfigureMembers is true the AutoPropertiesBehaviour property controls whether the AutoMockPropertiesCommand, that AutoMoqCustomization always adds, is added to the Postprocessor.
This command does auto properties on mock.Object without consulting the Fixture.OmitAutoProperties.

`public enum AutoPropertiesBehaviour { FollowFixture, Omit, Enable }`

Secondly, EnhancedAutoMoqCustomization adds a final command to the Postprocessor.

This command allows for

`public void Intercept<T>(Action<Mock<T>> interceptor) where T : class`