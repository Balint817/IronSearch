//using Assets.Scripts.GameCore;
//using Assets.Scripts.Structs;
//using FormulaBase;
//using GameLogic;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using PeroPeroGames.GlobalDefines;
//using System;
//using System.Linq;

//namespace ChartExporter
//{
//    public class TimeNodeOrderConverter : JsonConverter
//    {
//        private readonly Type[] _types;

//        public TimeNodeOrderConverter()
//        {
//            _types = new Type[]
//            {
//                typeof(TimeNodeOrder),
//                typeof(Il2CppSystem.Collections.Generic.List<TimeNodeOrder>),
//                typeof(Il2CppSystem.Collections.Generic.Dictionary<int, Il2CppSystem.Collections.Generic.List<TimeNodeOrder>>),
//            };
//        }
//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            JToken t;

//            if (value is null)
//            {
//                t = JToken.FromObject(null);
//                t.WriteTo(writer);
//                return;
//            }

//            switch (value)
//            {
//                case TimeNodeOrder tno:
//                    {
//                        writer.WriteStartObject();

//                        writer.WritePropertyName("idx");
//                        writer.WriteValue(tno.idx);

//                        writer.WritePropertyName("objId");
//                        writer.WriteValue(tno.md.objId);

//                        writer.WritePropertyName("isLongPressType");
//                        writer.WriteValue(tno.isLongPressType);

//                        writer.WritePropertyName("isLongPressStart");
//                        writer.WriteValue(tno.isLongPressStart);

//                        writer.WritePropertyName("isLongPressEnd");
//                        writer.WriteValue(tno.isLongPressEnd);

//                        writer.WritePropertyName("isLongPressing");
//                        writer.WriteValue(tno.isLongPressing);

//                        writer.WritePropertyName("isMulType");
//                        writer.WriteValue(tno.isMulType);

//                        writer.WritePropertyName("isMulStart");
//                        writer.WriteValue(tno.isMulStart);

//                        writer.WritePropertyName("isMuling");
//                        writer.WriteValue(tno.isMuling);

//                        writer.WritePropertyName("isLast");
//                        writer.WriteValue(tno.isLast);

//                        writer.WritePropertyName("isFirst");
//                        writer.WriteValue(tno.isFirst);

//                        writer.WritePropertyName("isAir");
//                        writer.WriteValue(tno.isAir);

//                        writer.WritePropertyName("isFucked");
//                        writer.WriteValue(tno.isFucked);

//                        writer.WritePropertyName("isRight");
//                        writer.WriteValue(tno.isRight);

//                        writer.WritePropertyName("isPerfectNode");
//                        writer.WriteValue(tno.isPerfectNode);

//                        writer.WritePropertyName("enableJump");
//                        writer.WriteValue(tno.enableJump);

//                        writer.WriteEndObject();
//                        break;
//                    }
//                case Il2CppSystem.Collections.Generic.List<TimeNodeOrder> cpList:
//                    {
//                        serializer.Serialize(writer, cpList.ToManaged());
//                        break;
//                    }
//                case Il2CppSystem.Collections.Generic.Dictionary<int, Il2CppSystem.Collections.Generic.List<TimeNodeOrder>> cpDict:
//                    {
//                        serializer.Serialize(writer, cpDict.ToManaged());
//                        break;
//                    }
//            }

//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
//        }

//        public override bool CanRead
//        {
//            get { return false; }
//        }

//        public override bool CanConvert(Type objectType)
//        {
//            return _types.Any(t => t == objectType);
//        }
//    }
//}
