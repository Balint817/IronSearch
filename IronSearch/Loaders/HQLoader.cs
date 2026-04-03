using IronSearch.Records;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace IronSearch.Loaders
{
    public class HQLoader
    {
        private static readonly HttpClient _http = new();
        public static async Task<Dictionary<string, bool>> LoadHQ(CancellationToken cancellationToken = default)
        {
            bool cacheUpdated = !TryLoadBackup(out var result);

            var allCharts = new List<HQChartInfo>();

            int currentPage = 1;
            int totalPages = int.MaxValue;

            while (currentPage <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = $"https://api.mdmc.moe/v3/charts?page={currentPage}&sort=latest&rankedOnly=false&limit=100";

                using var response = await _http.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = JsonConvert.DeserializeObject<HQResponse>(json);

                if (data?.charts != null)
                {
                    allCharts.AddRange(data.charts);
                }

                totalPages = data?.totalPages ?? 0;
                currentPage++;

                // should let this throw in case there's a server error,
                // just so we don't save a half-complete backup
                if (data!.charts.Count == 0)
                {
                    break;
                }

                // break if we pass the latest backup
                if (data.charts.SelectMany(x => x.sheets).Any(x => result.ContainsKey(x.hash)))
                {
                    break;
                }
            }

            foreach (var chart in allCharts)
            {
                foreach (var sheet in chart.sheets)
                {
                    if (!result.TryGetValue(sheet.hash, out var isRankedCache) || isRankedCache != chart.ranked)
                    {
                        cacheUpdated = true;
                    }
                    result[sheet.hash] = chart.ranked;
                }
            }

            if (cacheUpdated)
            {
                await CreateBackup(result);
            }

            return result;
        }

        private const string HQRankingBackupFile = "hqChartsRankInfo.json";
        private static readonly string HQRankingBackupFilePath = Path.Join(MelonEnvironment.UserDataDirectory, HQRankingBackupFile);
        private static async Task CreateBackup(Dictionary<string, bool> result)
        {
            await File.WriteAllTextAsync(HQRankingBackupFilePath, JsonConvert.SerializeObject(result));
        }

        private static bool TryLoadBackup(out Dictionary<string, bool> result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(HQRankingBackupFile))!;
                if (result is null)
                {
                    result = new();
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                result = new();
                return false;
                // catch silently
            }
        }
    }
}
