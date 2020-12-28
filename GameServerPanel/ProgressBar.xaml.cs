using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GameServerPanel
{
    /// <summary>
    /// 进度条显示窗口
    /// </summary>
    public partial class ProgressBar : Window
    {
        /// <summary>
        /// 是否可以被关闭，设成true则允许关闭
        /// </summary>
        public bool CanBeClosed = false;
        /// <summary>
        /// 在启动时调用，可以将某些启动方法交给进度条窗口执行然后通过ShowDialog显示进度条窗口
        /// </summary>
        public Action RunWhenStart;
        /// <summary>
        /// 是否已经启动，即RunWhenStart是否调用过了
        /// </summary>
        private bool HasStarted = false;
        public ProgressBar()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 避免在进度条跑完前窗口被关掉
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CanBeClosed)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnClosed(e);
            }
        }
        //貌似内容渲染事件是最有可能在窗口启动后调用的……其他几个事件的生命周期都说法不一
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            //确保RunWhenStart不会被多次执行，虽然应该不会
            if (!HasStarted)
            {
                RunWhenStart?.Invoke();
                HasStarted = true;
            }
        }
    }
}
