using System.Dynamic;
using System.Reflection;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronPython.Runtime;

namespace IronSearch
{
    public class SearchArgument : DynamicObject
    {
        public MusicInfo I { get; } = null!;
        public PeroString PS { get; } = null!;

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
            // First, try direct properties
            var prop = this.GetType().GetProperty(binder.Name, flags);
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

            // Forward to PS
            if (PS != null)
            {
                var psProp = PS.GetType().GetProperty(binder.Name, flags);
                if (psProp != null)
                {
                    result = psProp.GetValue(PS);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }


    public class ExpressionSearchArgument: SearchArgument
    {
        public PythonTuple A { get; } = new();
        public PythonDictionary K { get; } = new();
        public ExpressionSearchArgument(SearchArgument searchBase, PythonTuple args, PythonDictionary kwargs): base(searchBase.I, searchBase.PS)
        {
            A = args;
            K = kwargs;
        }
    }
}