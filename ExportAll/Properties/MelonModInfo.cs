using System.Reflection;

namespace ExportAll.Properties
{
    [Obfuscation(Exclude = true, ApplyToMembers = true, Feature = "all")]
    internal static class MelonModInfo
    {
        public const string Name = "ExportAll";

        public const string Description = "Exports the StageInfo data of all vanilla charts to JSON files.";

        public const string Author = "PBalint817";

        public const string Version = "1.0.0";

        public const string DownloadLink = "";

        //Lower == Greater priority
        public const int Priority = 0;
    }
}
