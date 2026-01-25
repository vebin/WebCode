using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.SessionOutput;

/// <summary>
/// 会话输出仓储接口
/// </summary>
public interface ISessionOutputRepository : IRepository<SessionOutputEntity>
{
    /// <summary>
    /// 根据会话ID获取输出状态
    /// </summary>
    Task<SessionOutputEntity?> GetBySessionIdAsync(string sessionId);
    
    /// <summary>
    /// 根据会话ID和用户名获取输出状态
    /// </summary>
    Task<SessionOutputEntity?> GetBySessionIdAndUsernameAsync(string sessionId, string username);
    
    /// <summary>
    /// 根据会话ID删除输出状态
    /// </summary>
    Task<bool> DeleteBySessionIdAsync(string sessionId);
    
    /// <summary>
    /// 保存或更新输出状态
    /// </summary>
    Task<bool> SaveOrUpdateAsync(SessionOutputEntity entity);
}
