using Il2CppAssets.Scripts.PeroTools.Nice.Interface;
using Il2CppInterop.Runtime;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;
using System.Runtime.InteropServices;

namespace IronSearch.Utils
{
    public static class Il2CppExtensions
    {

        internal static bool LowerContains(this PeroString peroString, string compareText, string containsText)
        {
            compareText = (compareText ?? "").ToLowerInvariant();
            containsText = (containsText ?? "").ToLowerInvariant();
            peroString.Clear();
            peroString.Append(compareText);
            peroString.ToLower();
            return (peroString.Contains(containsText) || compareText.Contains(containsText));
        }

        public static Il2CppSystem.Collections.Generic.List<T> IL_List<T>(params T[] args)
        {
            var list = new Il2CppSystem.Collections.Generic.List<T>();
            if (args != null)
            {
                foreach (var item in args)
                {
                    list.Add(item);
                }
            }
            return list;
        }
        public static T GetResult<T>(this IVariable data)
        {
            return VariableUtils.GetResult<T>(data);
        }

        public static T GetResultOrDefault<T>(this IVariable data, T defaultValue)
        {
            try
            {
                return VariableUtils.GetResult<T>(data) ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static T GetResultViaMarshal<T>(this IVariable data, T defaultValue)
        {
            try
            {
                if (data is null)
                {
                    return defaultValue;
                }
                var obj = VariableUtils.GetResult<Il2CppSystem.Object>(data);

                IntPtr rawValuePtr = IL2CPP.il2cpp_object_unbox(obj.Pointer);
                return Marshal.PtrToStructure<T>(rawValuePtr) ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        public static Highscore ScoresToObjects(this IData data)
        {
            return new Highscore
            {
                Uid = data.fields["uid"].GetResultOrDefault<string>("?"),
                Evaluate = data.fields["evaluate"].GetResultOrDefault<int>(-1),
                Score = data.fields["score"].GetResultOrDefault<int>(-1),
                Combo = data.fields["combo"].GetResultOrDefault<int>(-1),
                Clears = data.fields["clear"].GetResultOrDefault<int>(-1),
                AccuracyStr = data.fields["accuracyStr"].GetResultOrDefault<string>("?"),
                Accuracy = data.fields["accuracy"].GetResultViaMarshal<float>(-1)
            };
        }


        public static List<T> ToSystem<T>(this IList<T> cpList)
        {
            if (cpList == null)
            {
                return null!;
            }
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }
        public static List<T> ToSystem<T>(this Il2CppSystem.Collections.Generic.List<T> cpList)
        {
            if (cpList == null)
            {
                return null!;
            }
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIL2CPP<T>(this IEnumerable<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list)
            {
                result.Add(item);
            }
            return result;
        }

        public static Dictionary<TKey, TValue> ToSystem<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> cpDict) where TKey : notnull
        {
            if (cpDict == null)
            {
                return null!;
            }
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in cpDict)
            {
                dict[item.Key] = item.Value;
            }
            return dict;
        }

        public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> IL_Dict<TKey, TValue>(params (TKey Key, TValue Value)[] args)
        {
            var dict = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
            if (args != null)
            {
                foreach (var item in args)
                {
                    if (dict.ContainsKey(item.Key))
                    {
                        throw new ArgumentException("duplicated key while initalizing dictionary");
                    }
                    dict[item.Key] = item.Value;
                }
            }
            return dict;
        }
    }
}
