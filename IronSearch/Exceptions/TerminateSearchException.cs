namespace IronSearch.Exceptions
{
    [Serializable]
    internal class TerminateSearchException : Exception
    {
        internal readonly bool IsTrue;
        public TerminateSearchException(bool b)
        {
            this.IsTrue = b;
        }
    }
}