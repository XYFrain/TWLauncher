namespace TWLauncher.Models {
    /// <summary>
    /// 下载进度数据传输对象，用于 IProgress 跨线程传递进度数据。
    /// </summary>
    internal struct ProgressInfo {
        /// <summary>下载进度百分比（0-100）。</summary>
        public double Percent { get; set; }
        /// <summary>当前阶段文本描述（如"正在下载 3/5 文件 1.2GB/2GB"）。</summary>
        public string StageText { get; set; }
        /// <summary>下载是否活跃中。</summary>
        public bool IsActive { get; set; }
    }
}
