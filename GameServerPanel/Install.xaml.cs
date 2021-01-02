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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace GameServerPanel
{
    /// <summary>
    /// Install.xaml 的交互逻辑
    /// </summary>
    public partial class Install : Window
    {
        public PanelManager Manager;
        public Install(PanelManager Manager)
        {
            this.Manager = Manager;
            InitializeComponent();
            //将全局数据绑定都设置到PanelConfig上
            DataContext = Manager.Config;
            //设置页面基本信息
            Init();
        }
        /// <summary>
        /// 初始化页面显示的信息，可重复调用（即刷新）
        /// </summary>
        public void Init()
        {
            BDSStatus.Content = (Manager.Config.ServerType & GameServerType.BDS) == GameServerType.BDS ? "已安装" : "未安装";
            EZStatus.Content = (Manager.Config.ServerType & GameServerType.EZ) == GameServerType.EZ ? "已安装" : "未安装";
            BDXStatus.Content = (Manager.Config.ServerType & GameServerType.BDX) == GameServerType.BDX ? "已安装" : "未安装";
            BDX_CDK.Text = Manager.GetBDXCDK();
        }
        /// <summary>
        /// 打开选择文件对话框——BDS压缩包
        /// </summary>
        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Button Sender = sender as Button;
            var dialog = new OpenFileDialog();
            //设置允许的文件
            dialog.Filter = PanelManager.SupportFileType;
            dialog.InitialDirectory = BDS_FilePath.Text;
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    SetFilePath(dialog.FileName, Sender.Name);
                }
            }
            catch (ArgumentException)
            {
                //如果BDS_FilePath里的路径格式不对就会报此错误，此时以当前程序目录为默认文件目录
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
                if (dialog.ShowDialog() == true)
                {
                    SetFilePath(dialog.FileName, Sender.Name);
                }
            }
        }
        /// <summary>
        /// 根据选择文件按钮名称来设置按钮对应的文件路径框内容
        /// </summary>
        private void SetFilePath(string FilePath,string ButtonName)
        {
            //每种类型通过switch分开设置
            switch (ButtonName)
            {
                case nameof(BDS_ChooseFile):
                    BDS_FilePath.Text = FilePath;
                    break;
                case nameof(EZ_ChooseFile):
                    EZ_FilePath.Text = FilePath;
                    break;
                case nameof(BDX_ChooseFile):
                    BDX_FilePath.Text = FilePath;
                    break;
                default:
                    throw new ArgumentException("输入的按钮名称不符！", "ButtonName");

            }
        }
        /// <summary>
        /// 根据安装按钮名称获取文件路径
        /// </summary>
        private string GetFilePath(string ButtonName)
        {
            switch (ButtonName)
            {
                case nameof(BDS_Install):
                    return BDS_FilePath.Text;
                case nameof(EZ_Install):
                    return EZ_FilePath.Text;
                case nameof(BDX_Install):
                    return BDX_FilePath.Text;
                default:
                    throw new ArgumentException("输入的按钮名称不符！", nameof(ButtonName));
            }
        }
        /// <summary>
        /// 通过安装按钮名称获取服务端类型
        /// </summary>
        private GameServerType GetType(string ButtonName)
        {
            switch (ButtonName)
            {
                case nameof(BDS_Install):
                    return GameServerType.BDS;
                case nameof(EZ_Install):
                    return GameServerType.EZ;
                case nameof(BDX_Install):
                    return GameServerType.BDX;
                default:
                    throw new ArgumentException("输入的按钮名称不符！", nameof(ButtonName));
            }
        }
        private void Install_Click(object sender, RoutedEventArgs e)
        {
            string SenderName = (sender as Button).Name;
            //创建进度条
            ProgressBar bar = new ProgressBar();
            bar.progressBar.Value = 0;
            bar.progressBar.Maximum = 100;
            bar.RunWhenStart = async () =>
            {
                await Manager.Install(GetType(SenderName), GetFilePath(SenderName), BDS_Backup_Option.IsChecked == true, (s, e) =>
                {
                    //设置进度条内容
                    bar.operationId.Content = $"待完成任务：{e.CurrentOperationId}/{e.TotalNum} 任务完成百分比：{e.CurrentOperation.PercentDone}";
                    bar.progressBar.Value = e.CurrentOperation.PercentDone;
                    bar.FileName.Content = e.CurrentOperation.CurrentFileName;
                });
                //如果安装的是BDX，顺便保存一次激活码
                if (SenderName == nameof(BDX_Install))
                    BDX_CDK_Save_Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //关闭bar
                bar.CanBeClosed = true;
                bar.Close();
            };
            try
            {
                bar.ShowDialog();
                Init();
            }
            catch(Exception E)
            {
                MessageBox.Show(E.Message);
                //关闭bar
                bar.CanBeClosed = true;
                bar.Close();
            }
        }
        /// <summary>
        /// 重置服务端按钮点击
        /// </summary>
        private void ResetServer_Button_Click(object sender, RoutedEventArgs e)
        {
            //创建进度条
            ProgressBar bar = new ProgressBar();
            bar.progressBar.Value = 0;
            bar.progressBar.Maximum = 100;
            bar.RunWhenStart += async () =>
            {
                await Manager.ResetServer(BDS_Backup_Option.IsChecked == true, (s, e) =>
                {
                    //设置进度条内容
                    bar.operationId.Content = $"待完成任务：{e.CurrentOperationId}/{e.TotalNum} 任务完成百分比：{e.CurrentOperation.PercentDone}";
                    bar.progressBar.Value = e.CurrentOperation.PercentDone;
                    bar.FileName.Content = e.CurrentOperation.CurrentFileName;
                });
                bar.CanBeClosed = true;
                bar.Close();
            };
            try
            {
                bar.ShowDialog();
                Init();
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message);
            }
            finally
            {
                //关闭bar
                bar.CanBeClosed = true;
                bar.Close();
            }
        }

        private void BDX_CDK_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.SaveBDXCDK(BDX_CDK.Text);
        }
    }
}
