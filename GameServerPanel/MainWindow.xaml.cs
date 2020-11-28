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

namespace GameServerPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO:运行时禁止修改内容
        /// <summary>
        /// 面板管理器类
        /// </summary>
        public PanelManager Manager = new PanelManager();
        /// <summary>
        /// 面板配置引用
        /// </summary>
        public PanelConfig Config;
        public MainWindow()
        {

            Config = Manager.Config;
            InitializeComponent();
            //设置数据绑定
            this.DataContext = Config;
            this.ServerState.DataContext = Manager.GameManager;
            //监听控制台输出并显示
            Manager.GameManager.OnLog += OnConsoleOutputLog;
            Manager.GameManager.OnError += OnConsoleOutputError;
            //监听崩服重启事件
            Manager.OnAutoRestarting += OnRestarting;
            Manager.OnCancelAutoRestarting += OnCancelAutoRestarting;
            //监听服务端状态变更事件
            Manager.GameManager.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == "GameState")
                {
                    ChangeStartButton(Manager.GameManager.GameState);
                }
            };
        }
        public void ChangeStartButton(GameServerState state)
        {
            switch (state)
            {
                case GameServerState.Off:
                    ServerStartButton.Content = "启动服务端";
                    ServerStartButton.IsEnabled = true;
                    break;
                case GameServerState.Starting:
                    ServerStartButton.Content = "启动中";
                    ServerStartButton.IsEnabled = false;
                    break;
                case GameServerState.Running:
                    ServerStartButton.Content = "关闭服务端";
                    ServerStartButton.IsEnabled = true;
                    break;
                case GameServerState.Stopping:
                    ServerStartButton.Content = "关闭中";
                    ServerStartButton.IsEnabled = false;
                    break;
            }
        }
        #region 崩服重启UI显示
        private void OnRestarting(int t)
        {
            Console.Inlines.Add(new Run($"服务端崩溃，尝试第{t}次重启" + Environment.NewLine) { Foreground = Brushes.Blue });
        }
        private void OnCancelAutoRestarting()
        {
            Console.Inlines.Add(new Run("崩服重启失败多次，已停止崩服重启，请检查问题！" + Environment.NewLine) { Foreground = Brushes.Blue });
        }
        #endregion
        //TODO:支持Log过多时自动删除顶部
        /// <summary>
        /// 游戏服务器输出Log时调用的事件
        /// </summary>
        protected void OnConsoleOutputLog(string message)
        {
            Console.Inlines.Add(new Run(message + Environment.NewLine));
        }
        /// <summary>
        /// 游戏服务器输出Error时调用的事件
        /// </summary>
        protected void OnConsoleOutputError(string message)
        {
            Console.Inlines.Add(new Run(message + Environment.NewLine) { Foreground = Brushes.Red });
        }
        /// <summary>
        /// 启动/关闭服务端按钮按下事件
        /// </summary>
        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Manager.GameManager.GameState)
                {
                    case GameServerState.Off:
                        Manager.StartGameServer();
                        break;
                    case GameServerState.Starting:
                        MessageBox.Show("正在启动中，请勿重复启动！");
                        break;
                    case GameServerState.Running:
                        Manager.StopGameServer();
                        break;
                    case GameServerState.Stopping:
                        MessageBox.Show("正在关闭中，请勿重复关闭！");
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 窗口关闭逻辑
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            //TODO: 窗口关闭时转为后台运行，退出程序时关闭所有服务
            base.OnClosed(e);
            //取消监听控制台输出
            Manager.GameManager.OnError -= OnConsoleOutputError;
            Manager.GameManager.OnLog -= OnConsoleOutputLog;
            //TODO:在支持后台运行后把关闭就kill服务端改成结束进程kill服务端
            Manager.GameManager.Kill();
        }
        /// <summary>
        /// 向服务端发送指令
        /// </summary>
        private void SendCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                //TODO:服务端未启动时的逻辑
                if (InputCommand.Text == "stop")
                {
                    //停止要另外处理
                    Manager.StopGameServer();
                }
                else
                {
                    Manager.GameManager.InputCommand(InputCommand.Text);
                }
                Console.Inlines.Add(new Run(InputCommand.Text + Environment.NewLine) { Foreground = Brushes.Blue });
                InputCommand.Text = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 输入指令框按下键时
        /// </summary>
        private void InputCommandKeyDown(object sender, KeyEventArgs e)
        {
            //如果是回车，发送指令
            if(e.Key == Key.Enter)
            {
                SendCommandButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //清空指令框
                InputCommand.Text = "";
            }
        }
    }
}
