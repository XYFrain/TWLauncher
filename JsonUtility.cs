using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace TWlauncher
{
    internal static class JsonUtility
    {

        /// <summary>
        /// 读取json文件转为c#对象
        /// </summary>
        public static Dictionary<string, object> ReadJsonObject(string path)
        {
            return new JavaScriptSerializer().DeserializeObject(File.ReadAllText(path))
                as Dictionary<string, object>;
        }
        /// <summary>
        /// 安全地从 JSON 对象中读取一个数组字段
        /// </summary>
        public static bool TryGetJsonArray(Dictionary<string, object> source, string key, out object[] value)
        {
            value = null;
            object rawValue;
            return source != null
                && source.TryGetValue(key, out rawValue)
                && (value = rawValue as object[]) != null;
        }

        public static bool TryGetJsonObject(Dictionary<string, object> source, string key, out Dictionary<string, object> value)
        {
            value = null;
            object rawValue;
            return source != null
                && source.TryGetValue(key, out rawValue)
                && (value = rawValue as Dictionary<string, object>) != null;
        }

        public static bool TryGetJsonString(Dictionary<string, object> source, string key, out string value)
        {
            value = null;
            object rawValue;
            return source != null
                && source.TryGetValue(key, out rawValue)
                && (value = rawValue as string) != null;
        }

        public static bool TryGetJsonInt64(Dictionary<string, object> source, string key, out long value)
        {
            value = 0;
            object rawValue;
            if (source == null || !source.TryGetValue(key, out rawValue))
                return false;

            try
            {
                value = Convert.ToInt64(rawValue);
                return true;
            }
            catch (Exception exception) when (exception is FormatException
                || exception is InvalidCastException
                || exception is OverflowException)
            {
                return false;
            }
        }
    }
}
