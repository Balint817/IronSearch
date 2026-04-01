namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        public abstract class Evaluator
        {
            public abstract string EvaluatorName { get; }
            public abstract bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs);
        }
    }
}
