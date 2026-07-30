using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TWLauncher.Utils {
    /// <summary>
    /// HTTP 网络请求工具，封装了 HttpClient，提供文本下载和流式文件下载。
    /// </summary>
    internal static class HttpUtil {
        private static readonly HttpClient _httpClient;
        static HttpUtil() {
            System.Net.ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
        }

        /// <summary>
        /// GET 请求，将响应体作为字符串返回。适用于下载小体积 JSON 文本。
        /// </summary>
        public static async Task<string> GetString(string url, CancellationToken ct) {
            for (int retry = 0; retry < 3; retry++) {
                ct.ThrowIfCancellationRequested();

                try {
                    using (HttpResponseMessage response = await _httpClient.GetAsync(url, ct)) {
                        if (!response.IsSuccessStatusCode)
                            throw new HttpRequestException(string.Format("{0} → {1} {2}", url, (int)response.StatusCode, response.ReasonPhrase));
                        return await response.Content.ReadAsStringAsync();
                    }
                } catch (HttpRequestException) {
                    if (retry >= 2) throw;
                } catch (OperationCanceledException) {
                    throw;
                }

                await Task.Delay(200, ct);
            }

            throw new HttpRequestException(string.Format("多次重试后仍然失败: {0}", url));
        }

        /// <summary>
        /// 流式下载文件到本地磁盘，不一次性加载到内存。自动创建目标目录，每次读取 80KB 并支持取消。
        /// </summary>
        public static async Task DownloadToFile(string url, string filePath, CancellationToken ct) {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            for (int retry = 0; retry < 3; retry++) {
                ct.ThrowIfCancellationRequested();

                try {
                    using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)) {
                        if (!response.IsSuccessStatusCode)
                            throw new HttpRequestException(string.Format("{0} → {1} {2}", url, (int)response.StatusCode, response.ReasonPhrase));
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                        using (FileStream fileStream = File.Create(filePath)) {
                            await responseStream.CopyToAsync(fileStream, 81920, ct);
                        }
                    }
                    return;
                } catch (HttpRequestException) {
                    if (retry >= 2) throw;
                } catch (OperationCanceledException) {
                    throw;
                }

                await Task.Delay(200, ct);
            }
        }
    }
}
