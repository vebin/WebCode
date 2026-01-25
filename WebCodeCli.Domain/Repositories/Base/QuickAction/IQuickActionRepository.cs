using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.QuickAction;

/// <summary>
/// 快捷操作仓储接口
/// </summary>
public interface IQuickActionRepository : IRepository<QuickActionEntity>
{
    /// <summary>
    /// 根据用户名获取所有快捷操作（按顺序排序）
    /// </summary>
    Task<List<QuickActionEntity>> GetByUsernameAsync(string username);
    
    /// <summary>
    /// 根据ID和用户名获取快捷操作
    /// </summary>
    Task<QuickActionEntity?> GetByIdAndUsernameAsync(string id, string username);
    
    /// <summary>
    /// 根据ID和用户名删除快捷操作
    /// </summary>
    Task<bool> DeleteByIdAndUsernameAsync(string id, string username);
    
    /// <summary>
    /// 根据用户名清空所有快捷操作
    /// </summary>
    Task<bool> ClearByUsernameAsync(string username);
    
    /// <summary>
    /// 批量保存快捷操作（先清空再插入）
    /// </summary>
    Task<bool> SaveAllAsync(string username, List<QuickActionEntity> actions);
}
