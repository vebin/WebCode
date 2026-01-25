using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebCodeCli.Domain.Common.Extensions;
using WebCodeCli.Domain.Repositories.Base.UserSetting;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 用户设置服务实现
/// </summary>
[ServiceDescription(typeof(IUserSettingService), ServiceLifetime.Scoped)]
public class UserSettingService : IUserSettingService
{
    private readonly IUserSettingRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<UserSettingService> _logger;

    public UserSettingService(
        IUserSettingRepository repository,
        IUserContextService userContextService,
        ILogger<UserSettingService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// 获取设置值
    /// </summary>
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            
            var username = _userContextService.GetCurrentUsername();
            return await _repository.GetValueAsync(username, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置失败: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// 获取设置值（带默认值）
    /// </summary>
    public async Task<string> GetAsync(string key, string defaultValue)
    {
        var value = await GetAsync(key);
        return value ?? defaultValue;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    public async Task<bool> SetAsync(string key, string? value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("设置失败: 键为空");
                return false;
            }
            
            var username = _userContextService.GetCurrentUsername();
            return await _repository.SetValueAsync(username, key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置失败: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 删除设置
    /// </summary>
    public async Task<bool> DeleteAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            
            var username = _userContextService.GetCurrentUsername();
            return await _repository.DeleteByKeyAsync(username, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设置失败: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 获取所有设置
    /// </summary>
    public async Task<Dictionary<string, string?>> GetAllAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetAllByUsernameAsync(username);
            
            return entities.ToDictionary(e => e.Key, e => e.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有设置失败");
            return new Dictionary<string, string?>();
        }
    }
}
