using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SevenZip;

namespace GameServerPanel
{
    /// <summary>
    /// 面板核心控制类
    /// </summary>
    public class PanelManager
    {
        #region Paths
        /// <summary>
        /// 根目录地址
        /// </summary>
        public static readonly string RootDic;
        /// <summary>
        /// 服务端目录
        /// </summary>
        public static string ServerDic { get => RootDic + "\\server"; }
        /// <summary>
        /// 缓存目录
        /// </summary>
        public static string CacheDic { get => RootDic + "\\Cache"; }
        /// <summary>
        /// 插件缓存目录
        /// </summary>
        public static string PluginsCacheDic { get => CacheDic + "\\Plugins"; }
        /// <summary>
        /// Addon缓存目录
        /// </summary>
        public static string AddonsCacheDic { get => CacheDic + "\\Addons"; }
        /// <summary>
        /// 服务端数据文件转存目录
        /// </summary>
        public static string ServerDataDic { get => RootDic + "\\ServerData"; }
        /// <summary>
        /// 服务端行为包路径
        /// </summary>
        public static string BehaviorPacksDic { get => ServerDataDic + "\\behavior_packs"; }
        /// <summary>
        /// 服务端材质包路径
        /// </summary>
        public static string ResourcePacksDic { get => ServerDataDic + "\\resource_packs"; }
        /// <summary>
        /// 真-服务端数据路径（如插件数据等）
        /// </summary>
        public static string DataDic { get => ServerDataDic + "\\datas"; }
        /// <summary>
        /// 存档目录
        /// </summary>
        public static string WorldsDic { get => ServerDataDic + "\\worlds"; }
        /// <summary>
        /// 插件目录
        /// </summary>
        public static string PluginsDic { get => ServerDataDic + "\\Plugins"; }
        /// <summary>
        /// c#插件目录
        /// </summary>
        public static string CSRDic { get => PluginsDic + "\\CSR"; }
        /// <summary>
        /// JSR插件目录
        /// </summary>
        public static string JSRDic { get => PluginsDic + "\\JSR"; }
        /// <summary>
        /// 梦之故里插件目录
        /// </summary>
        public static string MGDic { get => PluginsDic + "\\MG"; }
        /// <summary>
        /// EZ插件目录
        /// </summary>
        public static string EZDic { get => PluginsDic + "\\EZ"; }
        /// <summary>
        /// 备份目录
        /// </summary>
        public static string BackupsDic { get => RootDic + "\\Backups"; }
        public static string WorldsBackupDic { get => BackupsDic + "\\worlds"; }
        public static string DatasBackupDic { get => BackupsDic + "\\datas"; }
        public static string ServerBackupDic { get => BackupsDic + "\\server"; }
        /*文件目录结构：
     * GameServerPanel：
     * -GameServerPanel.exe;
     * -Config.json;
     * -server:
     *  //服务端存放位置
     * -Cache://缓存目录
     *  -Plugins:
     *   -[].dll;
     *   -Plugins.json;
     *  -Addons:
     *  [AddonName].mcaddon/mcpack
     * -ServerData：
     *  //服务器内的数据文件，实施数据和本体分开储存，方便重装服务端
     *  -behavior_packs://服务端行为包
     *  -resource_packs://服务端资源包
     *  -worlds://存档
     *  -datas://插件数据
     *  -Plugins://各种插件
     *   -CSR:
     *   -JSR:
     *   -MG:
     *   -EZ:
     * -Backups://备份文件
     *  -worlds://存档备份
     *  -datas://服务端数据备份
     *  -server://本体备份
     */
        #endregion
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
        /// <summary>
        /// 开始安装时调用
        /// </summary>
        public Action BeginInstalling;
        /// <summary>
        /// 安装结束时调用
        /// </summary>
        public Action EndInstalling;
        /// <summary>
        /// 安装失败时调用，传参为错误内容
        /// </summary>
        public Action<Exception> FailToInstall;
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
            //初始化事件内容
            BeginInstalling += () => GameManager.GameState = GameServerState.Installing;
            EndInstalling += () => GameManager.GameState = GameServerState.Off;
            FailToInstall += (ex) => GameManager.GameState = GameServerState.Off;
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
            Directory.CreateDirectory(ServerDic);
            Directory.CreateDirectory(CacheDic);
            Directory.CreateDirectory(PluginsCacheDic);
            Directory.CreateDirectory(AddonsCacheDic);
            Directory.CreateDirectory(ServerDataDic);
            Directory.CreateDirectory(BehaviorPacksDic);
            Directory.CreateDirectory(ResourcePacksDic);
            Directory.CreateDirectory(WorldsDic);
            Directory.CreateDirectory(DataDic);
            Directory.CreateDirectory(PluginsDic);
            Directory.CreateDirectory(CSRDic);
            Directory.CreateDirectory(MGDic);
            Directory.CreateDirectory(JSRDic);
            Directory.CreateDirectory(EZDic);
            Directory.CreateDirectory(BackupsDic);
            Directory.CreateDirectory(WorldsBackupDic);
            Directory.CreateDirectory(DatasBackupDic);
            Directory.CreateDirectory(ServerBackupDic);

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
            //只有服务端处于安装状态可以设置
            if(GameManager != null && GameManager.GameState != GameServerState.Installing)
            {
                throw new ServerIsRunningException("服务端不处于安装状态，无法更改服务端类型！"); 
            }
            Config.ServerType = type;
            GameManager.SetType(type);
        }
        

        #region Backup
        /// <summary>
        /// 备份服务端文件
        /// </summary>
        public async Task BackupServer(EventHandler<FileOperationProgress> progressChange = null)
        {
            await BackupServer("ServerBackup-" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"),progressChange);
        }
        public async Task BackupServer(string FileName, EventHandler<FileOperationProgress> progressChange = null)
        {
            await Backup(PanelManager.ServerBackupDic + "\\" + FileName + ".7z", ServerDic, progressChange);
        }
        /// <summary>
        /// 备份存档文件
        /// </summary>
        public async Task BackupWorlds(EventHandler<FileOperationProgress> progressChange = null)
        {
            await BackupWorlds("WorldsBackup-" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"),progressChange);
        }
        /// <summary>
        /// 备份存档文件
        /// </summary>
        /// <param name="FileName">输出的压缩包名</param>
        public async Task BackupWorlds(string FileName, EventHandler<FileOperationProgress> progressChange = null)
        {
            await Backup(PanelManager.WorldsBackupDic + "\\" + FileName + ".7z", WorldsDic, progressChange);
        }
        /// <summary>
        /// 备份数据文件
        /// </summary>
        public async Task BackupDatas(EventHandler<FileOperationProgress> progressChange = null)
        {
            await BackupDatas("DatasBackup-" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"), progressChange);
        }
        /// <summary>
        /// 备份数据文件
        /// </summary>
        public async Task BackupDatas(string FileName, EventHandler<FileOperationProgress> progressChange = null)
        {
            await Backup(PanelManager.DatasBackupDic + "\\" + FileName + ".7z", DataDic, progressChange);
        }
        /// <summary>
        /// 备份文件
        /// </summary>
        public async Task Backup(string AchieveFilePath,string SourcePath, EventHandler<FileOperationProgress> progressChange = null)
        {
            //如果要备份的文件夹是空的，抛出错误
            if (Directory.GetFiles(SourcePath).Length == 0)
            {
                throw new BackupException("目标文件是空的，无法备份");
            }
            //创建压缩类
            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
            //添加压缩进度监听事件
            var progress = new FileOperationProgress();
            compressor.FileCompressionStarted += (s, e) =>
            {
                if (progressChange != null)
                {
                    progress.PercentDone = e.PercentDone;
                    progress.CurrentFileName = e.FileName;
                    progressChange(this, progress);
                }
            };
            //如果有同名就覆盖
            if (File.Exists(AchieveFilePath))
            {
                File.Delete(AchieveFilePath);
            }
            using (var s = File.Create(AchieveFilePath))
                await compressor.CompressDirectoryAsync(SourcePath, s,"");//备注：传入流的版本作者貌似忘记把password参数设成可选了，手动传无密码
        }

        #endregion
        #region ServerIntall
        
        public async Task Install(GameServerType type, string path, bool AutoBackup = true,EventHandler<FileOperationsProgress> progressChange = null)
        {
            //只有不在运行时可以安装
            if (GameManager.GameState != GameServerState.Off)
                throw new ServerIsRunningException("服务端正在运行（或者正在安装），请勿安装服务端！");
            //检测是否存在冲突
            GameServerType conflict  = GameServerType.None;
            //如果已经安装了该类型服务端只是升级/倒版本就不需要再检测了
            bool HasInstalled = (GameManager.serverType & type) == type;
            if (!HasInstalled)
            {
                conflict = HasConflict(type);
                if (conflict != GameServerType.None)
                    throw new ServerTypeConflictException(conflict);
            }
            //开始安装
            BeginInstalling?.Invoke();
            try
            {
                //创建文件操作类
                var progress = new FileOperationsProgress();
                //设置BDS安装的任务总数
                progress.TotalNum = 1;
                //备份
                if (AutoBackup)
                {
                    //把备份的任务数添加上去
                    progress.TotalNum += 3;
                    try
                    {
                        //备份服务端文件
                        progress.CurrentOperationId += 1;
                        await BackupServer((s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                    }
                    catch (BackupException)
                    {
                        //没文件要备份就直接跳过
                    }
                    try
                    {
                        //备份世界
                        progress.CurrentOperationId += 1;
                        await BackupWorlds((s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                    }
                    catch (BackupException)
                    {
                        //没文件要备份就直接跳过
                    }
                    try
                    {
                        //备份数据
                        progress.CurrentOperationId += 1;
                        await BackupDatas((s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                    }
                    catch (BackupException)
                    {
                        //没文件要备份就直接跳过
                    }
                }
                progress.CurrentOperationId += 1;
                switch (type)
                {
                    case GameServerType.BDS:
                        //安装BDS
                        await InstallBDS(path, (s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                        break;
                    case GameServerType.EZ:
                        await InstallEZ(path, (s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                        break;

                        //TODO：其他服务端类型的安装逻辑
                }
                if (!HasInstalled)
                {
                    //如果之前没安装过，设置服务端类型
                    SetGameServerType(GameManager.serverType | type);
                }
                //结束安装
                EndInstalling?.Invoke();
            }
            catch(Exception e)
            {
                //出错先调用安装出错回调，然后继续把错误往外抛
                FailToInstall?.Invoke(e);
                throw;
            }
        }
        /// <summary>
        /// 检测目标服务端类型是否和当前已安装的服务端冲突
        /// </summary>
        /// <param name="type">要检测的服务端类型，请务必只传一个！</param>
        /// <returns>和目标服务端类型冲突的已存在服务端类型，如果没有则返回GameServerType.None</returns>
        public GameServerType HasConflict(GameServerType type)
        {
            //如果要检测的服务端类型已经安装就没有检测的必要性了
            if ((Config.ServerType & type) == type)
                throw new ArgumentException("要检测的服务端类型已经安装了！");
            var result = GameServerType.None;
            //如果有装EZ，检测目标是否会和EZ冲突
            if((Config.ServerType & GameServerType.EZ) == GameServerType.EZ)
            {
                //BDX和EZ冲突
                if (type == GameServerType.BDX)
                    result |= GameServerType.EZ;
            }
            //如果有装BDX，检测目标是否和BDX冲突
            if((Config.ServerType & GameServerType.BDX) == GameServerType.BDX)
            {
                //EZ和BDX冲突
                if (type == GameServerType.EZ)
                    result |= GameServerType.BDX;
            }
            //根据不同的类型调用不同的安装方式
            return result;
        }
        /// <summary>
        /// 目前支持解压安装的文件类型，提供给OpenFileDialog使用
        /// </summary>
        public static readonly string SupportFileType = "压缩包 (*.zip;*.7z;*.rar)|*.zip;*.7z;*.rar";
        /// <summary>
        /// 安装BDS
        /// </summary>
        /// <param name="BDSPath">要安装的BDS文件目录</param>
        public async Task InstallBDS(string BDSPath, EventHandler<FileOperationProgress> progressChange = null)
        {
            //检测文件是否有效
            if (!File.Exists(BDSPath))
                throw new ArgumentException("该文件不存在!");
            if (!BDSPath.EndsWith(".zip"))
            {
                throw new ArgumentException("BDS服务端压缩包文件必须是zip");
            }
            //如果是首次安装，创建World、behavior_packs，resource_packs目录的符号链接（不知道是啥百度mklink）
            if ((GameManager.serverType & GameServerType.BDS) != GameServerType.BDS)
            {
                Utils.RunCMD($"mklink /D \"{ServerDic}/worlds\" \"..\\{Utils.GetRelativePath(PanelManager.WorldsDic)}\"");
                Utils.RunCMD($"mklink /D \"{ServerDic}/behavior_packs\" \"..\\{Utils.GetRelativePath(PanelManager.BehaviorPacksDic)}\"");
                Utils.RunCMD($"mklink /D \"{ServerDic}/resource_packs\" \"..\\{Utils.GetRelativePath(PanelManager.ResourcePacksDic)}\"");
            }
            //创建解压类和进度
            SevenZipExtractor extractor = new SevenZipExtractor(BDSPath);

            var progress = new FileOperationProgress();
            //当压缩进度变化时调用事件
            extractor.FileExtractionStarted += (s, e) =>
            {
                if (progressChange != null)
                {
                    progress.PercentDone = e.PercentDone;
                    progress.CurrentFileName = e.FileInfo.FileName;
                    progressChange.Invoke(this, progress);
                }
            };
            //用完释放
            using (extractor)
            {
                //直接解压到服务端目录即可
                await extractor.ExtractArchiveAsync(PanelManager.ServerDic);
            }
        }
        /// <summary>
        /// 安装EZ
        /// </summary>
        /// <param name="EZPath">要安装的BDS文件目录</param>
        public async Task InstallEZ(string EZPath, EventHandler<FileOperationProgress> progressChange = null)
        {
            //检测BDS是否已安装
            if ((GameManager.serverType & GameServerType.BDS) != GameServerType.BDS)
                throw new Exception("请先安装BDS再安装EZ！");
            //检测文件是否有效
            if (!File.Exists(EZPath))
                throw new ArgumentException("该文件不存在!");
            //首次安装要创建Mods的符号链接
            if ((GameManager.serverType & GameServerType.EZ) != GameServerType.EZ)
            {
                Utils.RunCMD($"mklink /D \"{ServerDic}/Mods\" \"..\\{Utils.GetRelativePath(PanelManager.EZDic)}\"");
            }
            //创建解压类和进度
            SevenZipExtractor extractor = new SevenZipExtractor(EZPath);
            var progress = new FileOperationProgress();
            //用完释放
            using (extractor)
            {
                //EZ目录里套了个文件夹，所以要手动剔除
                var Files = extractor.ArchiveFileNames;
                var Count = Files.Count;

                for (int i = 0; i < Count; i++)
                {

                    //由于是自己挨个解压文件，解压进度要由自己计算
                    progress.PercentDone = (float)i * 100f / (float)Count;
                    progress.CurrentFileName = Files[i];
                    progressChange.Invoke(this, progress);
                    //配置解压后文件路径
                    var s = Files[i].Split("\\", 2);//去掉内嵌的EZ文件夹名
                    //因为不明nt原因，Files中存在两目录名（Mods和Lib），但是又没有根目录名……，直接剔除掉没有后缀的
                    if (!s[1].Contains('.'))
                        continue;
                    var ExtractedFileInfo = new FileInfo(ServerDic + "\\" + s[1]);
                    //检测目录是否存在，不存在则创建
                    if (!ExtractedFileInfo.Directory.Exists)
                        ExtractedFileInfo.Directory.Create();
                    Stream stream;
                    //解压
                    stream = File.Open(ExtractedFileInfo.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await extractor.ExtractFileAsync(Files[i], stream);
                    stream.Close();
                    await stream.DisposeAsync();
                }
            }
        }
        #endregion
    }
    /// <summary>
    /// 备份时出现的错误
    /// </summary>
    public class BackupException : Exception
    {
        public BackupException(string message) : base(message)
        {

        }
    }
    /// <summary>
    /// 服务端类型出现冲突时产生的错误
    /// </summary>
    public class ServerTypeConflictException : Exception
    {
        public GameServerType ConflictType;
        public ServerTypeConflictException(GameServerType conflicttype) : base(conflicttype.ToString() + "发生了冲突！")
        {
            ConflictType = conflicttype;
        }
    }
    /// <summary>
    /// 压缩/解压缩进度表示类
    /// </summary>
    public class FileOperationProgress
    {
        /// <summary>
        /// 完成进度
        /// </summary>
        public float PercentDone;
        /// <summary>
        /// 当前处理文件名
        /// </summary>
        public string CurrentFileName;
    }
    /// <summary>
    /// 多个压缩/解压缩操作进度表示类
    /// </summary>
    public class FileOperationsProgress
    {
        /// <summary>
        /// 要执行的操作总数
        /// </summary>
        public int TotalNum;
        /// <summary>
        /// 当前执行操作的序号，默认为0，执行任何操作前都应先将其+1
        /// </summary>
        public int CurrentOperationId = 0;
        /// <summary>
        /// 当前执行操作的完成进度
        /// </summary>
        public FileOperationProgress CurrentOperation;
    }
}

