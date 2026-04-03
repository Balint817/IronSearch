namespace IronSearch.Config
{
    public class GenericValidator<T> : MelonLoader.Preferences.ValueValidator
    {
        public T DefaultValue { get; }
        public GenericValidator(T defaultValue)
        {
            DefaultValue = defaultValue ?? throw new NullReferenceException();
        }
        public override object EnsureValid(object value)
        {
            if (value is not T)
            {
                return DefaultValue!;
            }
            return value;
        }

        public override bool IsValid(object value)
        {
            return value is T;
        }
    }
    public class WaitMultiplierValidator : MelonLoader.Preferences.ValueValidator
    {
        public double DefaultValue { get; }
        public WaitMultiplierValidator(double defaultValue)
        {
            DefaultValue = defaultValue;
        }
        public override object EnsureValid(object value)
        {
            if (value is not double d || d < 1)
            {
                return DefaultValue!;
            }
            return value;
        }

        public override bool IsValid(object value)
        {
            return value is double;
        }
    }
}
