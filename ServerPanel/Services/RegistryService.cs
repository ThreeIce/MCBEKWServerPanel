using Grpc.Core;
using GRPCInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerPanel
{
    /// <summary>
    /// 游戏服务器的登录/注册处理
    /// </summary>
    public class RegistryService : Registry.RegistryBase
    {
        public static ConcurrentDictionary<string,GameServerInfo> GameServers;
        public override Task<GameServerLoginResult> GameServerLogin(GameServerInfo request, ServerCallContext context)
        {
            if (GameServers.ContainsKey(request.ServerName))
            {
                Console.WriteLine("有个同名服务器链接！");
                return Task.FromResult<GameServerLoginResult>(new GameServerLoginResult { IsSuccess = false });
            }
            else
            {
                Console.WriteLine($"游戏服务器{request.ServerName}连接到服务器！");
                GameServers.TryAdd(request.ServerName, request);
            }
            return base.GameServerLogin(request, context);
        }

    }
}
