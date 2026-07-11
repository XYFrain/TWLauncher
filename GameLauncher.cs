using System;
using System.Threading.Tasks;

namespace TWlauncher
{
    internal sealed class GameLauncher
    {
        /// <summary>
        /// 异步启动游戏，避免阻塞界面线程。
        /// </summary>
        public Task LaunchAsync(IProgress<string> progress)
        {
            return Task.Run(() => Launch(progress));
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        private void Launch(IProgress<string> progress)
        {
            throw new NotImplementedException("Minecraft 启动流程尚未实现。");
        }
    }
}
