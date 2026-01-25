using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.ChatSession;

/// <summary>
/// 聊天会话仓储实现
/// </summary>
[ServiceDescription(typeof(IChatSessionRepository), ServiceLifetime.Scoped)]
public class ChatSessionRepository : Repository<ChatSessionEntity>, IChatSessionRepository
{
    public ChatSessionRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名获取所有会话
    /// </summary>
    public async Task<List<ChatSessionEntity>> GetByUsernameAsync(string username)
    {
        return await GetListAsync(x => x.Username == username);
    }

    /// <summary>
    /// 根据会话ID和用户名获取会话
    /// </summary>
    public async Task<ChatSessionEntity?> GetByIdAndUsernameAsync(string sessionId, string username)
    {
        return await GetFirstAsync(x => x.SessionId == sessionId && x.Username == username);
    }

    /// <summary>
    /// 根据会话ID和用户名删除会话
    /// </summary>
    public async Task<bool> DeleteByIdAndUsernameAsync(string sessionId, string username)
    {
        return await DeleteAsync(x => x.SessionId == sessionId && x.Username == username);
    }

    /// <summary>
    /// 根据用户名获取会话列表（按更新时间降序）
    /// </summary>
    public async Task<List<ChatSessionEntity>> GetByUsernameOrderByUpdatedAtAsync(string username)
    {
        return await GetDB().Queryable<ChatSessionEntity>()
            .Where(x => x.Username == username)
            .OrderBy(x => x.UpdatedAt, OrderByType.Desc)
            .ToListAsync();
    }
}
