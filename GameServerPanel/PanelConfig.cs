using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;


namespace GameServerPanel
{
    /// <summary>
    /// 面板的配置信息类
    /// </summary>
    public class PanelConfig : INotifyPropertyChanged
    {
        
        public static string ConfigPath { get => PanelManager.RootDic + "/config.json"; }

        /// <summary>
        /// 核心面板地址
        /// </summary>
        public string CorePanelAddress
        {
            get => corePanelAddress; set
            {
                if (corePanelAddress != value)
                {
                    corePanelAddress = value;
                    OnPropertyChanged(() => CorePanelAddress);
                }

            }
        }

        /// <summary>
        /// 核心面板端口
        /// </summary>
        public float CorePanelPort { get => corePanelPort; set {
                if (corePanelPort != value)
                {
                    corePanelPort = value;
                    OnPropertyChanged(() => CorePanelPort);
                }
            } 
        }

        /// <summary>
        /// 服务端的类型
        /// </summary>
        public GameServerType ServerType { get => serverType; set
            {
                if (serverType != value)
                {
                    serverType = value;
                    OnPropertyChanged(() => ServerType);
                }
            }
        }

        /// <summary>
        /// 启用WebSocket服务的端口
        /// </summary>
        public float WebSocketPort
        {
            get => webSocketPort; set
            {
                if (webSocketPort != value)
                {
                    webSocketPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private string corePanelAddress = "localhost";
        private float corePanelPort = 13576;
        private GameServerType serverType = GameServerType.None;
        private float webSocketPort = 36330;

        //属性变更通知面板上的显示值更改
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 当属性变更时的通知回调
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="property">"() => 要更改的属性"这样一个lambda表达式</param>
        private void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((property.Body as MemberExpression).Member.Name));
            SetToFile();
        }
        //另一种实现方式的尝试
        private void OnPropertyChanged([CallerMemberName]string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            SetToFile();
        }

        /// <summary>
        /// 读取配置文件，若不存在则返回Null
        /// </summary>
        /// <returns>配置文件，不存在则为null</returns>
        public static PanelConfig ReadFromFile()
        {
            string configPath = ConfigPath;
            if (!File.Exists(ConfigPath))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<PanelConfig>(File.ReadAllText(ConfigPath));
            }
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public void SetToFile()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this));
        }
        /// <summary>
        /// 创建新的配置文件
        /// </summary>
        public static PanelConfig CreateNew()
        {
            var config = new PanelConfig();
            File.Create(ConfigPath);
            config.SetToFile();
            return config;
        }
    }
    [Flags]
    public enum GameServerType {
        None = 0,
        BDS = 1,
        EZ = 2,
        MengGu = 4
    }

}
