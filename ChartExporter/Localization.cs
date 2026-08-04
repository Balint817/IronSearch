using LocalizeLib;

namespace ChartExporter;

internal static class Localization
{
    internal static LocalString ExportConfirm { get; } = new LocalString()
    {
        English = "Do you want to export this chart?",
        ChineseSimplified = null,
        ChineseTraditional = null,
        Japanese = null,
        Korean = null,
    };
    internal static LocalString ExportSuccess { get; } = new LocalString()
    {
        English = "Export successful",
        ChineseSimplified = null,
        ChineseTraditional = null,
        Japanese = null,
        Korean = null,
    };
    internal static LocalString ExportFailure { get; } = new LocalString()
    {
        English = "Export failed!\nCheck logs for details",
        ChineseSimplified = null,
        ChineseTraditional = null,
        Japanese = null,
        Korean = null,
    };
}
