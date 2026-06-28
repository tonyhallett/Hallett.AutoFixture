using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Collections;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4.Internal.TestCaseSource
{
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
}
