using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GameServerPanel
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 运行CMD命令，不处理运行出错和调用目标卡死的情况
        /// </summary>
        /// <param name="MaxExecuteTime">最多执行时间，以毫秒为单位</param>
        public static async Task RunCMD(string cmd,int MaxExecuteTime = 5000)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            //先把目录切过去再执行命令，虽然我也不知道默认目录会是哪个目录
            //p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            //p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.StandardInput.WriteLine("cd " + PanelManager.RootDic);
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            //MaxExecuteTime毫秒后检查有没有退出，没有则强制退出，该任务异步执行，不等待
            Task t = Task.Run(async () =>
            {
                await Task.Delay(MaxExecuteTime);
                if (p.HasExited != true)
                {
                    p.Kill();
                }
                p.Dispose();
            });
            //等待运行完成，运行完成后结束函数
            await Task.Run(() =>
            {
                p.WaitForExit();
            });
        }
        /// <summary>
        /// 绝对路径转相对路径，基于程序运行目录且只能转程序运行目录下的子目录
        /// </summary>
        public static string GetRelativePath(string absolutePath)
        {
            return absolutePath.Remove(0, PanelManager.RootDic.Length + 1);//由于还有个”\要删所以+1
        }
    }
}
