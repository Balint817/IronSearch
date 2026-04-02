using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronPython.Runtime;
using System.Dynamic;
using System.Reflection;

namespace IronSearch
{
    public class SearchArgument : DynamicObject
    {
        public MusicInfo I { get; internal set; } = null!;
        public PeroString PS { get; internal set; } = null!;

        public SearchArgument(MusicInfo mi)
        {
            I = mi;
            PS = new PeroString(0);
        }
        public SearchArgument(MusicInfo mi, PeroString ps)
        {
            I = mi;
            PS = ps;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            var prop = GetType().GetProperty(binder.Name, flags);
            if (prop != null)
            {
                result = prop.GetValue(this);
                return true;
            }

            // Forward to M
            if (I != null)
            {
                var mProp = I.GetType().GetProperty(binder.Name, flags);
                if (mProp != null)
                {
                    result = mProp.GetValue(I);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }


    public class ExpressionSearchArgument : SearchArgument
    {
        public PythonTuple A { get; } = new();
        public PythonDictionary K { get; } = new();
        public ExpressionSearchArgument(SearchArgument searchBase, PythonTuple args, PythonDictionary kwargs) : base(searchBase.I, searchBase.PS)
        {
            A = args;
            K = kwargs;
        }
    }
}