using AutoFixture.Kernel;
using System.Reflection;

namespace Hallett.AutoFixture.NUnit4.Internal
{
    internal static class Criterions
    {
        public static Criterion<string> IsNamedExactly(string name)
        {
            return new Criterion<string>(
                name,
                StringComparer.Ordinal);
        }

        public static Criterion<string> IsNamed(string name)
        {
            return new Criterion<string>(
                name,
                StringComparer.OrdinalIgnoreCase);
        }

        public static Criterion<Type> DerivesFrom(Type type)
        {
            return new Criterion<Type>(
                type,
                new DerivesFromTypeComparer());
        }

        public static Criterion<Type> IsType(Type type)
        {
            return new Criterion<Type>(
                type,
                new IsTypeComparer());
        }

        private class IsTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type? x, Type? y)
            {
                if (y == null && x == null)
                    return true;
                if (y == null)
                    return false;

                return y == x;
            }

            public int GetHashCode(Type obj)
            {
                return 0;
            }
        }

        private class DerivesFromTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type? x, Type? y)
            {
                if (y == null && x == null)
                    return true;
                if (y == null)
                    return false;
                return y.GetTypeInfo().IsAssignableFrom(x);
            }

            public int GetHashCode(Type obj)
            {
                return 0;
            }
        }
    }
}
