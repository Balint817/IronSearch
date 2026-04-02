using IronSearch;
using IronSearch.Properties;
using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: MelonIncompatibleAssemblies(new string[] { })]
[assembly: MelonOptionalDependencies(new string[] { "Headquarters", "CustomAlbums", "KeybindManager", "PopupLib" })]
[assembly: MelonAdditionalDependencies(new string[] { })]
//[assembly: MelonAuthorColor()]
//[assembly: MelonColor()]


[assembly: MelonInfo(typeof(ModMain), MelonModInfo.Name, MelonModInfo.Version, MelonModInfo.Author, MelonModInfo.DownloadLink)]

[assembly: AssemblyTitle(MelonModInfo.Name)]
[assembly: AssemblyDescription(MelonModInfo.Description)]
[assembly: AssemblyProduct(MelonModInfo.Name)]
[assembly: AssemblyCopyright(MelonModInfo.Author)]
[assembly: AssemblyTrademark(MelonModInfo.Author)]
[assembly: MelonPriority(MelonModInfo.Priority)]

[assembly: MelonGameVersion(MelonGameInfo.Version)]
[assembly: MelonGame(MelonGameInfo.Developer, MelonGameInfo.Name)]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(MelonModInfo.Version)]
[assembly: AssemblyFileVersion(MelonModInfo.Version)]