using Il2CppAssets.Scripts.GameCore;
using Il2CppAssets.Scripts.Structs;
using Il2CppGameLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Il2CppPeroPeroGames.GlobalDefines;
using System;
using System.Linq;

namespace ChartExporter
{
    public class StageInfoConverter : JsonConverter
    {
        private readonly Type[] _types;

        public StageInfoConverter()
        {
            _types = new Type[]
            {
                typeof(Il2CppSystem.Decimal),
                typeof(Il2CppSystem.Text.StringBuilder),
                typeof(Il2CppSystem.Collections.Generic.List<MusicData>),
                typeof(Il2CppSystem.Collections.Generic.List<GameDialogArgs>),
                typeof(Il2CppSystem.Collections.Generic.List<SceneEvent>),
                typeof(Il2CppSystem.Collections.Generic.List<string>),
                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Collections.Generic.List<GameDialogArgs>>),
                typeof(IntPtr),
                typeof(StageInfo),
                typeof(MusicData),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t;

            if (value is null)
            {
                t = JToken.FromObject(null);
                t.WriteTo(writer);
                return;
            }

            switch (value)
            {
                case IntPtr ptr:
                    {
                        t = JToken.FromObject(0);
                        t.WriteTo(writer);
                        break;
                    }
                case Il2CppSystem.Decimal d:
                    {
                        t = JToken.FromObject((double)d);
                        t.WriteTo(writer);
                        break;
                    }
                case Il2CppSystem.Text.StringBuilder sb:
                    {
                        t = JToken.FromObject($"<StringBuilder>: {sb.ToString()}");
                        t.WriteTo(writer);
                        break;
                    }
                case MusicData md:
                    {
                        writer.WriteStartObject();//

                        writer.WritePropertyName("objId");
                        writer.WriteValue(md.objId);

                        writer.WritePropertyName("tick");
                        writer.WriteValue((double)md.tick);

                        writer.WritePropertyName("showTick");
                        writer.WriteValue((double)md.showTick);



                        var cd = md.configData;
                        writer.WritePropertyName("configData");
                        writer.WriteStartObject();//

                        writer.WritePropertyName("id");
                        writer.WriteValue(cd.id);

                        writer.WritePropertyName("time");
                        writer.WriteValue((double)cd.time);

                        writer.WritePropertyName("note_uid");
                        writer.WriteValue(cd.note_uid);

                        writer.WritePropertyName("length");
                        writer.WriteValue((double)cd.length);

                        writer.WritePropertyName("blood");
                        writer.WriteValue(cd.blood);

                        writer.WritePropertyName("pathway");
                        writer.WriteValue(cd.pathway);

                        writer.WriteEndObject();//



                        NoteConfigData nd;
                        if (cd?.note_uid is null || !ModMain.noteDataDict.TryGetValue(cd.note_uid, out nd))
                        {
                            nd = md.noteData;
                        }
                        writer.WritePropertyName("noteData");
                        writer.WriteStartObject();//

                        writer.WritePropertyName("ibms_id");
                        writer.WriteValue(nd.ibms_id);

                        writer.WritePropertyName("m_BmsUid");
                        writer.WriteValue(Enum.GetName(typeof(BmsNodeUid), nd.m_BmsUid) ?? $"<unknown:{(int)nd.m_BmsUid}>");

                        writer.WritePropertyName("type");
                        writer.WriteValue(nd.type);

                        writer.WritePropertyName("scene");
                        writer.WriteValue(nd.scene);

                        writer.WritePropertyName("effect");
                        writer.WriteValue(nd.effect);

                        writer.WritePropertyName("boss_action");
                        writer.WriteValue(nd.boss_action);

                        writer.WritePropertyName("sceneChangeNames");
                        serializer.Serialize(writer, nd.sceneChangeNames.ToManaged());

                        writer.WritePropertyName("pathway");
                        writer.WriteValue(nd.pathway);

                        writer.WritePropertyName("speed");
                        writer.WriteValue(nd.speed);

                        writer.WritePropertyName("isShowPlayEffect");
                        writer.WriteValue(nd.isShowPlayEffect);

                        writer.WritePropertyName("isValid");
                        writer.WriteValue(nd.isValid);

                        writer.WriteEndObject();//



                        writer.WritePropertyName("doubleIdx");
                        writer.WriteValue(md.doubleIdx);

                        writer.WritePropertyName("IsDouble");
                        writer.WriteValue(md.IsDouble);

                        writer.WritePropertyName("isMul");
                        writer.WriteValue(md.isMul);

                        writer.WritePropertyName("isAir");
                        writer.WriteValue(md.isAir);

                        writer.WritePropertyName("isLongPressing");
                        writer.WriteValue(md.isLongPressing);

                        writer.WritePropertyName("isLongPressEnd");
                        writer.WriteValue(md.isLongPressEnd);

                        writer.WritePropertyName("longPressPTick");
                        writer.WriteValue((double)md.longPressPTick);

                        writer.WritePropertyName("isLongPressType");
                        writer.WriteValue(md.isLongPressType);

                        writer.WritePropertyName("isLongPressStart");
                        writer.WriteValue(md.isLongPressStart);

                        writer.WritePropertyName("longPressCount");
                        writer.WriteValue(md.longPressCount);

                        writer.WritePropertyName("isBossNote");
                        writer.WriteValue(md.isBossNote);

                        writer.WritePropertyName("IsBossNearAttack");
                        writer.WriteValue(md.IsBossNearAttack);

                        writer.WriteEndObject();//
                        break;
                    }
                case Il2CppSystem.Collections.Generic.List<string> cpList:
                    {
                        serializer.Serialize(writer, cpList.ToManaged());
                        break;
                    }
                case Il2CppSystem.Collections.Generic.List<GameDialogArgs> cpList:
                    {
                        serializer.Serialize(writer, cpList.ToManaged());
                        break;
                    }
                case Il2CppSystem.Collections.Generic.List<SceneEvent> cpList:
                    {
                        serializer.Serialize(writer, cpList.ToManaged());
                        break;
                    }
                case Il2CppSystem.Collections.Generic.List<MusicData> cpList:
                    {
                        serializer.Serialize(writer, cpList.ToManaged());
                        break;
                    }
                case Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Collections.Generic.List<GameDialogArgs>> cpDict:
                    {
                        serializer.Serialize(writer, cpDict.ToManaged().ToDictionary(x => x.Key, x => x.Value.ToManaged()));
                        break;
                    }
                case StageInfo stageInfo:
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName("delay");
                        writer.WriteValue((double)stageInfo.bpm);

                        writer.WritePropertyName("mapName");
                        writer.WriteValue(stageInfo.mapName);

                        writer.WritePropertyName("music");
                        writer.WriteValue(stageInfo.music);

                        writer.WritePropertyName("scene");
                        writer.WriteValue(stageInfo.scene);

                        writer.WritePropertyName("difficulty");
                        writer.WriteValue(stageInfo.difficulty);

                        writer.WritePropertyName("md5");
                        writer.WriteValue(stageInfo.md5);

                        writer.WritePropertyName("bpm");
                        writer.WriteValue(stageInfo.bpm);

                        writer.WritePropertyName("sceneEvents");
                        serializer.Serialize(writer, stageInfo.sceneEvents?.ToManaged());

                        writer.WritePropertyName("dialogEvents");
                        serializer.Serialize(writer, stageInfo.dialogEvents?.ToManaged()?.ToDictionary(x => x.Key, x => x.Value?.ToManaged()));

                        writer.WritePropertyName("musicDatas");
                        serializer.Serialize(writer, stageInfo.musicDatas?.ToManaged());

                        writer.WriteEndObject();
                        break;
                    }
                default:
                    {
                        t = JToken.FromObject(value);
                        t.WriteTo(writer);
                        break;
                    }
            }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
