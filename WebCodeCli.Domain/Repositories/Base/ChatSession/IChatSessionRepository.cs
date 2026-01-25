using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.ChatSession;

/// <summary>
/// 聊天会话仓储接口
/// </summary>
public interface IChatSessionRepository : IRepository<ChatSessionEntity>
{
    /// <summary>
    /// 根据用户名获取所有会话
    /// </summary>
    Task<List<ChatSessionEntity>> GetByUsernameAsync(string username);
    
    /// <summary>
    /// 根据会话ID和用户名获取会话
    /// </summary>
    Task<ChatSessionEntity?> GetByIdAndUsernameAsync(string sessionId, string username);
    
    /// <summary>
    /// 根据会话ID和用户名删除会话
    /// </summary>
    Task<bool> DeleteByIdAndUsernameAsync(string sessionId, string username);
    
    /// <summary>
    /// 根据用户名获取会话列表（按更新时间降序）
    /// </summary>
    Task<List<ChatSessionEntity>> GetByUsernameOrderByUpdatedAtAsync(string username);
}
