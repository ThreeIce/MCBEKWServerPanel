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
        public static string CSRPluginDic { get => PluginsDic + "\\CSR"; }
        /// <summary>
        /// JSR插件目录
        /// </summary>
        public static string JSRPluginDic { get => PluginsDic + "\\JSR"; }
        /// <summary>
        /// 梦之故里插件目录
        /// </summary>
        public static string MGPluginDic { get => PluginsDic + "\\MG"; }
        /// <summary>
        /// EZ C++插件目录
        /// </summary>
        public static string EZPluginDic { get => PluginsDic + "\\EZ"; }
        /// <summary>
        /// BDX C++插件目录
        /// </summary>
        public static string BDXPluginDic { get => PluginsDic + "\\BDX"; }
        /// <summary>
        /// BDXLua插件目录
        /// </summary>
        public static string BDXLuaPluginDic { get => PluginsDic + "\\Plugin"; }
        /// <summary>
        /// 配置文件目录
        /// </summary>
        public static string ConfigsDic { get => ServerDataDic + "\\Configs"; }
        /// <summary>
        /// BDS配置路径
        /// </summary>
        public static string BDSConfigPath { get => ServerDataDic + "\\server.properties"; }
        /// <summary>
        /// EZ配置路径
        /// </summary>
        public static string EZConfigPath { get => ServerDataDic + "\\ezcustom.yaml"; }
        /// <summary>
        /// 各种杂项配置文件目录，包含BDX配置文件
        /// </summary>
        public static string BasicConfigDic { get => ServerDataDic + "\\config"; }
        /// <summary>
        /// 备份目录
        /// </summary>
        public static string BackupsDic { get => RootDic + "\\Backups"; }
        /// <summary>
        /// 存档备份目录
        /// </summary>
        public static string WorldsBackupDic { get => BackupsDic + "\\worlds"; }
        /// <summary>
        /// 服务端数据备份目录
        /// </summary>
        public static string DatasBackupDic { get => BackupsDic + "\\datas"; }
        /// <summary>
        /// 服务端备份目录
        /// </summary>
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
     *  -Configs: //各类配置文件
     *   -server.properties//BDX配置文件
     *   -ezcustom.yaml//EZ配置文件
     *   -config:
     *    //各类杂项配置文件，包括BDX配置文件
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
        public Action<GameServerType> BeginInstalling;
        /// <summary>
        /// 安装结束时调用
        /// </summary>
        public Action<GameServerType> EndInstalling;
        /// <summary>
        /// 安装失败时调用，传参为错误内容
        /// </summary>
        public Action<GameServerType,Exception> FailToInstall;
        /// <summary>
        /// 开始重置服务端类型时调用
        /// </summary>
        public Action BeginResetingServer;
        /// <summary>
        /// 成功重置服务端类型时调用
        /// </summary>
        public Action EndResetingServer;
        /// <summary>
        /// 重置服务端类型失败时调用
        /// </summary>
        public Action<Exception> FailedToResetServer;
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
            BeginInstalling += (t) => GameManager.GameState = GameServerState.Installing;
            EndInstalling += (t) => GameManager.GameState = GameServerState.Off;
            FailToInstall += (t,ex) => GameManager.GameState = GameServerState.Off;
            BeginResetingServer += () => GameManager.GameState = GameServerState.Installing;
            EndResetingServer += () => GameManager.GameState = GameServerState.Off;
            FailedToResetServer += (ex) => GameManager.GameState = GameServerState.Off;
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
            Directory.CreateDirectory(CSRPluginDic);
            Directory.CreateDirectory(MGPluginDic);
            Directory.CreateDirectory(JSRPluginDic);
            Directory.CreateDirectory(EZPluginDic);
            Directory.CreateDirectory(BDXPluginDic);
            Directory.CreateDirectory(BDXLuaPluginDic);
            Directory.CreateDirectory(ConfigsDic);
            File.Create(BDSConfigPath);
            File.Create(EZConfigPath);
            Directory.CreateDirectory(BasicConfigDic);
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
        /// <summary>
        /// 安装服务端通用接口，BDX不走此路
        /// </summary>
        public async Task Install(GameServerType type, string path, bool IsAutoBackup = true,EventHandler<FileOperationsProgress> progressChange = null)
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
            BeginInstalling?.Invoke(type);
            try
            {
                //创建文件操作类
                var progress = new FileOperationsProgress();
                //设置BDS安装的任务总数
                progress.TotalNum = 1;
                //备份
                if (IsAutoBackup)
                {
                    await AutoBackup(progress, progressChange);
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
                        //安装EZ
                        await InstallEZ(path, (s, e) =>
                        {
                            progress.CurrentOperation = e;
                            progressChange?.Invoke(this, progress);
                        });
                        break;
                    case GameServerType.BDX:
                        //安装BDX
                        await InstallBDX(path,(s,e) =>
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
                EndInstalling?.Invoke(type);
            }
            catch(Exception e)
            {
                //出错先调用安装出错回调，然后继续把错误往外抛
                FailToInstall?.Invoke(type,e);
                throw;
            }
        }
        /// <summary>
        /// 内部复用自动备份的代码的函数，会对世界、服务端、服务端数据都进行备份
        /// </summary>
        /// <param name="op">要进行自动备份的任务的操作进程表示对象，会自动给操作总量+3</param>
        /// <param name="progressChange">原本任务的进度变更事件</param>
        /// <returns></returns>
        private async Task AutoBackup(FileOperationsProgress progress,EventHandler<FileOperationsProgress> progressChange)
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
            //如果有装EZ的冲突检测
            if((Config.ServerType & GameServerType.EZ) == GameServerType.EZ)
            {
                //BDX和EZ冲突
                if (type == GameServerType.BDX)
                    result |= GameServerType.EZ;
            }
            //如果有装BDX的冲突检测
            if((Config.ServerType & GameServerType.BDX) == GameServerType.BDX)
            {
                //EZ和BDX冲突
                if (type == GameServerType.EZ)
                    result |= GameServerType.BDX;
                //BDX和梦故冲突
                if(type == GameServerType.MengGu)
                {
                    result |= GameServerType.BDX;
                }
            }
            //如果有装MG的冲突检测
            if((Config.ServerType & GameServerType.MengGu) == GameServerType.MengGu)
            {
                //MG和BDX冲突
                if(type == GameServerType.BDX)
                {
                    result |= GameServerType.BDX;
                }
            }
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
            //如果是首次安装，创建World、behavior_packs，resource_packs目录和server.properties的符号链接（不知道是啥百度mklink）
            if ((GameManager.serverType & GameServerType.BDS) != GameServerType.BDS)
            {
                await Utils.RunCMD($"mklink /D \"{ServerDic}/worlds\" \"..\\{Utils.GetRelativePath(PanelManager.WorldsDic)}\"");
                await Utils.RunCMD($"mklink /D \"{ServerDic}/behavior_packs\" \"..\\{Utils.GetRelativePath(PanelManager.BehaviorPacksDic)}\"");
                await Utils.RunCMD($"mklink /D \"{ServerDic}/resource_packs\" \"..\\{Utils.GetRelativePath(PanelManager.ResourcePacksDic)}\"");
                await Utils.RunCMD($"mklink \"{ServerDic}/server.properties\" \"..\\{Utils.GetRelativePath(PanelManager.BDSConfigPath)}\"");
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
            //首次安装要创建Mods和custom.yaml的符号链接
            if ((GameManager.serverType & GameServerType.EZ) != GameServerType.EZ)
            {
                await Utils.RunCMD($"mklink /D \"{ServerDic}/Mods\" \"..\\{Utils.GetRelativePath(PanelManager.EZPluginDic)}\"");
                await Utils.RunCMD($"mklink \"{ServerDic}/custom.yaml\" \"..\\{Utils.GetRelativePath(EZConfigPath)}\"");
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
        /// <summary>
        /// 安装BDX
        /// </summary>
        public async Task InstallBDX(string BDXPath,EventHandler<FileOperationProgress> progressChange = null)
        {
            //检测文件是否有效
            if (!File.Exists(BDXPath))
                throw new ArgumentException("该文件不存在!");
            //如果是首次安装
            if ((GameManager.serverType & GameServerType.BDX) != GameServerType.BDX)
            {
                //创建bdxmod,lua,config目录的符号链接（不知道是啥百度mklink）
                await Utils.RunCMD($"mklink /D \"{ServerDic}/bdxmod\" \"..\\{Utils.GetRelativePath(BDXPluginDic)}\"");
                await Utils.RunCMD($"mklink /D \"{ServerDic}/lua\" \"..\\{Utils.GetRelativePath(BDXLuaPluginDic)}\"");
                //config目录的符号链接可能会在装别的启动器时创建，所以先检测一下
                if (!Directory.Exists(ServerDic + "/config"))
                {
                    await Utils.RunCMD($"mklink /D \"{ServerDic}/config\" \"..\\{Utils.GetRelativePath(BasicConfigDic)}\"");
                }
                //BDX运行有个头疼的目录问题，为了解决通过bat运行
                File.WriteAllText(ServerDic + "/RunBDX.bat", "cd server&bedrock_server.exe");
                File.WriteAllText(ServerDic + "/RunRoDB.bat", "cd server&RoDB.exe");
            }
            //创建解压类和进度
            SevenZipExtractor extractor = new SevenZipExtractor(BDXPath);
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
            //运行RoDB.exe
            await Utils.RunCMD(ServerDic + "/RunRoDB.bat",10000);
        }
        public string BDXCDKPath { get => ServerDic + "/b.txt"; }
        public string GetBDXCDK()
        {
            if (File.Exists(BDXCDKPath))
                return File.ReadAllText(BDXCDKPath);
            else
                return null;
        }
        public void SaveBDXCDK(string cdk)
        {
            File.WriteAllText(BDXCDKPath, cdk);
        }
        /// <summary>
        /// 重置服务端类型，卸载所有服务端
        /// </summary>
        public async Task ResetServer(bool IsAutoBackup = true,EventHandler<FileOperationsProgress> progressChange = null)
        {
            //只有不在运行时可以重置服务端
            if (GameManager.GameState != GameServerState.Off)
                throw new ServerIsRunningException("服务端正在运行（或者正在安装），请勿重置服务端！");
            //如果服务端类型为None，即没装，就不允许删除
            if(Config.ServerType == GameServerType.None)
            {
                throw new ArgumentException("没装服务端的情况下不允许重置服务端！");
            }
            try
            {
                BeginResetingServer?.Invoke();
                //创建进度表示类
                FileOperationsProgress progress = new();
                progress.TotalNum = 1;
                //自动备份
                if (IsAutoBackup)
                {
                    await AutoBackup(progress, progressChange);
                }
                progress.CurrentOperationId += 1;
                //卸载服务端，简单粗暴全部删除
                DirectoryInfo info = new DirectoryInfo(ServerDic);
                var Files = info.GetFiles();
                var Directories = info.GetDirectories();
                int Count = Files.Length + Directories.Length;
                FileOperationProgress FileProgress = new();
                progress.CurrentOperation = FileProgress;
                //先删文件
                for(int i = 0; i<Files.Length;i++)
                {
                    if (progressChange != null)
                    {
                        FileProgress.CurrentFileName = Files[i].FullName;
                        FileProgress.PercentDone = i / Count;
                        progressChange?.Invoke(this, progress);
                    }
                    Files[i].Delete();
                }
                //再删目录
                for(int i = 0; i < Directories.Length; i++)
                {
                    if (progressChange != null)
                    {
                        FileProgress.CurrentFileName = Directories[i].FullName;
                        FileProgress.PercentDone = (i + Files.Length) / Count;
                        progressChange(this, progress);
                    }
                    Directories[i].Delete(true);
                }
                //删完设置服务端类型
                SetGameServerType(GameServerType.None);
                EndResetingServer?.Invoke();
            }catch(Exception e)
            {
                FailedToResetServer?.Invoke(e);
                throw;
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

