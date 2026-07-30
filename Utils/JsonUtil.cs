using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace TWLauncher.Utils {
    /// <summary>
    /// JSON 字典读写工具：读取 JSON 文件，安全读写 Dictionary&lt;string, object&gt;。
    /// </summary>
    internal static class JsonUtil {


        // 读取 json 文件并反序列化
        public static Dictionary<string, object> ReadJson(string path) {
            return new JavaScriptSerializer().DeserializeObject(File.ReadAllText(path))
                as Dictionary<string, object>;
        }




        // 安全读取数组
        public static bool TryGetArray(Dictionary<string, object> source, string key, out object[] value) {
            value = null;
            return source != null
                && source.TryGetValue(key, out object rawValue)
                && (value = rawValue as object[]) != null;
        }

        // 安全读取子对象
        public static bool TryGetDict(Dictionary<string, object> source, string key, out Dictionary<string, object> value) {
            value = null;
            return source != null
                && source.TryGetValue(key, out object rawValue)
                && (value = rawValue as Dictionary<string, object>) != null;
        }

        // 安全读取字符串
        public static bool TryGetString(Dictionary<string, object> source, string key, out string value) {
            value = null;
            return source != null
                && source.TryGetValue(key, out object rawValue)
                && (value = rawValue as string) != null;
        }

        // 安全读取长整数
        public static bool TryGetInt64(Dictionary<string, object> source, string key, out long value) {
            value = 0;
            if (source == null || !source.TryGetValue(key, out object rawValue))
                return false;
            try {
                value = Convert.ToInt64(rawValue);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        // 安全读取字符串列表
        public static bool TryGetStringList(Dictionary<string, object> source, string key, out List<string> value) {
            value = null;
            if (!TryGetArray(source, key, out object[] arr))
                return false;
            var list = new List<string>();
            foreach (object item in arr)
                if (item is string s) list.Add(s);
            value = list;
            return true;
        }
    }
}
