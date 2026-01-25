using WebCodeCli.Domain.Domain.Model;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 快捷操作服务接口
/// </summary>
public interface IQuickActionService
{
    /// <summary>
    /// 获取所有快捷操作
    /// </summary>
    Task<List<QuickAction>> GetAllAsync();
    
    /// <summary>
    /// 保存快捷操作
    /// </summary>
    Task<bool> SaveAsync(QuickAction action);
    
    /// <summary>
    /// 批量保存快捷操作
    /// </summary>
    Task<bool> SaveAllAsync(List<QuickAction> actions);
    
    /// <summary>
    /// 删除快捷操作
    /// </summary>
    Task<bool> DeleteAsync(string id);
    
    /// <summary>
    /// 清空所有快捷操作
    /// </summary>
    Task<bool> ClearAsync();
}
