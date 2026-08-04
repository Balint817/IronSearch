using Il2CppAssets.Scripts.Database;
using PopupLib;
using PopupLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChartExporter
{
    public static class ExportManager
    {
        public static readonly Regex BPMRegex = new Regex(@"^[0-9,]*\.?[0-9,]+[^0-9.,][0-9,]*\.?[0-9,]+$");
        public static void GetCurrentChartData()
        {
            if (PopupUtils.ActiveMenu is not MenuType.InGame)
            {
                throw new InvalidOperationException();
            }

            var chart = new Chart();
            var musicInfo = BattleHelper.MusicInfo();
            var stageInfo = GlobalDataBase.dbStageInfo.m_StageInfo;

            chart.info.name = musicInfo.name;

            chart.info.author = musicInfo.author;


            if (BPMRegex.IsMatch(musicInfo.bpm))
            {
                chart.info.bpm = musicInfo.bpm;
            }
            else
            {
                //chart.info.bpm =
            }

            



        }
    }
}
