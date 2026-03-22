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

namespace IronSearch
{

    public static class AudioHelper
    {
        private const string AudioLengthBackupFile = "chartLengthSearchCache.json";
        private static readonly string AudioLengthBackupFilePath = Path.Join(MelonEnvironment.UserDataDirectory, AudioLengthBackupFile);

        public static readonly ConcurrentDictionary<string, TimeSpan?> CustomCache = new();
        public static ConcurrentDictionary<string, TimeSpan>? VanillaCache { get; private set; }
        public static TimeSpan? GetMusicLength(MusicInfo musicInfo)
        {
            TimeSpan? result;
            if (BuiltIns.EvalCustom(musicInfo))
            {
                if (CustomCache.TryGetValue(musicInfo.uid, out result))
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
            if (VanillaCache.TryGetValue(musicInfo.uid, out var value))
            {
                VanillaCache.TryAdd(musicInfo.uid, LoadVanillaOne(musicInfo));
                return value;
            }
            return null;
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
            var ac = ResourcesManager.instance.LoadFromName<AudioClip>(musicInfo.music);
            return TimeSpan.FromSeconds(ac.length);
        }

        private static TimeSpan? GetCustomLength(MusicInfo musicInfo)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            try
            {
                if (album.IsPackaged)
                {
                    return GetFromZip(album.Path);
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

            // Prefer OGG
            var oggEntry = archive.GetEntry("music.ogg");
            if (oggEntry != null)
            {
                using var stream = oggEntry.Open();
                return GetOggLength(stream);
            }

            var mp3Entry = archive.GetEntry("music.mp3");
            if (mp3Entry != null)
            {
                using var stream = mp3Entry.Open();
                return GetMp3Length(stream);
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
            catch
            {
                return null;
            }
        }

        private static TimeSpan? GetMp3Length(Stream stream)
        {
            try
            {
                using var mpeg = new MpegFile(stream);

                double seconds = (double)mpeg.Length / mpeg.SampleRate;
                return TimeSpan.FromSeconds(seconds);
            }
            catch
            {
                return null;
            }
        }
    }
}
