using Il2CppAssets.Scripts.Database;
using System.Collections.ObjectModel;

namespace IronSearch.Records
{
    public class SorterInfo : IComparable<SorterInfo>
    {
        private static ulong _idTracker = 0;

        internal readonly ulong _id = _idTracker++;

        private List<dynamic> _comparers;
        public ReadOnlyCollection<dynamic> Comparers { get; private init; }
        public int Priority { get; }
        public bool Reverse { get; }
        public SorterInfo(IEnumerable<dynamic> comparers, bool reverse = false, int priority = 0)
        {
            _comparers = comparers.ToList();
            if (_comparers.Count == 0)
            {
                throw new ArgumentException("expected at least one comparer, got none");
            }
            foreach (var item in _comparers)
            {
                if (!Utils.IsCallable(item))
                {
                    throw new ArgumentException("one of the argument was not a comparer");
                }
                if (item is Delegate d)
                {
                    var parameters = d.Method.GetParameters();
                    if (parameters.Length != 2)
                    {
                        throw new ArgumentException("invalid comparer delegate");
                    }
                    foreach (var p in parameters)
                    {
                        if (!typeof(MusicInfo).IsAssignableTo(p.ParameterType))
                        {
                            throw new ArgumentException("invalid comparer delegate");
                        }
                    }
                }
                else
                {
                    var argCount = Utils.GetPythonArgCount(item);
                    if (argCount != 2)
                    {
                        throw new ArgumentException($"invalid comparer function, expected 2 arguments, got {argCount}");
                    }
                }
            }
            Reverse = reverse;
            Comparers = _comparers.AsReadOnly();
            Priority = priority;
        }

        public int CompareTo(SorterInfo? other)
        {
            if (other is null)
            {
                return 1; // this instance is greater than null
            }

            var t = Priority.CompareTo(other.Priority);
            if (t != 0)
            {
                return t;
            }

            return _id.CompareTo(other._id);
        }

    }
}
