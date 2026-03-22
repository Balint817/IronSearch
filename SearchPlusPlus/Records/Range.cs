using System.Diagnostics.CodeAnalysis;

namespace IronSearch.Records
{
    public class Range : IComparable
    {
        public MultiRange AsMultiRange()
        {
            if (this.IsReadonly)
            {
                return MultiRange.InvalidRange;
            }
            return new MultiRange(this);
        }
        public static bool operator ==(Range a, Range b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(Range a, Range b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return this.CompareTo(obj) == 0;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public int CompareTo(object? obj)
        {
            var range = (Range?)obj;
            if (range == null)
            {
                return 1;
            }
            if (double.IsNaN(_end))
            {
                if (double.IsNaN(range._end))
                {
                    return 0;
                }
                return -1;
            }
            else if (double.IsNaN(range._end))
            {
                return 1;
            }

            if (range._end == _end)
            {
                return _start.CompareTo(range._start);
            }
            return _end.CompareTo(range._end);
        }
        public static readonly Range InvalidRange = new() { IsReadonly = true };

        private bool _exclusiveStart;
        private bool _exclusiveEnd;

        public bool ExclusiveStart
        {
            get
            {
                return _start is double.NegativeInfinity
                    ? _exclusiveStart = false
                    : _exclusiveStart;
            }
            set
            {
                if (!IsReadonly)
                {
                    _exclusiveStart = value;
                }
            }
        }
        public bool ExclusiveEnd
        {
            get
            {
                return _end is double.PositiveInfinity
                    ? _exclusiveEnd = false
                    : _exclusiveEnd;
            }
            set
            {
                if (!IsReadonly)
                {
                    _exclusiveEnd = value;
                }
            }
        }

        private double _start;
        private double _end;
        public double Start
        {
            get
            {
                return _start;
            }
            set
            {
                if (value > _end)
                {
                    throw new ArgumentOutOfRangeException($"must be less than or equal to max value", nameof(value));
                }
                _start = value;
                ThrowIfNecessary();
            }
        }
        public double End
        {
            get
            {
                return _end;
            }
            set
            {
                if (value < _start)
                {
                    throw new ArgumentOutOfRangeException($"must be greater than or equal to min value", nameof(value));
                }
                _end = value;
                ThrowIfNecessary();
            }
        }

        public bool IsReadonly
        {
            get;
            private set;
        }

        private void ThrowIfNecessary()
        {
            if (double.IsNaN(_start) ^ double.IsNaN(_end))
            {
                throw new ArgumentException($"Either both or neither ends should be NaN.");
            }
            if (_start > _end)
            {
                throw new ArgumentOutOfRangeException($"Min value ({_start}) must be less than or equal to max value ({_end})!");
            }
            if (_start == double.NegativeInfinity)
            {
                ExclusiveStart = false;
            }
            if (_end == double.PositiveInfinity)
            {
                ExclusiveEnd = false;
            }
            if ((ExclusiveStart || ExclusiveEnd) && _start == _end)
            {
                throw new InvalidOperationException("Min and max value cannot be equal when either bound is exclusive!");
            }
            if (_end is double.NegativeInfinity)
            {
                throw new ArgumentException($"End of range cannot be negative infinity; use '{nameof(InvalidRange)}'");
            }
            if (_start is double.PositiveInfinity)
            {
                throw new ArgumentException($"Start of range cannot be positive infinity; use '{nameof(InvalidRange)}'");
            }
        }
        public Range(double start, double end)
        {
            Update(start, end);
        }
        public Range(double value)
        {
            Update(value);
        }

        private Range()
        {
            Update(double.NaN);
        }

        public void Update(double start, double end)
        {
            if (IsReadonly)
            {
                return;
            }
            _start = start;
            _end = end;
            ThrowIfNecessary();
        }
        public void Update(double value)
        {
            if (IsReadonly)
            {
                return;
            }
            _start = _end = value;
            ThrowIfNecessary();
        }
        public bool Contains(double value)
        {
            return (_start < value || !ExclusiveStart && _start == value) && (value < _end || !ExclusiveEnd && _end == value);
        }

        public bool IsOverlap(Range range)
        {
            if (_end < range._start)
            {
                return false;
            }
            if (_start > range._end)
            {
                return false;
            }

            if ((ExclusiveEnd || range.ExclusiveStart) && _end == range._start)
            {
                return false;
            }

            if ((ExclusiveStart || range.ExclusiveEnd) && _start == range._end)
            {
                return false;
            }

            if (double.IsNaN(_end) || double.IsNaN(range._end))
            {
                return false;
            }

            return true;
        }
        public bool TryGetOverlap(Range range, [MaybeNullWhen(false)] out Range overlap)
        {
            overlap = null;
            if (!IsOverlap(range))
            {
                return false;
            }

            double start;
            bool startFlag;
            if (range._start > _start)
            {
                startFlag = range.ExclusiveStart;
                start = range._start;
            }
            else
            {
                startFlag = ExclusiveStart;
                start = _start;
            }


            double end;
            bool endFlag;
            if (range._end < _end)
            {
                endFlag = range.ExclusiveEnd;
                end = range._end;
            }
            else
            {
                endFlag = ExclusiveEnd;
                end = _end;
            }

            overlap = new Range(start, end)
            {
                ExclusiveStart = startFlag,
                ExclusiveEnd = endFlag
            };
            return true;
        }

        public bool TryMergeOffset(Range range, [MaybeNullWhen(false)] out Range merge, double limit = 1)
        {
            if (limit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }
            merge = null;
            if (_end < range._start - limit)
            {
                return false;
            }
            else if (ExclusiveEnd && range.ExclusiveStart && _end == range._start - limit)
            {
                return false;
            }

            if (_start > range._end + limit)
            {
                return false;
            }
            else if (ExclusiveStart && range.ExclusiveEnd && _start == range._end + limit)
            {
                return false;
            }


            double start;
            bool startFlag;
            if (range._start < _start)
            {
                startFlag = range.ExclusiveStart;
                start = range._start;
            }
            else
            {
                startFlag = ExclusiveStart;
                start = _start;
            }


            double end;
            bool endFlag;
            if (range._end > _end)
            {
                endFlag = range.ExclusiveEnd;
                end = range._end;
            }
            else
            {
                endFlag = ExclusiveEnd;
                end = _end;
            }

            merge = new Range(start, end)
            {
                ExclusiveStart = startFlag,
                ExclusiveEnd = endFlag
            };

            return true;
        }
        public bool TryMerge(Range range, [MaybeNullWhen(false)] out Range merge)
        {
            merge = null;
            if (_end < range._start)
            {
                return false;
            }
            else if (ExclusiveEnd && range.ExclusiveStart && _end == range._start)
            {
                return false;
            }

            if (_start > range._end)
            {
                return false;
            }
            else if (ExclusiveStart && range.ExclusiveEnd && _start == range._end)
            {
                return false;
            }


            double start;
            bool startFlag;
            if (range._start < _start)
            {
                startFlag = range.ExclusiveStart;
                start = range._start;
            }
            else
            {
                startFlag = ExclusiveStart;
                start = _start;
            }


            double end;
            bool endFlag;
            if (range._end > _end)
            {
                endFlag = range.ExclusiveEnd;
                end = range._end;
            }
            else
            {
                endFlag = ExclusiveEnd;
                end = _end;
            }

            merge = new Range(start, end)
            {
                ExclusiveStart = startFlag,
                ExclusiveEnd = endFlag
            };


            return true;
        }

        public override string ToString()
        {
            if (double.IsNaN(_start))
            {
                return "?";
            }
            if (_start == _end)
            {
                return _start.ToString();
            }
            if (_start == double.NegativeInfinity)
            {
                if (_end == double.PositiveInfinity)
                {
                    return "*";
                }
                if (ExclusiveEnd)
                {
                    // the pipe sticks to the number being made exclusive (not the other way around)
                    return $"|{_end}-";
                }
                return $"{_end}-";
            }
            if (_end == double.PositiveInfinity)
            {
                if (ExclusiveStart)
                {
                    // the pipe sticks to the number being made exclusive (not the other way around)
                    return $"|{_start}+";
                }
                return $"{_start}+";
            }

            var s = _start + "-" + _end;
            if (ExclusiveStart)
            {
                s = "|" + s;
            }
            if (ExclusiveEnd)
            {
                s = s+"|";
            }

            return s;
        }
    }

}
