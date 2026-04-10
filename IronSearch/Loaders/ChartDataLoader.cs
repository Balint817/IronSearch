using CustomAlbums.Data;
using CustomAlbums.Managers;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore;
using Il2CppAssets.Scripts.GameCore.Managers;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppGameLogic;
using Il2CppPeroTools2.Resources;
using IronSearch.Patches;
using IronSearch.Records;
using IronSearch.Tags;
using IronSearch.Utils;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json.Nodes;

namespace IronSearch.Loaders
{
    public static class ChartDataLoader
    {
        private const string ChartDataCacheFile = "IronSearchChartDataCache.json.gz";
        private static readonly string ChartDataCacheFilePath = Path.Join(MelonEnvironment.UserDataDirectory, ChartDataCacheFile);

        public static readonly ConcurrentDictionary<string, ChartData?> CustomCache = new();
        public static ConcurrentDictionary<string, ChartData>? VanillaCache { get; private set; }
        private static bool _vanillaCacheUpdated = true;
        private static readonly Dictionary<string, object> NoteConfigDatas = new();

        private class CachedNote
        {
            public float Time { get; set; }
            public string Value { get; set; } = "";
            public string Tone { get; set; } = "";
        }

        private class CachedMapData
        {
            public float Bpm { get; set; }
            public string? Md5 { get; set; }
            public List<CachedNote> Notes { get; set; } = new();
        }

        private class CachedChartData
        {
            public long? LengthTicks { get; set; }
            public Dictionary<int, CachedMapData> Maps { get; set; } = new();
        }

        public static ChartData? GetChartData(MusicInfo musicInfo)
        {
            if (musicInfo.uid is null)
            {
                return null;
            }
            if (BuiltIns.EvalCustom(musicInfo))
            {
                if (CustomCache.TryGetValue(musicInfo.uid, out ChartData? result))
                {
                    return result;
                }
                result = GetCustomChartData(musicInfo);
                CustomCache.TryAdd(musicInfo.uid, result);

                return result;
            }
            if (VanillaCache is null)
            {
                return null;
            }
            if (!VanillaCache.TryGetValue(musicInfo.uid, out var value))
            {
                return null;
            }
            return value;
        }

        public static TimeSpan? GetMusicLength(MusicInfo musicInfo)
        {
            return GetChartData(musicInfo)?.MaxLength;
        }

        static bool isFirstVanillaBuild = true;
        public static void ForceBuildVanillaCache()
        {
            if (!isFirstVanillaBuild)
            {
                return;
            }
            isFirstVanillaBuild = false;
            if (VanillaCache is not null)
            {
                MelonLogger.Msg($"Vanilla chart data cache currently contains {VanillaCache.Count} items.");
            }
            if (VanillaCache?.IsEmpty ?? true)
            {
                MelonLogger.Msg(ConsoleColor.Magenta, "Need to re-build chart data cache!");
            }
            VanillaCache ??= new();
            var allMusic = GlobalDataBase.s_DbMusicTag.m_AllMusicInfo.ToSystem().Values.Where(x => x.albumIndex != 999 && x.noteJson is not null && !VanillaCache.ContainsKey(x.uid)).ToList();
            MelonLogger.Msg(ConsoleColor.Magenta, $"Need to load {allMusic.Count} items." + (allMusic.Count > 100 ? " This may take a while." : ""));
            _vanillaCacheUpdated = allMusic.Count != 0;
            var prevRatio = -1M;
            var currentRatio = 0.0M;

            var count = allMusic.Count;
            var countDecimal = (decimal)count;

            InitNoteDatas();

            for (int i = 0; i < count; i++)
            {
                var musicInfo = allMusic[i];
                var value = LoadVanillaOne(musicInfo);
                if (value is null)
                    continue;

                VanillaCache.TryAdd(musicInfo.uid, value);

                currentRatio = decimal.Floor((i + 1) / countDecimal * 1000) / 1000;
                if (currentRatio > prevRatio)
                {
                    var text = $"\rProgress: {currentRatio * 100:F1}%";
                    Console.Write(text.PadRight(Console.WindowWidth - 1));
                    prevRatio = currentRatio;
                }
            }
            SaveVanillaCache();
        }

        internal static void LoadVanillaCache()
        {
            Dictionary<string, CachedChartData>? loadCache = null;
            try
            {
                using var fileStream = File.OpenRead(ChartDataCacheFilePath);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                var json = reader.ReadToEnd();
                loadCache = JsonConvert.DeserializeObject<Dictionary<string, CachedChartData>>(json);
            }
            catch (Exception)
            {
                // catch silently
            }

            VanillaCache = new();
            if (loadCache is null)
                return;

            foreach (var (uid, cached) in loadCache)
            {
                var maps = new Dictionary<int, MapData>();
                foreach (var (index, cachedMap) in cached.Maps)
                {
                    var notes = cachedMap.Notes.Select(n => new NoteInfo(n.Time, n.Value, n.Tone)).ToList();
                    maps[index] = new MapData(notes, cachedMap.Bpm, cachedMap.Md5);
                }
                var length = cached.LengthTicks.HasValue ? new TimeSpan(cached.LengthTicks.Value) : (TimeSpan?)null;
                VanillaCache.TryAdd(uid, new ChartData(maps, length));
            }
        }

        private static ChartData LoadVanillaOne(MusicInfo musicInfo)
        {
            try
            {
                if (!MapUtils.GetAvailableMaps(musicInfo, out var availableMaps))
                {
                    throw new InvalidOperationException("vanilla song with no maps???");
                }

                var maps = new Dictionary<int, MapData>();
                float maxTime = -1f;
                int stageInfoCount = 0;

                foreach (var diff in availableMaps)
                {
                    try
                    {
                        var stageInfo = ResourcesManager.instance.LoadFromName<StageInfo>(musicInfo.noteJson + diff);
                        if (stageInfo?.musicDatas == null || stageInfo.musicDatas.Count == 0)
                            continue;

                        stageInfoCount++;
                        var notes = new List<NoteInfo>();
                        var musicDatas = stageInfo.musicDatas;
                        for (int j = 0; j < musicDatas.Count; j++)
                        {
                            var md = musicDatas[j];
                            var time = Il2CppSystem.Decimal.ToSingle(md.tick);
                            var noteUid = md.configData?.note_uid;

                            if (noteUid is null || !NoteConfigDatas.TryGetValue(noteUid, out var configObj))
                                continue;

                            var configData = (NoteConfigData)configObj;

                            // skip hold ticks
                            if (configData.ibms_id == "0F" && md.isLongPressing)
                                continue;
                            

                            var ibmsId = configData.ibms_id;
                            var pathway = configData.pathway == 1 ? "13" : "14";
                            notes.Add(new NoteInfo(time, ibmsId, pathway));

                            if (time > maxTime)
                                maxTime = time;
                        }
                        //var dialogEvents = new Dictionary<string, List<DialogEventInfo>>();
                        //if (stageInfo.dialogEvents != null && stageInfo.dialogEvents.Count != 0)
                        //{
                        //    foreach (var kv in stageInfo.dialogEvents)
                        //    {
                        //        if (kv.Value == null || kv.Value.Count == 0)
                        //        {
                        //            continue;
                        //        }
                        //        var list = new List<DialogEventInfo>();
                        //        foreach (var de in kv.Value)
                        //        {
                        //            list.Add(new DialogEventInfo
                        //            {
                        //                Time = Il2CppSystem.Decimal.ToSingle(de.time),
                        //                Text = de.text
                        //            });
                        //        }
                        //        dialogEvents[kv.Key] = list;
                        //    }
                        //}

                        maps[diff] = new MapData(notes, stageInfo.bpm, stageInfo.md5);
                    }
                    catch
                    {

                    }
                }

                if (stageInfoCount == 0)
                {
                    throw new InvalidOperationException("vanilla song with no stageInfos???");
                }

                return new ChartData(maps, maxTime >= 0f ? TimeSpan.FromSeconds(maxTime) : null);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                MelonLogger.Msg(ConsoleColor.DarkRed, ex);
                MelonLogger.Msg(ConsoleColor.DarkRed, (musicInfo.noteJson ?? "<null>") + ", " + (musicInfo.musicName ?? "<null>") + ", " + (musicInfo.uid ?? "<null>"));
                return null!;
            }
        }

        internal static Task CustomCacheTask = null!;
        internal static CancellationTokenSource customCts = new();
        internal static async Task BuildCustomCache(CancellationToken token)
        {
            MelonLogger.Msg($"Started async calculation of custom chart data.");
            await Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                var allCustoms = AlbumManager.LoadedAlbums.Values.ToList();
                var count = allCustoms.Count;
                for (int i = 0; i < count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        sw.Stop();
                        MelonLogger.Msg($"Custom async calculation cancelled after {sw.Elapsed.TotalSeconds:F1} seconds. Progress: {i}/{count}");
                        MelonLogger.Msg($"Remaining time estimate: {(1 - i / (double)count) * sw.Elapsed.TotalSeconds:F1} seconds");
                        return;
                    }
                    var album = allCustoms[i];

                    var chartData = GetCustomChartDataDirect(album.Uid);

                    CustomCache.TryAdd(album.Uid, chartData);
                }
                sw.Stop();
                MelonLogger.Msg($"Finished customs calculation in {sw.Elapsed.TotalSeconds:F1} seconds.");
            }, token);
        }

        private static ChartData? GetCustomChartData(MusicInfo musicInfo)
        {
            if (!customCts.IsCancellationRequested)
            {
                customCts.Cancel();
                if ((AlbumManager.LoadedAlbums.Count - CustomCache.Count) > 25)
                {
                    MelonLogger.Msg(ConsoleColor.DarkMagenta, $"Still need to calculate chart data for {(AlbumManager.LoadedAlbums.Count - CustomCache.Count)}/{AlbumManager.LoadedAlbums.Count} custom charts! Please wait.");
                }
                try
                {
                    CustomCacheTask?.Dispose();
                }
                catch (Exception)
                {

                }
            }
            return GetCustomChartDataDirect(musicInfo.uid);
        }

        private static Delegate? _loadBmsDelegate;

        private static object LoadBms(Stream stream, string name)
        {
            if (_loadBmsDelegate is null)
            {
                var type = AccessTools.TypeByName("CustomAlbums.BmsLoader");
                var method = AccessTools.Method(type, "Load");
                _loadBmsDelegate = method.CreateDelegate<Func<Stream, string, Bms>>();
            }
            return ((Func<Stream, string, Bms>)_loadBmsDelegate)(stream, name);
        }

        private static readonly Dictionary<int, object> EmptyBmsMaps = new();

        private static Dictionary<int, object> LoadBmsMaps(string path, bool isPackaged)
        {
            if (!BmsLoader_MathPatch.IsPatched)
                return EmptyBmsMaps;

            return isPackaged
                ? LoadBmsMapsFromZip(path)
                : LoadBmsMapsFromDirectory(path);
        }

        private static Dictionary<int, object> LoadBmsMapsFromZip(string zipPath)
        {
            var maps = new Dictionary<int, object>();
            using var archive = ZipFile.OpenRead(zipPath);

            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    var entry = archive.GetEntry($"map{i}.bms");
                    if (entry == null) continue;

                    using var stream = entry.Open();
                    using var ms = stream.CopyToMemory();
                    maps[i] = LoadBms(ms, $"map{i}.bms");
                }
                catch
                {
                    // silent ignore
                }
            }

            return maps;
        }

        private static Dictionary<int, object> LoadBmsMapsFromDirectory(string dirPath)
        {
            var maps = new Dictionary<int, object>();

            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    var bmsPath = Path.Combine(dirPath, $"map{i}.bms");
                    if (!File.Exists(bmsPath)) continue;

                    using var fs = File.OpenRead(bmsPath);
                    maps[i] = LoadBms(fs, $"map{i}.bms");
                }
                catch
                {
                    // silent ignore
                }
            }

            return maps;
        }

        private static ChartData? GetCustomChartDataDirect(string uid)
        {
            var album = (Album)ModMain.uidToCustom[uid];
            try
            {
                var bmsMaps = LoadBmsMaps(album.Path, album.IsPackaged);
                if (bmsMaps.Count == 0)
                    return null;

                var maps = new Dictionary<int, MapData>();
                float maxTime = -1f;

                foreach (var (index, bmsObj) in bmsMaps)
                {
                    var bms = (Bms)bmsObj;
                    var notes = new List<NoteInfo>();
                    foreach (var note in bms.Notes)
                    {
                        if (note is not JsonObject jo)
                            continue;

                        if (!jo.TryGetPropertyValue("time", out var timeNode)
                            || timeNode is not JsonValue jv
                            || !jv.TryGetValue<float>(out var time))
                            continue;

                        var value = jo["value"]?.GetValue<string>() ?? "";
                        var tone = jo["tone"]?.GetValue<string>() ?? "";
                        notes.Add(new NoteInfo(time, value, tone));

                        if (time > maxTime)
                            maxTime = time;
                    }
                    maps[index] = new MapData(notes, bms.Bpm, bms.Md5);
                }

                return new ChartData(maps, maxTime >= 0f ? TimeSpan.FromSeconds(maxTime) : null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        internal static void InitNoteDatas()
        {
            var noteDatas = SingletonScriptableObject<NoteDataMananger>.instance.m_NoteDatas.ToSystem();
            foreach (var item in noteDatas)
            {
                if (item == null || item.uid == null)
                    continue;
                NoteConfigDatas.TryAdd(item.uid, item);
            }
        }

        internal static async void SaveVanillaCache()
        {
            if (VanillaCache is null || !_vanillaCacheUpdated)
            {
                return;
            }

            var cacheDto = VanillaCache.ToDictionary(
                x => x.Key,
                x => new CachedChartData
                {
                    LengthTicks = x.Value.MaxLength?.Ticks,
                    Maps = x.Value.Maps.ToDictionary(
                        m => m.Key,
                        m => new CachedMapData
                        {
                            Bpm = m.Value.Bpm,
                            Md5 = m.Value.Md5,
                            Notes = m.Value.Notes.Select(n => new CachedNote
                            {
                                Time = n.Time,
                                Value = n.Value,
                                Tone = n.Tone
                            }).ToList()
                        })
                });

            var json = JsonConvert.SerializeObject(cacheDto);

            await using var fileStream = File.Create(ChartDataCacheFilePath);
            await using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            await using var writer = new StreamWriter(gzipStream);
            await writer.WriteAsync(json);
        }
    }
}
