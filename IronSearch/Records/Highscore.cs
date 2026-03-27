using Il2CppPeroPeroGames.GlobalDefines;

namespace IronSearch.Records
{
    public class Highscore
    {
        public string Uid { get; internal set; } = null!;
        public int Evaluate { get; internal set; }
        public int Score { get; internal set; }
        public int Combo { get; internal set; }
        public int Clear { get; internal set; }
        public string AccuracyStr { get; internal set; } = null!;
        public float Accuracy { get; internal set; }

        internal string _evalStr = null!;
        public string EvaluateStr
        {
            get
            {
                _evalStr ??= EvaluateDefine.EvaluateToRecordStr(Evaluate);
                return _evalStr;
            }
        }
    }

}
