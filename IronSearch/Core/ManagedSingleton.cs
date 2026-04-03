namespace IronSearch.Core
{
    public static class ManagedSingleton<T> where T : new()
    {
        private static T? _instance;
        public static T Instance
        {
            get
            {
                return _instance ??= new T();
            }
        }
    }
}
