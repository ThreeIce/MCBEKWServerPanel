using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;

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
