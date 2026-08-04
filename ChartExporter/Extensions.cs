using System.Net;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Il2CppAssets.Scripts.PeroTools.Nice.Interface;

// remember to always use this instead of CAM's Ionic.Zip
// (CAM should be an optional dependency)

namespace ChartExporter
{

    public static class Extensions
    {
        public static Il2CppSystem.Int32 BoxInt32(this int value)
        {
            return new Il2CppSystem.Int32
            {
                m_value = value
            };
        }
        public unsafe static Il2CppSystem.Int32 BoxInt32(this float value)
        {
            return new Il2CppSystem.Int32
            {
                m_value = *(int*)&value
            };
        }
        public static Il2CppSystem.Object BoxObject(this int value)
        {
            return value.BoxInt32().BoxIl2CppObject();
        }
        public static Il2CppSystem.Object BoxObject(this float value)
        {
            return value.BoxInt32().BoxIl2CppObject();
        }

        public static T GetResult<T>(this IVariable variable)
        {
            return VariableUtils.GetResult<T>(variable);
        }
        public static void SetResult<T>(this IVariable variable, Il2CppSystem.Object value)
        {
            VariableUtils.SetResult(variable, value);
        }

        public static T GetResultOrDefault<T>(this IVariable variable)
        {
            try
            {
                return VariableUtils.GetResult<T>(variable);
            }
            catch (Exception)
            {
                return default;
            }
        }
        public static List<Il2CppSystem.Object> ToManaged(this Il2CppSystem.Collections.IEnumerable cpList)
        {
            if (cpList is null)
                return null;
            var list = new List<Il2CppSystem.Object>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }
        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.IEnumerable cpList)
        {
            if (cpList is null)
                return null;
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add((T)(object)item);
            }
            return list;
        }
        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.IEnumerable cpList, Func<Il2CppSystem.Object, T> transformer)
        {
            if (transformer is null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }
            if (cpList is null)
                return null;
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(transformer.Invoke(item));
            }
            return list;
        }
        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.Generic.List<T> cpList)
        {
            if (cpList is null)
                return null;
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }
        public static Dictionary<TKey, TValue> ToManaged<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> cpDictionary)
        {
            if (cpDictionary is null)
                return null;
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var entry in cpDictionary)
            {
                dictionary[entry.Key] = entry.Value;
            }
            return dictionary;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIL2CPP<T>(this IEnumerable<T> collection)
        {
            if (collection is null)
                return null;
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in collection)
            {
                result.Add(item);
            }
            return result;
        }
        public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> ToIL2CPP<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary is null)
                return null;
            var result = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
            foreach (var entry in dictionary)
            {
                result[entry.Key] = entry.Value;
            }
            return result;
        }
        public static byte[] ReadFully(this Stream stream, int initialLength = 0)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        public static Stream ToStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static byte[] GetResponseAsBytes(this HttpWebRequest request)
        {
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream stream = response.GetResponseStream();
            return stream.ReadFully();

        }
        public static string GetResponseAsString(this HttpWebRequest request)
        {
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.AsString();
        }
        public static string AsString(this HttpWebResponse response)
        {
            using Stream stream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public static bool LowerContains(this string compareText, string containsText)
        {
            return (compareText ?? "").ToLowerInvariant().Contains((containsText ?? "").ToLowerInvariant());
        }
        public static byte[] GetMD5(this string input)
        {
            return MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(input));
        }
        public static byte[] GetMD5(this IEnumerable<byte> input)
        {
            return MD5.Create().ComputeHash(input.ToArray<byte>());
        }
        public static string MD5ToString(this byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var byte_ in bytes)
            {
                sb.Append(byte_.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string ReadString(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }
        public static T JsonDeserialize<T>(this Stream stream)
        {
            return stream.ReadString().JsonDeserialize<T>();
        }
        public static T JsonDeserialize<T>(this string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
        public static string RemoveFromEnd(this string str, IEnumerable<string> suffixes)
        {
            foreach (string text in suffixes)
            {
                if (str.EndsWith(text))
                {
                    return str.Substring(0, str.Length - text.Length);
                }
            }
            return str;
        }
        public static string RemoveFromStart(this string str, IEnumerable<string> suffixes)
        {
            foreach (string text in suffixes)
            {
                if (str.StartsWith(text))
                {
                    return str.Substring(text.Length);
                }
            }
            return str;
        }

    }
}
