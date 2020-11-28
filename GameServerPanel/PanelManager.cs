using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GameServerPanel
{
    /// <summary>
    /// 面板核心控制类
    /// </summary>
    public class PanelManager
    {
        /// <summary>
        /// 根目录地址
        /// </summary>
        public static readonly string RootDic;
        /// <summary>
        /// 是否是第一次运行
        /// </summary>
        public bool IsFirstRun = false;
        /// <summary>
        /// 面板配置
        /// </summary>
        public PanelConfig Config = null;
        /// <summary>
        /// 游戏进程控制器
        /// </summary>
        public GameConsoleManager GameManager = null;
        #region 事件列表
        /// <summary>
        /// 在自动重启时调用
        /// </summary>
        public Action<int> OnAutoRestarting;
        /// <summary>
        /// 因自动重启次数过多而放弃自动重启时调用
        /// </summary>
        public Action OnCancelAutoRestarting;
        #endregion
        static PanelManager()
        {
            RootDic = Directory.GetCurrentDirectory();
        }
        public PanelManager()
        {
            //读取配置文件
            Config = PanelConfig.ReadFromFile();
            //检测是不是第一次运行
            if(Config == null)
            {
                IsFirstRun = true;
                FirstRun();
            }
            //初始化GameManager
            GameManager = new GameConsoleManager(Config.ServerType);
            AutoRestartInit();
            //TODO:初始化WebSocket和链接CoreManager
        }
        /// <summary>
        /// 首次运行时执行
        /// </summary>
        public void FirstRun()
        {
            //TODO: 首次执行初始化
            //初始化配置文件
            Config = new PanelConfig();
            Config.SetToFile();
            //创建文件
        }
        /// <summary>
        /// 启动所有模块
        /// </summary>
        public void StartAll()
        {
            ConnectCorePanel();
            StartGameServer();
            StartWebSocket();
        }
        /// <summary>
        /// 启动WebSocket服务
        /// </summary>
        public void StartWebSocket()
        {
            //TODO: WebSocket服务启动逻辑
        }
        /// <summary>
        /// 启动游戏服务器
        /// </summary>
        public void StartGameServer()
        {
            //TODO: 启动游戏服务器逻辑
            GameManager.Start();
        }
        /// <summary>
        /// 关闭游戏服务器
        /// </summary>
        public void StopGameServer()
        {
            //TODO: 关闭游戏服务器逻辑
            GameManager.Stop();
        }
        #region 自动重启

        /// <summary>
        /// 自动重启次数
        /// </summary>
        private int autoRestartTimes;
        /// <summary>
        /// 最大自动重启次数
        /// </summary>
        private const int MaxAutoRestartTimes = 5;
        /// <summary>
        /// 崩服自动重启功能初始化
        /// </summary>
        private void AutoRestartInit()
        {
            //注册事件
            GameManager.OnCrashWhenStarting += CrashAutoRestart;
            GameManager.OnCrashWhenRunning += CrashAutoRestart;
        }
        /// <summary>
        /// 崩服自动重启逻辑
        /// </summary>
        private async void CrashAutoRestart()
        {
            //判断是否要自动重启
            if (!Config.IsAutoRestart) return;
            autoRestartTimes += 1;
            //如果自动重启次数过多，取消自动重启
            if (autoRestartTimes > MaxAutoRestartTimes)
            {
                autoRestartTimes = 0;
                OnCancelAutoRestarting?.Invoke();
                return;
            }
            //等待一秒，Crash事件“大概”全部执行完后开始重启
            await Task.Delay(1000);
            OnAutoRestarting?.Invoke(autoRestartTimes);
            //监听以判断重启是否成功
            GameManager.OnGameEndStarting += RestartSuccess;
            GameManager.OnCrashWhenStarting += FailedRestart;
            //启动
            StartGameServer();
        }
        /// <summary>
        /// 自动重启失败时调用该回调
        /// </summary>
        private void FailedRestart()
        {
            //取消事件监听
            GameManager.OnGameEndStarting -= RestartSuccess;
            GameManager.OnCrashWhenStarting -= FailedRestart;
        }
        /// <summary>
        /// 自动重启成功时调用该回调
        /// </summary>
        private void RestartSuccess()
        {
            //取消事件监听
            GameManager.OnGameEndStarting -= RestartSuccess;
            GameManager.OnCrashWhenStarting -= FailedRestart;
            //重置自动重启次数
            autoRestartTimes = 0;
        }
        #endregion
        /// <summary>
        /// 连接核心面板逻辑
        /// </summary>
        public void ConnectCorePanel()
        {
            //TODO: 连接核心面板逻辑
        }
        /// <summary>
        /// 设置游戏服务端类型
        /// </summary>
        /// <param name="type"></param>
        public void SetGameServerType(GameServerType type)
        {
            //服务端运行时不可设置
            if(GameManager != null && GameManager.GameState != GameServerState.Off)
            {
                throw new ServerIsRunningException("服务端正在运行，请勿更改服务端类型！"); 
            }
            if(Config.ServerType == GameServerType.None)
            {
                Config.ServerType = type;
                GameManager = new GameConsoleManager(type);
            }
            else
            {
                Config.ServerType = type;
                GameManager.SetType(type);
            }
        }
    }
    /*文件目录结构：
     * GameServerPanel：
     * -GameServerPanel.exe;
     * -Config.json;
     * -server:
     *  //EZ/EZ套梦故的服务器存放位置
     * -PluginCache:
     *  -[Plugin].dll;
     *  -Plugins.json;
     * -AddonCache:
     *  
     */
}
