using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GameServerPanel
{
    /// <summary>
    /// 游戏服务器状态
    /// </summary>
    public enum GameServerState { 
    
        /// <summary>
        /// 未运行
        /// </summary>
        Off,
        /// <summary>
        /// 启动中
        /// </summary>
        Starting,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 关闭中
        /// </summary>
        Stopping,
        /// <summary>
        /// 安装中
        /// </summary>
        Installing
    }
    /// <summary>
    /// 游戏进程控制器
    /// </summary>
    public class GameConsoleManager : INotifyPropertyChanged
    {
        /// <summary>
        /// 游戏服务端路径
        /// </summary>
        public string GamePath{get;private set; }
        /// <summary>
        /// 游戏服务端启动参数
        /// </summary>
        public string StartArgs { get; private set; }
        /// <summary>
        /// 游戏服务端类型
        /// </summary>
        public GameServerType serverType { get; private set; }
        /// <summary>
        /// 服务端状态
        /// </summary>
        public GameServerState GameState
        {
            get => gameState; set => OnPropertyChanged<GameServerState>(ref gameState, value);
        }
        private GameServerState gameState = GameServerState.Off;
        #region 事件大全
        /// <summary>
        /// 在游戏命令行输出一行时调用
        /// </summary>
        public Action<string> OnLog;
        /// <summary>
        /// 在游戏输出一个错误时调用
        /// </summary>
        public Action<string> OnError;
        /// <summary>
        /// 在游戏刚启动时调用
        /// </summary>
        public Action OnGameBeginStarting;
        /// <summary>
        /// 在游戏启动完调用
        /// </summary>
        public Action OnGameEndStarting;
        /// <summary>
        /// 在游戏刚关闭时调用
        /// </summary>
        public Action OnGameBeginStopping;
        /// <summary>
        /// 在游戏关闭完成时调用（非正常关闭不会调用！）
        /// </summary>
        public Action OnGameEndStopping;
        /// <summary>
        /// 在游戏因为各种神奇原因关闭失败时调用
        /// </summary>
        public Action OnFailStartStoping;
        /// <summary>
        /// 启动游戏时崩服则调用
        /// </summary>
        public Action OnCrashWhenStarting;
        /// <summary>
        /// 游戏运行时崩服则调用
        /// </summary>
        public Action OnCrashWhenRunning;
        #endregion
        /// <summary>
        /// 游戏进程
        /// </summary>
        private Process GameProcess;

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 类内属性发生改变时调用
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged<T>(ref T item,T value,[CallerMemberName]string name = null)
        {
            //为了减少装箱好像不用搞的这么复杂吧……算了懒得改了
            if((item as IEqualityComparer<T>)?.Equals(value) == true || item.Equals(value))
            {
                return;
            }
            else
            {
                item = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// 服务端目录
        /// </summary>
        public static string GameDic { get => PanelManager.RootDic + "/server"; }

        public GameConsoleManager(GameServerType type)
        {
            SetType(type);
            GameState = GameServerState.Off;
            //在四个状态切换事件被调用时将GameState设置成对应状态
            OnGameBeginStarting += () => GameState = GameServerState.Starting;
            OnGameEndStarting += () => GameState = GameServerState.Running;
            OnGameBeginStopping += () => GameState = GameServerState.Stopping;
            OnGameEndStopping += () => GameState = GameServerState.Off;
            OnCrashWhenRunning += () => GameState = GameServerState.Off;
            OnCrashWhenStarting += () => GameState = GameServerState.Off;
            
        }
        
        /// <summary>
        /// 设置服务端类型，以设置启动端信息
        /// </summary>
        /// <param name="type"></param>
        public void SetType(GameServerType type)
        {
            serverType = type;
            if(type == GameServerType.None)
            {
                GamePath = "";
                StartArgs = "";
                return;
            }
            if ((type & GameServerType.EZ) == GameServerType.EZ)
            {
                if ((type & GameServerType.MengGu) == GameServerType.MengGu)
                {
                    //梦故套EZ
                    GamePath = GameDic + "/MCDllInject.exe";
                    StartArgs = "bedrock_server_mod.exe MCModDll";
                }
                else
                {
                    //单开EZ
                    GamePath = GameDic + "/bedrock_server_mod.exe";
                    StartArgs = "";
                }
            }
            else if ((type & GameServerType.MengGu) == GameServerType.MengGu)
            {
                //单开梦故
                GamePath = GameDic + "/MCDllInject.exe";
                StartArgs = "bedrock_server.exe MCModDll";
            }else if((type & GameServerType.BDX) == GameServerType.BDX)
            {
                //BDX启动逻辑（由于BDX和EZ/MG都不能套，所以不考虑套娃情况）
                GamePath = GameDic + "/RunBDX.bat";
                StartArgs = "";
            }
            else
            {
                //裸奔
                GamePath = GameDic + "/bedrock_server.exe";
                StartArgs = "";
            }
        }
        /// <summary>
        /// 启动服务端
        /// </summary>
        public void Start()
        {
            //若服务端类型为空，无法启动
            if (serverType == GameServerType.None)
            {
                throw new ServerTypeException("服务端类型为空，请先安装服务端！");
            }
            if (GameState != GameServerState.Off)
            {
                throw new ServerIsRunningException("服务端正在运行，请勿启动！");
            }
            GameProcess = new Process();
            //配置启动信息
            GameProcess.StartInfo.FileName = GamePath;
            GameProcess.StartInfo.Arguments = StartArgs;
            GameProcess.StartInfo.CreateNoWindow = true;
            //重定向输入输出，方便程序接受
            GameProcess.StartInfo.RedirectStandardOutput = true;
            GameProcess.StartInfo.RedirectStandardInput = true;
            GameProcess.StartInfo.RedirectStandardError = true;
            //设置输入输出的编码
            GameProcess.StartInfo.StandardInputEncoding = Encoding.UTF8;
            GameProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //启动
            GameProcess.Start();
            //调用启动事件
            OnGameBeginStarting?.Invoke();
            //添加监测何时进入运行状态的监听
            OnLog += WaitingForEndStarting;
            //开始监听输入输出
            StartListening();

        }
        /// <summary>
        /// 开始监听游戏控制台
        /// </summary>
        public void StartListening()
        {
            StartListeningLog();
            StartListeningError();
        }
        /// <summary>
        /// 监听Log输出
        /// </summary>
        private async void StartListeningLog()
        {
            var LogStream = GameProcess.StandardOutput;
            do
            {
                var s = await LogStream.ReadLineAsync();
                //当程序停止运行时，将一直读到null,即s==null时程序已停止运行
                if (s == null)
                {
                    break;
                }
                OnLog?.Invoke(s);
            } while (true);
        }
        /// <summary>
        /// 监听Error输出
        /// </summary>
        private async void StartListeningError()
        {
            var ErrorStream = GameProcess.StandardError;
            do
            {
                var s = await ErrorStream.ReadLineAsync();
                //当程序停止运行时，将一直读到null，即s==null时程序已停止运行
                if(s == null)
                {
                    break;
                }
                OnError?.Invoke(s);
            } while (true);
            //以Error流读出null为判断进程关闭的基准，并调用进程关闭事件
            OnProcessExit();
        }
        /// <summary>
        /// 监听输出并确定何时进入运行状态的回调
        /// </summary>
        protected void WaitingForEndStarting(string log)
        {
            //如果出现Server started，即进入运行状态
            if(log.Contains("Server started"))
            {
                //取消监听
                OnLog -= WaitingForEndStarting;
                //调用事件
                OnGameEndStarting?.Invoke();
            }
        }
        /// <summary>
        /// 关闭服务端
        /// </summary>
        public void Stop()
        {
            if (GameState == GameServerState.Starting)
            {
                throw new ServerIsNotRunningException("服务端正在启动，无法关闭！");
            }
            else if(GameState == GameServerState.Off)
            {
                throw new ServerIsNotRunningException("服务端不在运行，无法关闭！");
            }else if(GameState == GameServerState.Stopping)
            {
                throw new ServerIsNotRunningException("服务端正在关闭，请勿重复！");
            }
            InputCommand("stop");
            //通过IsStartStoping来确认是否进入Stopping状态
            OnLog += IsStartStoping;
        }
        /// <summary>
        /// 由于stop指令有时会识别不了，该回调用来确认stop是否执行成功，执行成功才进入关闭状态
        /// </summary>
        /// <param name="output"></param>
        private void IsStartStoping(string output)
        {
            if(output.Contains("Server stop requested"))
            {
                //关闭成功
                OnGameBeginStopping?.Invoke();
            }
            else
            {
                OnFailStartStoping?.Invoke();
            }
            OnLog -= IsStartStoping;
        }
        /// <summary>
        /// 强制关闭服务端
        /// </summary>
        public void Kill()
        {
            GameProcess.Kill();
        }
        /// <summary>
        /// 当游戏进程结束时的回调
        /// </summary>
        protected void OnProcessExit()
        {
            //判断是自然关闭还是错误关闭
            if(GameState == GameServerState.Stopping)
            {
                //自然关闭
                OnGameEndStopping?.Invoke();
            }
            else if(GameState == GameServerState.Starting)
            {
                //TODO:启动失败
                OnCrashWhenStarting?.Invoke();
            }
            else
            {
                //TODO:运行出错
                OnCrashWhenRunning?.Invoke();
            }
        }
        /// <summary>
        /// 向控制台中输入指令
        /// </summary>
        public void InputCommand(string cmd)
        {
            if (GameState == GameServerState.Off)
            {
                throw new ServerIsNotRunningException("服务端不在运行，无法输入指令！");
            }else if(GameState == GameServerState.Starting)
            {
                throw new ServerIsNotRunningException("服务端正在启动，无法输入指令！");
            }else if(GameState == GameServerState.Stopping)
            {
                throw new ServerIsNotRunningException("服务端正在关闭，无法输入指令！");
            }
            GameProcess.StandardInput.WriteLine(cmd);
        }
        
        
    }
    /// <summary>
    /// ServerType出错时引发的错误
    /// </summary>
    public class ServerTypeException : Exception
    {
        public ServerTypeException(string message) : base(message)
        {

        }
    }
    /// <summary>
    /// 服务端正在运行时进行停止时操作引起的错误
    /// </summary>
    public class ServerIsRunningException : Exception
    {
        public ServerIsRunningException(string message) : base(message)
        {

        }
    }
    /// <summary>
    /// 服务端未运行时试图进行运行时操作引起的错误
    /// </summary>
    public class ServerIsNotRunningException : Exception
    {
        public ServerIsNotRunningException(string message) : base(message)
        {

        }
    }
}
