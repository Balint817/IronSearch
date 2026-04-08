using CustomAlbums.Data;
using CustomAlbums.Managers;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore;
using Il2CppPeroTools2.Resources;
using IronSearch.Tags;
using IronSearch.Utils;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace IronSearch.Loaders
{

    public static class ChartDataLoader
    {
        private const string AudioLengthBackupFile = "IronSearchChartLengthCache.json";
        private static readonly string AudioLengthBackupFilePath = Path.Join(MelonEnvironment.UserDataDirectory, AudioLengthBackupFile);

        public static readonly ConcurrentDictionary<string, TimeSpan?> CustomCache = new();
        public static readonly ConcurrentDictionary<string, Dictionary<int, object>> BmsCache = new();
        public static ConcurrentDictionary<string, TimeSpan>? VanillaCache { get; private set; }
        private static bool _vanillaCacheUpdated = true;
        public static void ForceBuildVanillaCache()
        {
            if (VanillaCache is not null)
            {
                MelonLogger.Msg($"Vanilla length cache currently contains {VanillaCache.Count} items.");
            }
            VanillaCache ??= new();
            var allMusic = GlobalDataBase.s_DbMusicTag.m_AllMusicInfo.ToSystem().Values.Where(x => x.albumIndex != 999 && !VanillaCache.ContainsKey(x.uid)).ToList();
            MelonLogger.Msg(ConsoleColor.Magenta, $"Need to load {allMusic.Count} items." + (allMusic.Count > 100 ? " This may take a while." : ""));
            _vanillaCacheUpdated = allMusic.Count != 0;
            var prevRatio = -1M;
            var currentRatio = 0.0M;

            var count = allMusic.Count;
            var countDecimal = (decimal)count;

            for (int i = 0; i < count; i++)
            {
                _ = GetMusicLength(allMusic[i]);
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
        public static TimeSpan? GetMusicLength(MusicInfo musicInfo)
        {
            if (musicInfo.uid is null)
            {
                return null;
            }
            if (BuiltIns.EvalCustom(musicInfo))
            {
                if (CustomCache.TryGetValue(musicInfo.uid, out TimeSpan? result))
                {
                    return result;
                }
                result = GetCustomLength(musicInfo);
                CustomCache.TryAdd(musicInfo.uid, result);

                return result;
            }
            if (VanillaCache is null)
            {
                return null;
            }
            if (!VanillaCache.TryGetValue(musicInfo.uid, out var value))
            {
                value = LoadVanillaOne(musicInfo);
                VanillaCache.TryAdd(musicInfo.uid, value);
            }
            return value;
        }

        internal static void LoadVanillaCache()
        {
            string? text = null;
            try
            {
                text = File.ReadAllText(AudioLengthBackupFilePath);
            }
            catch (Exception)
            {
                // catch silently
            }

            Dictionary<string, TimeSpan> loadCache = new();

            if (text is not null)
            {
                try
                {
                    loadCache = JsonConvert.DeserializeObject<Dictionary<string, long>>(text)?.ToDictionary(x => x.Key, x => new TimeSpan(x.Value)) ?? throw new NullReferenceException();
                }
                catch (Exception)
                {
                    // catch silently
                }
            }

            VanillaCache = new();
            foreach (var item in loadCache)
            {
                VanillaCache.TryAdd(item.Key, item.Value);
            }

        }

        private static TimeSpan LoadVanillaOne(MusicInfo musicInfo)
        {
            if (musicInfo.noteJson is null)
            {
                return TimeSpan.FromSeconds(-1);
            }
            try
            {
                if (!MapUtils.GetAvailableMaps(musicInfo, out var availableMaps))
                {
                    return TimeSpan.FromSeconds(-1);
                }

                float maxTime = -1f;
                foreach (var diff in availableMaps)
                {
                    try
                    {
                        var stageInfo = ResourcesManager.instance.LoadFromName<StageInfo>(musicInfo.noteJson + diff);
                        var musicDatas = stageInfo.musicDatas;
                        if (musicDatas == null || musicDatas.Count == 0)
                            continue;

                        for (int i = 0; i < musicDatas.Count; i++)
                        {
                            var tick = Il2CppSystem.Decimal.ToSingle(musicDatas[i].tick);
                            if (tick > maxTime)
                                maxTime = tick;
                        }
                    }
                    catch
                    {

                    }
                }

                return maxTime >= 0f
                    ? TimeSpan.FromSeconds(maxTime)
                    : TimeSpan.FromSeconds(-1);
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.DarkRed, ex);
                MelonLogger.Msg(ConsoleColor.DarkRed, (musicInfo.noteJson ?? "<null>") + ", " + (musicInfo.musicName ?? "<null>") + ", " + (musicInfo.uid ?? "<null>"));
                throw;
            }
        }

        internal static Task CustomCacheTask = null!;
        internal static CancellationTokenSource customCts = new();
        internal static async Task BuildCustomCache(CancellationToken token)
        {
            MelonLogger.Msg($"Started async calculation of custom chart lengths.");
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
                        MelonLogger.Msg($"Remaining time estimate: {(1 - i / count) * sw.Elapsed.TotalSeconds:F1} seconds");
                        return;
                    }
                    var album = allCustoms[i];

                    var length = GetCustomLengthDirect(album.Uid);

                    CustomCache.TryAdd(album.Uid, length);
                }
                sw.Stop();
                MelonLogger.Msg($"Finished customs calculation in {sw.Elapsed.TotalSeconds:F1} seconds.");
            }, token);
        }

        private static TimeSpan? GetCustomLength(MusicInfo musicInfo)
        {
            if (!customCts.IsCancellationRequested)
            {
                customCts.Cancel();
                if ((AlbumManager.LoadedAlbums.Count - CustomCache.Count) > 25)
                {
                    MelonLogger.Msg(ConsoleColor.DarkMagenta, $"Still need to calculate length for {(AlbumManager.LoadedAlbums.Count - CustomCache.Count)}/{AlbumManager.LoadedAlbums.Count} custom charts! Please wait.");
                }
                try
                {
                    CustomCacheTask?.Dispose();
                }
                catch (Exception)
                {

                }
            }
            return GetCustomLengthDirect(musicInfo.uid);
        }

        private static Func<Stream, string, Bms>? _loadBmsDelegate;

        private static Bms LoadBms(Stream stream, string name)
        {
            if (_loadBmsDelegate is null)
            {
                var type = AccessTools.TypeByName("CustomAlbums.BmsLoader");
                var method = AccessTools.Method(type, "Load");
                _loadBmsDelegate = method.CreateDelegate<Func<Stream, string, Bms>>();
            }
            return _loadBmsDelegate(stream, name);
        }

        private static Dictionary<int, object> LoadBmsMaps(string uid, string path, bool isPackaged)
        {
            if (BmsCache.TryGetValue(uid, out var cached))
                return cached;

            var maps = isPackaged
                ? LoadBmsMapsFromZip(path)
                : LoadBmsMapsFromDirectory(path);

            BmsCache.TryAdd(uid, maps);
            return maps;
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

        private static float? GetMaxTimeFromMaps(Dictionary<int, object> maps)
        {
            float maxTime = -1f;
            bool foundAny = false;

            foreach (Bms bms in maps.Values.Cast<Bms>())
            {
                foreach (var note in bms.Notes)
                {
                    if (note is System.Text.Json.Nodes.JsonObject jo
                        && jo.TryGetPropertyValue("time", out var timeNode)
                        && timeNode is System.Text.Json.Nodes.JsonValue jv
                        && jv.TryGetValue<float>(out var time)
                        && time >= maxTime)
                    {
                        maxTime = time;
                    }
                }
                foundAny = true;
            }

            return foundAny ? maxTime : null;
        }

        private static TimeSpan? GetCustomLengthDirect(string uid)
        {
            var album = (Album)ModMain.uidToCustom[uid];
            try
            {
                var maps = LoadBmsMaps(uid, album.Path, album.IsPackaged);
                var maxTime = GetMaxTimeFromMaps(maps);
                return maxTime != null ? TimeSpan.FromSeconds(maxTime.Value) : null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        internal static async void SaveVanillaCache()
        {
            if (VanillaCache is null || !_vanillaCacheUpdated)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(VanillaCache.ToDictionary(x => x.Key, x => x.Value.Ticks));

            await File.WriteAllTextAsync(AudioLengthBackupFilePath, json);
        }
    }
}
