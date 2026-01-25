namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 用户设置服务接口
/// </summary>
public interface IUserSettingService
{
    /// <summary>
    /// 获取设置值
    /// </summary>
    Task<string?> GetAsync(string key);
    
    /// <summary>
    /// 获取设置值（带默认值）
    /// </summary>
    Task<string> GetAsync(string key, string defaultValue);
    
    /// <summary>
    /// 设置值
    /// </summary>
    Task<bool> SetAsync(string key, string? value);
    
    /// <summary>
    /// 删除设置
    /// </summary>
    Task<bool> DeleteAsync(string key);
    
    /// <summary>
    /// 获取所有设置
    /// </summary>
    Task<Dictionary<string, string?>> GetAllAsync();
}
