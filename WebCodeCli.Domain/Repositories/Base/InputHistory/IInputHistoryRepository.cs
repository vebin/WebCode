using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.InputHistory;

/// <summary>
/// 输入历史仓储接口
/// </summary>
public interface IInputHistoryRepository : IRepository<InputHistoryEntity>
{
    /// <summary>
    /// 根据用户名获取最近的输入历史
    /// </summary>
    Task<List<InputHistoryEntity>> GetRecentByUsernameAsync(string username, int limit = 50);
    
    /// <summary>
    /// 根据用户名搜索输入历史
    /// </summary>
    Task<List<InputHistoryEntity>> SearchByUsernameAsync(string username, string searchText, int limit = 10);
    
    /// <summary>
    /// 根据用户名清空输入历史
    /// </summary>
    Task<bool> ClearByUsernameAsync(string username);
    
    /// <summary>
    /// 保存输入历史
    /// </summary>
    Task<bool> SaveInputAsync(string username, string text);
}
