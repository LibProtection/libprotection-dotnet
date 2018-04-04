using System.Collections.Generic;

namespace LibProtection.Injections.Extensions
{
    public static class ArrayExtensions
    {
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static int ArrayGetHashCode<T>(T[] a)
        {
            var hashCode = -80248532;

            for(int i = 0; i < a.Length; i++)
            {
                hashCode = hashCode * -1521134295 + a[i].GetHashCode();
            }
            return hashCode;
        }
    }
}
