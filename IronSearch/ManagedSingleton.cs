namespace IronSearch
{
    public static class ManagedSingleton<T> where T: new()
    {
        static T? _instance;
        public static T Instance
        {
            get
            {
                return _instance ??= new T();
            }
        }
    }
}
