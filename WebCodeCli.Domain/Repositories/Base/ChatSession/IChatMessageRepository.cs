using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.ChatSession;

/// <summary>
/// 聊天消息仓储接口
/// </summary>
public interface IChatMessageRepository : IRepository<ChatMessageEntity>
{
    /// <summary>
    /// 根据会话ID获取所有消息
    /// </summary>
    Task<List<ChatMessageEntity>> GetBySessionIdAsync(string sessionId);
    
    /// <summary>
    /// 根据会话ID和用户名获取所有消息
    /// </summary>
    Task<List<ChatMessageEntity>> GetBySessionIdAndUsernameAsync(string sessionId, string username);
    
    /// <summary>
    /// 根据会话ID删除所有消息
    /// </summary>
    Task<bool> DeleteBySessionIdAsync(string sessionId);
    
    /// <summary>
    /// 根据会话ID和用户名删除所有消息
    /// </summary>
    Task<bool> DeleteBySessionIdAndUsernameAsync(string sessionId, string username);
    
    /// <summary>
    /// 批量插入消息
    /// </summary>
    Task<bool> InsertMessagesAsync(List<ChatMessageEntity> messages);
}
