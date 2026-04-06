using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppFormulaBase;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "GameStart")]
    internal class StageBattleComponent_GameStartPatch
    {
        private static void Prefix(StageBattleComponent __instance)
        {
            string result;
            var uid = GlobalDataBase.dbBattleStage.selectedMusicInfo.uid;
            if (uid.StartsWith("999-"))
            {
                if (!ModMain.CustomAlbumsLoaded)
                {
                    // just making sure...
                    return;
                }
                result = GlobalDataBase.dbStageInfo.md5;
            }
            else
            {
                var selectedDiff = GlobalDataBase.s_DbMusicTag.selectedDiffTglIndex;
                result = $"{uid}_{selectedDiff}";
            }
            ModMain.playIds.Add(result);
        }
    }
}