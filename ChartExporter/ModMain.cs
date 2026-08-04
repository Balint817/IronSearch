using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.Managers;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using Harmony;
using KeybindManager;
using MelonLoader;
using PopupLib;
using Il2CppRewired.UI.ControlMapper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using PopupLib.UI.Windows;
using Newtonsoft.Json;
using PopupLib.UI;

namespace ChartExporter
{

    public class BMS
    {
        public class Note
        {

        }
        //int == measure
        //double == [0,1[, position within measure
        public class DoubleGrid: Dictionary<int, Dictionary<double, List<Note>>>
        {

        }
        //int == measure
        //int == number of lines within measure
        //int == the line within the measure
        public class RatioGrid : Dictionary<int, Dictionary<int, Dictionary<int, List<Note>>>>
        {

        }


    }
    public class ModMain : MelonMod
    {
        internal static PromptWindow ConfirmWindow { get; private set; }
        internal static MessageWindow SuccessWindow { get; private set; }
        internal static MessageWindow FailWindow { get; private set; }
        internal static KeybindListener ExportKeybind { get; private set; }

        internal static JsonSerializerSettings SerializerSettings { get; private set; }

        public override void OnInitializeMelon()
        {
            ConfirmWindow = new(Localization.ExportConfirm);
            SuccessWindow = new(Localization.ExportSuccess);
            FailWindow = new(Localization.ExportFailure);
            var category = MelonPreferences.CreateCategory("ChartExporter");
            ExportKeybind = new KeybindListener(category.CreateEntry<string>("ExportKeybind", "Equals"));
            ExportKeybind.OnPress += OnPressExport;
            SerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            SerializerSettings.Converters.Add(new StageInfoConverter());
            //SerializerSettings.Converters.Add(new TimeNodeOrderConverter());
        }

        internal static Dictionary<string, NoteConfigData> noteDataDict { get; set; }

        private string[] GetFirstAvailableFilenames(params string[] filenames)
        {
            int suffix = 0;
            int length = filenames.Length;
            string[] formatted = new string[length];
            bool any;
            do
            {
                any = false;
                suffix++;
                for (int ii = 0; ii < length; ii++)
                {
                    any |= File.Exists(formatted[ii] =string.Format(filenames[ii], suffix));
                }
            } while (any);
            return formatted;
        }
        private void DumpCurrentJson()
        {
            //var filenames = GetFirstAvailableFilenames("dump{0}.json", "music{0}.wav");
            var filenames = GetFirstAvailableFilenames("dump{0}.json");

            var dumpFilename = filenames[0];
            //var musicFilename = filenames[1];


            var noteDatas = SingletonScriptableObject<NoteDataMananger>.instance.m_NoteDatas.ToManaged();
            foreach (var item in noteDatas.Where(x => x.uid is null))
            {
                MelonLogger.Msg("\n" + JsonConvert.SerializeObject(item, Formatting.Indented));
            }

            noteDataDict = new();
            foreach (var item in noteDatas.Where(x => x.uid is not null))
            {
                if (noteDataDict.ContainsKey(item.uid))
                {
                    continue;
                }
                noteDataDict[item.uid] = item;
            }

            var json = new Dictionary<string, object>();

            //var tnoOriginal = Singleton<StageBattleComponent>.instance.m_TimeNodeOrders;

            //if (tnoOriginal is not null)
            //{
            //    json["timeNodeOrders"] = tnoOriginal;
            //}
            json["stageInfo"] = GlobalDataBase.dbStageInfo.m_StageInfo;

            File.WriteAllText(dumpFilename, JsonConvert.SerializeObject(json, Formatting.Indented, SerializerSettings));
            MelonLogger.Msg(AudioManager.instance.bgm.name);
            //File.WriteAllBytes(musicFilename, SavWav.Save(AudioManager.instance.bgm.clip));
        }

        private void DumpCurrentBMS()
        {
            var stageInfo = GlobalDataBase.dbStageInfo.m_StageInfo;
            var musicDatas = stageInfo.musicDatas.ToManaged().Select(x => new MusicDataWrapper(x)).ToList();
            musicDatas = musicDatas.Where(x => !x.musicData.isLongPressing).ToList();
            musicDatas.Sort(CompareMusicDatas);
        }
        private int CompareMusicDatas(MusicDataWrapper mdw1, MusicDataWrapper mdw2)
        {
            int comparison = mdw1.configData.time.CompareTo(mdw2.configData.time);
            if (comparison != 0)
            {
                return comparison;
            }
            if (mdw1.musicData.isLongPressStart)
            {
                if (mdw2.musicData.isLongPressEnd)
                {
                    return -1;
                }
            }
            else if (mdw2.musicData.isLongPressStart)
            {
                if (mdw1.musicData.isLongPressEnd)
                {
                    return 1;
                }
            }
            if (mdw1.configData.note_uid is null)
            {
                return -1;
            }
            if (mdw2.configData.note_uid is null)
            {
                return -1;
            }
            return 0;
        }
        private void OnPressExport(KeybindListener sender)
        {
            //MelonLogger.Warning("OnPressExport called");
            if (PopupUtils.ActiveMenu is not MenuType.InGame
                //|| !PopupUtils.IsGamePaused
                )
            {
                //MelonLogger.Warning("OnPressExport return 1");
                return;
            }
            DumpCurrentJson();
            //DumpCurrentBMS();
        }
    }
    class MusicDataWrapper
    {
        public readonly MusicData musicData;
        public MusicConfigData configData => musicData.configData;
        public readonly NoteConfigData noteData;
        public MusicDataWrapper(MusicData md)
        {
            musicData = md;
            try
            {
                noteData = SingletonScriptableObject<NoteDataMananger>.instance.GetNoteByUid(md.configData.note_uid);
            }
            catch (System.Exception)
            {
                noteData = md.noteData;
            }
        }
    }
}
