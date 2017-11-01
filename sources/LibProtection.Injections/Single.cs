using System;
using System.Reflection;

namespace LibProtection.Injections
{
    internal class TypeInstantiationException : Exception
    {
        public TypeInstantiationException(string message) : base(message)
        {
        }
    }

    internal static class Single<T> where T : class
    {
        private static volatile T _instance;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();

        static Single()
        {
        }

        public static T Instance
        {
            get
            {
                if (_instance != null) { return _instance; }

                lock (Lock)
                {
                    if (_instance != null) { return _instance; }

                    ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new Type[0], null);

                    if (constructor == null || constructor.IsAssembly)
                    {
                        throw new TypeInstantiationException(
                            $"A private or protected constructor is missing for '{typeof(T).Name}'.");
                    }

                    _instance = (T) constructor.Invoke(null);
                }
                return _instance;
            }
        }
    }
}
