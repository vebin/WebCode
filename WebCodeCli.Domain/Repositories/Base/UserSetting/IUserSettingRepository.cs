using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.UserSetting;

/// <summary>
/// 用户设置仓储接口
/// </summary>
public interface IUserSettingRepository : IRepository<UserSettingEntity>
{
    /// <summary>
    /// 根据用户名和键获取设置
    /// </summary>
    Task<UserSettingEntity?> GetByKeyAsync(string username, string key);
    
    /// <summary>
    /// 根据用户名获取所有设置
    /// </summary>
    Task<List<UserSettingEntity>> GetAllByUsernameAsync(string username);
    
    /// <summary>
    /// 设置值（不存在则创建，存在则更新）
    /// </summary>
    Task<bool> SetValueAsync(string username, string key, string? value);
    
    /// <summary>
    /// 获取设置值
    /// </summary>
    Task<string?> GetValueAsync(string username, string key);
    
    /// <summary>
    /// 删除设置
    /// </summary>
    Task<bool> DeleteByKeyAsync(string username, string key);
}
