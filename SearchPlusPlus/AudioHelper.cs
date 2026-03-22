using System.IO.Compression;
using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppSystem.Resources;
using IronSearch.Tags;
using NAudio.Vorbis;
using UnityEngine;
using NLayer;
using static MelonLoader.Modules.MelonModule;
using Il2CppPeroTools2.Resources;
using System.Collections.Concurrent;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Il2CppAssets.Scripts.UI.Controls;

namespace IronSearch
{

    public static class AudioHelper
    {
        private const string AudioLengthBackupFile = "chartLengthSearchCache.json";
        private static readonly string AudioLengthBackupFilePath = Path.Join(MelonEnvironment.UserDataDirectory, AudioLengthBackupFile);

        public static readonly ConcurrentDictionary<string, TimeSpan?> CustomCache = new();
        public static ConcurrentDictionary<string, TimeSpan>? VanillaCache { get; private set; }
        public static void ForceBuildCache()
        {
            VanillaCache ??= new();
            var allMusic = GlobalDataBase.s_DbMusicTag.m_AllMusicInfo.ToSystem().Values.Where(x => !x.uid.StartsWith("999_")).ToList();
            var prevRatio = -1M;
            var currentRatio = 0.0M;

            var count = allMusic.Count;
            var countDecimal = (decimal)count;

            for (int i = 0; i < count; i++)
            {
                _ = GetMusicLength(allMusic[i]);
                currentRatio = decimal.Floor((i+1) / countDecimal * 1000)/1000;
                if (currentRatio > prevRatio)
                {
                    var text = $"\rProgress: {currentRatio * 100:F1}%";
                    Console.Write(text.PadRight(Console.WindowWidth-1));
                    prevRatio = currentRatio;
                }
            }
            
        }
        public static TimeSpan? GetMusicLength(MusicInfo musicInfo)
        {
            if (musicInfo.uid is null)
            {
                return null;
            }
            TimeSpan? result;
            if (BuiltIns.EvalCustom(musicInfo))
            {
                if (CustomCache.TryGetValue(musicInfo.uid, out result))
                {
                    return result;
                }
                result = GetCustomLength(musicInfo);
                if (result != null)
                {
                    CustomCache.TryAdd(musicInfo.uid, result);
                }
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

            Dictionary<string,TimeSpan> loadCache = new();

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
            if (musicInfo.music is null)
            {
                return TimeSpan.FromSeconds(-1);
            }
            try
            {
                var ac = ResourcesManager.instance.LoadFromName<AudioClip>(musicInfo.music);
                var length = ac.length;
                ac.UnloadAudioData();
                return TimeSpan.FromSeconds(length);
            }
            catch (Exception)
            {

                MelonLogger.Msg((musicInfo.music ?? "<null>") + ", " + (musicInfo.musicName ?? "<null>") + ", " + (musicInfo.uid ?? "<null>"));
                throw;
            }
        }

        private static TimeSpan? GetCustomLength(MusicInfo musicInfo)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            try
            {
                if (album.IsPackaged)
                {
                    var t = GetFromZip(album.Path);
                    return t;
                }
                else
                {
                    return GetFromDirectory(album.Path);
                }
            }
            catch
            {
                return null;
            }
        }

        private static TimeSpan? GetFromDirectory(string dirPath)
        {
            string oggPath = Path.Combine(dirPath, "music.ogg");
            if (File.Exists(oggPath))
            {
                using var fs = File.OpenRead(oggPath);
                return GetOggLength(fs);
            }

            string mp3Path = Path.Combine(dirPath, "music.mp3");
            if (File.Exists(mp3Path))
            {
                using var fs = File.OpenRead(mp3Path);
                return GetMp3Length(fs);
            }

            return null;
        }
        private static TimeSpan? GetFromZip(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            var oggEntry = archive.GetEntry("music.ogg");
            if (oggEntry != null)
            {
                using var stream = oggEntry.Open();
                using var ms = stream.CopyToMemory();
                return GetOggLength(ms);
            }

            var mp3Entry = archive.GetEntry("music.mp3");
            if (mp3Entry != null)
            {
                using var stream = mp3Entry.Open();
                using var ms = stream.CopyToMemory();
                return GetMp3Length(ms);
            }

            return null;
        }

        private static TimeSpan? GetOggLength(Stream stream)
        {
            try
            {
                using var reader = new VorbisWaveReader(stream);
                return reader.TotalTime;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static TimeSpan? GetMp3Length(Stream stream)
        {
            try
            {
                using var mpeg = new MpegFile(stream);

                return mpeg.Duration;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        internal static void SaveCache()
        {
            if (VanillaCache is null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(VanillaCache.ToDictionary(x => x.Key, x => x.Value.Ticks));

            File.WriteAllText(AudioLengthBackupFilePath, json);
        }
    }
}
