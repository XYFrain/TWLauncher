using System;
using System.Collections.Generic;

namespace TWLauncher.Utils {
    /// <summary>
    /// 根据 Library rules 规则判断该库是否适用Windows 
    /// 没有规则的库默认适用于所有平台?    
    /// </summary>
    internal static class RuleUtil {
        public static bool IsAllowedOnWindows(Dictionary<string, object> library) {
            object[] rules;
            // 如果没有 rules，该库默认适用于所有系
            if (!JsonUtil.TryGetArray(library, "rules", out rules))
                return true;

            // 存在 rules 时，默认不允许；后面的匹配规则覆盖前面的结果
            bool allowed = false;
            foreach (object ruleValue in rules) {
                Dictionary<string, object> rule = ruleValue as Dictionary<string, object>;

                string action;
                if (!JsonUtil.TryGetString(rule, "action", out action))
                    continue;

                Dictionary<string, object> os;

                // ?os 条件时，仅处Windows 规则
                if (JsonUtil.TryGetDict(rule, "os", out os)) {
                    string name;

                    if (!JsonUtil.TryGetString(os, "name", out name))
                        continue;

                    if (!string.Equals(name, "windows", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // 没有 os 表示适用于所有系统；os 则已经确认是 Windows
                if (string.Equals(action, "allow", StringComparison.OrdinalIgnoreCase))
                    allowed = true;
                else if (string.Equals(action, "disallow", StringComparison.OrdinalIgnoreCase))
                    allowed = false;
            }
            return allowed;
        }
    }
}
