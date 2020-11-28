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

        }
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

        private void ServerStart(object sender, RoutedEventArgs e)
        {
            Manager.StartGameServer();
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
                    Manager.GameManager.Stop();
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
