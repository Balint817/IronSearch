namespace IronSearch.Records
{
    using System;
    using System.Collections.Generic;

    public class FuzzyContains
    {
        public readonly string Pattern;
        public readonly bool CaseInsensitive;
        public readonly int MaxDistance;
        private readonly Dictionary<char, ulong> _charMask;

        public FuzzyContains(string containsText, int maxDistance = 1, bool caseInsensitive = true)
        {
            if (string.IsNullOrEmpty(containsText))
            {
                throw new ArgumentException("Pattern cannot be null or empty.", nameof(containsText));
            }

            if (containsText.Length > 63)
            {
                throw new ArgumentException("Pattern too long for this implementation (max 63 chars)");
            }

            CaseInsensitive = caseInsensitive;
            Pattern = containsText;
            if (CaseInsensitive)
            {
                Pattern = containsText.ToLowerInvariant();
            }
            MaxDistance = maxDistance;
            _charMask = BuildCharMask(Pattern);
        }

        public bool Match(string text) => IsMatch(text);

        public bool IsMatch(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (CaseInsensitive)
            {
                text = text.ToLowerInvariant();
            }

            int m = Pattern.Length;

            // R[d] = bit masks for distance d
            ulong[] R = new ulong[MaxDistance + 1];
            for (int i = 0; i <= MaxDistance; i++)
            {
                R[i] = (1UL << i) - 1;
            }

            ulong matchBit = 1UL << (m - 1);

            foreach (char c in text)
            {
                // Unrecognized characters get 0
                ulong charMask = _charMask.TryGetValue(c, out var mask) ? mask : 0UL;

                ulong prev = R[0];

                R[0] = ((R[0] << 1) | 1UL) & charMask;

                for (int d = 1; d <= MaxDistance; d++)
                {
                    ulong temp = R[d];

                    R[d] = (((R[d] << 1) | 1UL) & charMask) // match
                         | ((prev | R[d - 1]) << 1)         // insertion & substitution
                         | prev;                            // deletion

                    // Constantly allow matches to start with errors
                    R[d] |= (1UL << d) - 1;

                    prev = temp;
                }

                if ((R[MaxDistance] & matchBit) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private Dictionary<char, ulong> BuildCharMask(string pattern)
        {
            var mask = new Dictionary<char, ulong>();

            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (!mask.ContainsKey(c))
                {
                    mask[c] = 0UL;
                }
                mask[c] |= (1UL << i);
            }

            return mask;
        }
    }
}