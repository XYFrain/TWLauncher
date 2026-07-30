namespace TWLauncher.Models
{
    /// <summary>
    /// 单个资源文件的下载信息。
    /// </summary>
    internal class ResourceItem {
        /// <summary>下载地址。</summary>
        public string Url { get; set; }

        /// <summary>本地保存路径。</summary>
        public string Path { get; set; }

        /// <summary>文件的 SHA-1 校验值。</summary>
        public string Sha1 { get; set; }

        /// <summary>文件大小（字节）。</summary>
        public long Size { get; set; }
    }
}
