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

            FullRange = new() { IsReadOnly = true };
            FullRange._ranges.Add(Range.FullRange);

            EmptyRange = new() { IsReadOnly = true };

        }
        private List<Range> _ranges = new();

        public static readonly MultiRange InvalidRange;
        public static readonly MultiRange EmptyRange;
        public static readonly MultiRange FullRange;
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not MultiRange mr) return false;

            if (this._ranges.Count != mr._ranges.Count) return false;

            for (int i = 0; i < this._ranges.Count; i++)
            {
                if (this._ranges[i] != mr._ranges[i]) return false;
            }
            return true;
        }

        public static bool operator ==(MultiRange? a, MultiRange? b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(MultiRange? a, MultiRange? b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var r in _ranges)
            {
                hash.Add(r);
            }
            return hash.ToHashCode();
        }
        public MultiRange Invert()
        {
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
            // REMOVED: if (IsReadOnly) return this; 
            var result = new MultiRange
            {
                _ranges = _ranges.ToList()
            };
            result.AddSelf(ranges); // result is not ReadOnly, so AddSelf will succeed
            return result;
        }
        public MultiRange Subtract(params Range[] ranges)
        {
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
                Ranges ??= _ranges.AsReadOnly();
                return;
            }

            _ranges = _ranges.Where(x => x is not null).ToList();

            if (_ranges.Count > 1)
            {
                _ranges.Sort(); // Sort FIRST
                for (int i = 0; i < _ranges.Count - 1; i++)
                {
                    if (_ranges[i].TryMerge(_ranges[i + 1], out var merged))
                    {
                        _ranges[i] = merged;
                        _ranges.RemoveAt(i + 1);
                        i--; // Step back to check the newly merged range against the next one
                    }
                }
            }

            Ranges = _ranges.AsReadOnly();
        }

        internal bool Contains(double n)
        {
            return _ranges.Any(x => x.Contains(n));
        }

        public ReadOnlyCollection<Range> Ranges;
        public bool IsReadOnly { get; private init; }

        public MultiRange(params Range[] ranges)
        {
            if (ranges is null || ranges.Length == 0)
            {
                _ranges = Array.Empty<Range>().ToList();
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
