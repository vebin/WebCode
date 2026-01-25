using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.ChatSession;

/// <summary>
/// 聊天消息仓储实现
/// </summary>
[ServiceDescription(typeof(IChatMessageRepository), ServiceLifetime.Scoped)]
public class ChatMessageRepository : Repository<ChatMessageEntity>, IChatMessageRepository
{
    public ChatMessageRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据会话ID获取所有消息
    /// </summary>
    public async Task<List<ChatMessageEntity>> GetBySessionIdAsync(string sessionId)
    {
        return await GetDB().Queryable<ChatMessageEntity>()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CreatedAt, OrderByType.Asc)
            .ToListAsync();
    }

    /// <summary>
    /// 根据会话ID和用户名获取所有消息
    /// </summary>
    public async Task<List<ChatMessageEntity>> GetBySessionIdAndUsernameAsync(string sessionId, string username)
    {
        return await GetDB().Queryable<ChatMessageEntity>()
            .Where(x => x.SessionId == sessionId && x.Username == username)
            .OrderBy(x => x.CreatedAt, OrderByType.Asc)
            .ToListAsync();
    }

    /// <summary>
    /// 根据会话ID删除所有消息
    /// </summary>
    public async Task<bool> DeleteBySessionIdAsync(string sessionId)
    {
        return await DeleteAsync(x => x.SessionId == sessionId);
    }

    /// <summary>
    /// 根据会话ID和用户名删除所有消息
    /// </summary>
    public async Task<bool> DeleteBySessionIdAndUsernameAsync(string sessionId, string username)
    {
        return await DeleteAsync(x => x.SessionId == sessionId && x.Username == username);
    }

    /// <summary>
    /// 批量插入消息
    /// </summary>
    public async Task<bool> InsertMessagesAsync(List<ChatMessageEntity> messages)
    {
        if (messages == null || messages.Count == 0)
            return true;
        
        return await InsertRangeAsync(messages);
    }
}
