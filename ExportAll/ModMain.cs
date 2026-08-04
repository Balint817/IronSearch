using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.Managers;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppGameLogic;
using Il2CppPeroTools2.Resources;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExportAll
{
    public class ModMain : MelonMod
    {
        internal static Dictionary<string, NoteConfigData> NoteDataDict { get; private set; } = new();

        private static JsonSerializerSettings _serializerSettings = null!;
        private static readonly string OutputDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "ExportAll");

        public override void OnInitializeMelon()
        {
            _serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            _serializerSettings.Converters.Add(new StageInfoConverter());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "UISystem_PC")
            {
                ExportAllCharts();
            }
        }

        private static void ExportAllCharts()
        {
            MelonLogger.Msg("ExportAll: starting export of all vanilla charts...");

            Directory.CreateDirectory(OutputDirectory);

            // Build note data dictionary
            NoteDataDict = new();
            var noteDatas = SingletonScriptableObject<NoteDataMananger>.instance.m_NoteDatas;
            foreach (var nd in noteDatas)
            {
                if (nd.uid is not null)
                    NoteDataDict.TryAdd(nd.uid, nd);
            }

            var allMusic = new List<MusicInfo>();
            foreach (var mi in GlobalDataBase.s_DbMusicTag.m_AllMusicInfo.Values)
            {
                if (mi.albumIndex != 999 && mi.noteJson is not null)
                    allMusic.Add(mi);
            }

            MelonLogger.Msg($"ExportAll: found {allMusic.Count} vanilla music entries.");

            int exported = 0;
            int failed = 0;

            foreach (var musicInfo in allMusic)
            {
                for (int diff = 1; diff <= 5; diff++)
                {
                    var musicDiff = musicInfo.GetMusicLevelStringByDiff(diff, false);
                    if (string.IsNullOrEmpty(musicDiff) || musicDiff == "0")
                    {
                        continue;
                    }

                    try
                    {
                        var stageInfo = ResourcesManager.instance.LoadFromName<Il2CppAssets.Scripts.GameCore.StageInfo>(musicInfo.noteJson + diff);
                        if (stageInfo is null)
                        {
                            MelonLogger.Warning($"ExportAll: null StageInfo for {musicInfo.uid} diff {diff}, skipping.");
                            failed++;
                            continue;
                        }

                        StageInfoConverter.currentDiffHack = musicDiff;
                        var json = JsonConvert.SerializeObject(stageInfo, Formatting.Indented, _serializerSettings);
                        var filename = Path.Combine(OutputDirectory, $"{musicInfo.uid}_{diff}.json");
                        File.WriteAllText(filename, json);
                        exported++;
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"ExportAll: failed to export {musicInfo.uid} diff {diff}: {ex.Message}");
                        failed++;
                    }
                }
            }

            MelonLogger.Msg($"ExportAll: done. Exported {exported} charts, {failed} failures.");
            MelonLogger.Msg($"ExportAll: files saved to {OutputDirectory}");
        }
    }
}
