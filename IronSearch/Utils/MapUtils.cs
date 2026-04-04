using CustomAlbums.Data;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Records;
using IronSearch.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSearch.Utils
{
    public static class MapUtils
    {
        public static bool GetAvailableMaps(MusicInfo musicInfo, out HashSet<int> availableMaps)
        {
            return GetAvailableMaps(musicInfo, out availableMaps, out _);
        }

        public static bool GetAvailableMaps(MusicInfo musicInfo, out HashSet<int> availableMaps, out bool isCustom)
        {
            isCustom = BuiltIns.EvalCustom(musicInfo);
            if (isCustom)
            {
                availableMaps = GetCustomMaps(musicInfo);
            }
            else
            {
                availableMaps = new HashSet<int>();
                for (int i = 1; i < 6; i++)
                {
                    var musicDiff = musicInfo.GetMusicLevelStringByDiff(i, false);
                    if (!(string.IsNullOrEmpty(musicDiff) || musicDiff == "0"))
                    {
                        availableMaps.Add(i);
                    }
                }
            }
            if (availableMaps.Count == 0)
            {
                return false;
            }
            return true;
        }

        private static HashSet<int> GetCustomMaps(MusicInfo musicInfo)
        {
            return ((Album)ModMain.uidToCustom[musicInfo.uid]).Sheets.Where(x => !string.IsNullOrEmpty(x.Value.Md5)).Select(x => x.Key).ToHashSet();
        }

        public static bool GetMapDifficulties(MusicInfo musicInfo, out string[] difficulties)
        {
            difficulties = null!;
            if (!GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            difficulties = new string[5];

            if (availableMaps.Contains(1))
            {
                difficulties[0] = musicInfo.difficulty1;
            }

            if (availableMaps.Contains(2))
            {
                difficulties[1] = musicInfo.difficulty2;
            }

            if (availableMaps.Contains(3))
            {
                difficulties[2] = musicInfo.difficulty3;
            }

            if (availableMaps.Contains(4))
            {
                difficulties[3] = musicInfo.difficulty4;
            }

            if (availableMaps.Contains(5))
            {
                difficulties[4] = musicInfo.difficulty5;
            }

            return true;
        }

        public static bool GetMapCallbacks(MusicInfo musicInfo, out int[] difficulties)
        {
            difficulties = null!;
            if (!GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            difficulties = new int[5];

            difficulties[0] = availableMaps.Contains(1) ? musicInfo.callBackDifficulty1 : int.MinValue;
            difficulties[1] = availableMaps.Contains(2) ? musicInfo.callBackDifficulty2 : int.MinValue;
            difficulties[2] = availableMaps.Contains(3) ? musicInfo.callBackDifficulty3 : int.MinValue;
            difficulties[3] = availableMaps.Contains(4) ? musicInfo.callBackDifficulty4 : int.MinValue;
            difficulties[4] = availableMaps.Contains(5) ? musicInfo.callBackDifficulty5 : int.MinValue;

            return true;
        }

        internal static LocalInfo GetLocalSafe(this MusicInfo mi, int language)
        {
            return SearchResults_RefreshPatch.localInfos[mi.uid][language];
        }

        public static object? GetCustomAlbumsSave()
        {
            if (!ModMain.CustomAlbumsLoaded)
            {
                return null;
            }
            return GetCustomAlbumsSaveInternal();
        }

        private static object? GetCustomAlbumsSaveInternal()
        {
            var saveManagerType = AccessTools.TypeByName("CustomAlbums.Managers.SaveManager");
            var saveDataField = AccessTools.Field(saveManagerType, "SaveData");
            return saveDataField.GetValue(null);
        }


        internal static bool TryParseCinemaJson(Album album, bool skipUnpackaged = true)
        {
            string path = album.Path;
            JObject items;

            try
            {
                if (album.IsPackaged)
                {
                    using var fs = File.OpenRead(path);
                    using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
                    var entry = archive.GetEntry("cinema.json");
                    if (entry == null)
                    {
                        return false;
                    }

                    using (var reader = new StreamReader(entry.Open()))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        items = JObject.Load(jsonReader);
                    }

                    string fileName = (string)items["file_name"]!;
                    if (archive.GetEntry(fileName) == null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (skipUnpackaged)
                    {
                        return false;
                    }
                    items = JsonConvert.DeserializeObject<JObject>(
                        File.ReadAllText(Path.Combine(path, "cinema.json"))
                    )!;

                    if (!File.Exists(Path.Combine(path, (string)items["file_name"]!)))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // catch silently
            }

            return false;
        }
    }
}
