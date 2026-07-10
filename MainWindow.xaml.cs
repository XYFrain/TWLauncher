using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TWlauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 窗口比例
        private const double WindowRatio= 1920.0 / 1080.0;
        // 窗口最大屏幕占比
        private const double maxScale = 0.6;

        public MainWindow()
        {
            // 获取显示器可用区域（剔除任务栏）
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            // 限制窗口最大尺寸
            double maxWindowWidth = screenWidth * maxScale;
            double maxWindowHeight = screenHeight * maxScale;

            double winW, winH;

            // 以宽度优先计算，若高度超出屏幕则改用高度计算
            double tempH = maxWindowWidth / WindowRatio;
            if (tempH <= maxWindowHeight)
            {
                winW = maxWindowWidth;
                winH = tempH;
            }
            else
            {
                winH = maxWindowHeight;
                winW = winH * WindowRatio;
            }

            // 在窗口显示前设定宽高和位置
            this.Width = winW;
            this.Height = winH;
            this.Left = (screenWidth - winW) / 2;
            this.Top = (screenHeight - winH) / 2;

            InitializeComponent();
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void MainClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        private void MainMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 顶部拖动区域 — 按住拖拽移动窗口
        /// </summary>
        private void MainDragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
