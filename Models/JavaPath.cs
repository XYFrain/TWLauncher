namespace TWLauncher.Models {
    /// <summary>
    /// Java 路径条目（用于 ComboBox 绑定和配置存储）。
    /// </summary>
    public class JavaPath {
        /// <summary>Java bin 目录路径（如 "H:\Java\jdk-17\bin\"）。</summary>
        public string Path { get; set; }

        /// <summary>Java 版本号（如 "17.0.19"）。</summary>
        public string Version { get; set; }

        /// <summary>显示文本（如 "Java 17: H:\...\bin\"），用于 ComboBox。</summary>
        public string DisplayName {
            get { return string.Format("Java {0}: {1}", Version, Path); }
        }

        /// <summary></summary>
        public JavaPath(string path, string version) {
            Path = path;
            Version = version;
        }

        /// <summary></summary>
        public override string ToString() => DisplayName;
    }
}
