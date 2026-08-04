using System.Reflection;

namespace ChartExporter.Properties
{
    [Obfuscation(Exclude = true, ApplyToMembers = true, Feature = "all")]
    internal static class MelonGameInfo
    {
        public const string Name = "MuseDash";

        public const string Developer = "PeroPeroGames";

        public const string Version = null; // IsUniversal => string.IsNullOrEmpty(Version)
    }
}
