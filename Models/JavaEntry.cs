namespace TWLauncher.Models {
    /// <summary>
    /// Java 运行时条目：用于设置面板 ComboBox 绑定。
    /// </summary>
    internal class JavaEntry {
        /// <summary>bin 目录路径。</summary>
        public string Path { get; set; }
        /// <summary>显示文本（版本 + 路径）。</summary>
        public string DisplayName { get; set; }
    }
}
