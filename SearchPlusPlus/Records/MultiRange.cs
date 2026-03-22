using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace IronSearch.Records
{
    public class MultiRange
    {
        static MultiRange()
        {
            InvalidRange = new() { IsReadOnly = true };
            InvalidRange._ranges.Add(Range.InvalidRange);

            EmptyRange = new() { IsReadOnly = false };

        }
        private List<Range> _ranges = new();

        public static readonly MultiRange InvalidRange;
        public static readonly MultiRange EmptyRange;
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj == (object)this) return true;
            if (obj is not MultiRange mr) return false;

            lock (this._ranges)
            {
                lock (mr._ranges)
                {
                    if (this._ranges.Count != mr._ranges.Count) return false;

                    var count = this._ranges.Count;

                    for (int i = 0; i < count; i++)
                    {
                        if (this._ranges[i] != mr._ranges[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

        }

        public static bool operator ==(MultiRange a, MultiRange b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(MultiRange a, MultiRange b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public MultiRange Invert()
        {
            if (IsReadOnly)
            {
                return this;
            }
            var result = new MultiRange
            {
                _ranges = _ranges
            };
            result.InvertSelf();
            return result;
        }
        public void InvertSelf()
        {
            if (IsReadOnly)
            {
                return;
            }
            _ranges = _ranges.SelectMany(Utils.InvertArray).ToList();
            Resolve();
        }

        public MultiRange Add(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return this;
            }
            var result = new MultiRange
            {
                _ranges = _ranges.ToList()
            };
            result.AddSelf(ranges);
            return result;
        }
        public MultiRange Subtract(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return this;
            }
            var result = new MultiRange
            {
                _ranges = _ranges.ToList()
            };
            result.SubtractSelf(ranges);
            return result;
        }
        public bool IsOverlap(MultiRange other)
        {
            var result = Overlap(other);
            return result != EmptyRange && result != InvalidRange;
        }
        public bool IsOverlap(params Range[] ranges)
        {
            var result = Overlap(ranges);
            return result != EmptyRange && result != InvalidRange;
        }
        public MultiRange Overlap(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return this;
            }
            var result = new MultiRange
            {
                _ranges = _ranges.ToList()
            };
            result.OverlapSelf(ranges);
            return result;
        }
        public void AddSelf(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return;
            }
            _ranges.AddRange(ranges);
            Resolve();
        }
        public void SubtractSelf(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return;
            }
            foreach (var range in ranges)
            {
                this.OverlapSelf(range.InvertArray());
            }
        }
        public void OverlapSelf(params Range[] ranges)
        {
            if (IsReadOnly)
            {
                return;
            }
            var overlaps = new List<Range>();
            foreach (var range1 in _ranges)
            {
                foreach (var range2 in ranges)
                {
                    if (range2.TryGetOverlap(range1, out var overlap))
                    {
                        overlaps.Add(overlap);
                    };
                }
            }
            _ranges = overlaps;
            Resolve();
        }



        public MultiRange Add(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return this;
            }
            return Add(multiRange._ranges.ToArray());
        }
        public MultiRange Subtract(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return this;
            }
            return Subtract(multiRange._ranges.ToArray());
        }
        public MultiRange Overlap(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return this;
            }
            return Overlap(multiRange._ranges.ToArray());
        }
        public void AddSelf(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return;
            }
            AddSelf(multiRange._ranges.ToArray());
        }
        public void SubtractSelf(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return;
            }
            SubtractSelf(multiRange._ranges.ToArray());
        }
        public void OverlapSelf(MultiRange multiRange)
        {
            if (IsReadOnly)
            {
                return;
            }
            OverlapSelf(multiRange._ranges.ToArray());
        }

        //MergeSelf

        //params Range[] ranges
        //MultiRange ranges
        [MemberNotNull(nameof(Ranges))]
        private void Resolve()
        {
            if (IsReadOnly)
            {
                var t = Ranges;
                Ranges = t!;
                return;
            }
            _ranges = _ranges.Where(x => !(x is null || x.Start is double.NaN || x.End is double.NaN)).ToList();
            Ranges = _ranges.AsReadOnly();
            for (int i = 0; i < _ranges.Count - 1; i++)
            {
                var range1 = _ranges[i];
                for (int j = i + 1; j < _ranges.Count; j++)
                {
                    var range2 = _ranges[j];
                    if (range1.TryMerge(range2, out var result))
                    {
                        range1 = range2;
                        _ranges.RemoveAt(j);
                        j = i + 1;
                    };
                }
                _ranges[i] = range1;
            }
            _ranges.Sort();
        }

        internal bool Contains(double n)
        {
            return _ranges.Any(x => x.Contains(n));
        }

        public ReadOnlyCollection<Range> Ranges;
        public bool IsReadOnly { get; private init; }
        public  MultiRange(params Range[] ranges)
        {
            if (ranges is null || ranges.Length == 0)
            {
                _ranges = Array.Empty<Range>().ToList();
                Ranges = _ranges.AsReadOnly();
                return;
            }
            else if (ranges.Length == 1)
            {
                if (ranges[0].Start is not double.NaN)
                {
                    _ranges.Add(ranges[0]);
                }
                Ranges = _ranges.AsReadOnly();
                return;
            }

            _ranges = ranges.ToList();
            Resolve();
        }

        internal MultiRange(Range range)
        {
            if (range is null)
            {
                throw new ArgumentNullException(nameof(range));
            }
            _ranges.Add(range);
            Ranges = _ranges.AsReadOnly();
        }

        private MultiRange()
        {
            Ranges = _ranges.AsReadOnly();
        }
        public override string ToString()
        {
            return "("+string.Join(" ", this._ranges.Select(x => x.ToString()))+")";
        }
    }

}
