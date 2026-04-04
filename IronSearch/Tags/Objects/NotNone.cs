using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Utils;
using System.Collections;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalNotNone(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "NotNone", varArgs, varKwargs);
            ThrowIfNotMatching(varArgs, 1, "NotNone", varArgs, varKwargs);
            if (varArgs[0] is not IEnumerable ie)
            {
                throw new SearchWrongTypeException("a list", varArgs[0]?.GetType(), "NotNone", varArgs, varKwargs);
            }


            var l = new PythonList();

            var enumerator = ie.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (value is not null)
                {
                    l.Add(value);
                }
            }
            return l;
        }
    }
}
