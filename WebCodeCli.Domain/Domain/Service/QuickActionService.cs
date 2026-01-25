using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebCodeCli.Domain.Common.Extensions;
using WebCodeCli.Domain.Domain.Model;
using WebCodeCli.Domain.Repositories.Base.QuickAction;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 快捷操作服务实现
/// </summary>
[ServiceDescription(typeof(IQuickActionService), ServiceLifetime.Scoped)]
public class QuickActionService : IQuickActionService
{
    private readonly IQuickActionRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<QuickActionService> _logger;

    public QuickActionService(
        IQuickActionRepository repository,
        IUserContextService userContextService,
        ILogger<QuickActionService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有快捷操作
    /// </summary>
    public async Task<List<QuickAction>> GetAllAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetByUsernameAsync(username);
            
            return entities.Select(e => new QuickAction
            {
                Id = e.Id,
                Title = e.Title ?? string.Empty,
                Content = e.Prompt ?? string.Empty,
                Icon = e.Icon ?? string.Empty,
                Order = e.Order,
                IsEnabled = e.IsEnabled
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取快捷操作失败");
            return new List<QuickAction>();
        }
    }

    /// <summary>
    /// 保存快捷操作
    /// </summary>
    public async Task<bool> SaveAsync(QuickAction action)
    {
        try
        {
            if (action == null || string.IsNullOrWhiteSpace(action.Id))
            {
                _logger.LogWarning("保存快捷操作失败: 无效的数据");
                return false;
            }

            var username = _userContextService.GetCurrentUsername();
            var entity = new QuickActionEntity
            {
                Id = action.Id,
                Username = username,
                Title = action.Title,
                Icon = action.Icon,
                Prompt = action.Content,
                Order = action.Order,
                IsEnabled = action.IsEnabled
            };

            return await _repository.InsertOrUpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存快捷操作失败: {Id}", action?.Id);
            return false;
        }
    }

    /// <summary>
    /// 批量保存快捷操作
    /// </summary>
    public async Task<bool> SaveAllAsync(List<QuickAction> actions)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = actions.Select(a => new QuickActionEntity
            {
                Id = a.Id,
                Username = username,
                Title = a.Title,
                Icon = a.Icon,
                Prompt = a.Content,
                Order = a.Order,
                IsEnabled = a.IsEnabled
            }).ToList();

            return await _repository.SaveAllAsync(username, entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量保存快捷操作失败");
            return false;
        }
    }

    /// <summary>
    /// 删除快捷操作
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            return await _repository.DeleteByIdAndUsernameAsync(id, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除快捷操作失败: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// 清空所有快捷操作
    /// </summary>
    public async Task<bool> ClearAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            return await _repository.ClearByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空快捷操作失败");
            return false;
        }
    }
}
