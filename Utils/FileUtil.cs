using System;
using System.IO;
using System.Security.Cryptography;

namespace TWLauncher.Utils {
    /// <summary>
    /// 文件完整性校验工具，支持大小比较和 SHA-1 哈希校验。
    /// </summary>
    internal static class FileUtil {
        /// <summary>
        /// 校验文件大小是否匹配预期值。
        /// </summary>
        public static bool SizeVerify(string filePath, long expectedSize) {
            if (!File.Exists(filePath))
                return false;
            return new FileInfo(filePath).Length == expectedSize;
        }

        /// <summary>
        /// 校验文件 SHA-1 哈希是否匹配预期值。
        /// </summary>
        public static bool HashVerify(string filePath, string expectedHash) {
            if (!File.Exists(filePath))
                return false;

            using (FileStream stream = File.OpenRead(filePath))
            using (SHA1 sha1 = SHA1.Create()) {
                byte[] hash = sha1.ComputeHash(stream);
                string actual = BitConverter.ToString(hash).Replace("-", string.Empty);
                return string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
