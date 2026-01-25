using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebCodeCli.Domain.Common.Extensions;
using WebCodeCli.Domain.Repositories.Base.InputHistory;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 输入历史服务实现
/// </summary>
[ServiceDescription(typeof(IInputHistoryService), ServiceLifetime.Scoped)]
public class InputHistoryService : IInputHistoryService
{
    private readonly IInputHistoryRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<InputHistoryService> _logger;

    public InputHistoryService(
        IInputHistoryRepository repository,
        IUserContextService userContextService,
        ILogger<InputHistoryService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// 获取最近的输入历史
    /// </summary>
    public async Task<List<InputHistoryItem>> GetRecentAsync(int limit = 50)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetRecentByUsernameAsync(username, limit);
            
            return entities.Select(e => new InputHistoryItem
            {
                Id = e.Id,
                Text = e.Text ?? string.Empty,
                Timestamp = e.Timestamp
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取输入历史失败");
            return new List<InputHistoryItem>();
        }
    }

    /// <summary>
    /// 搜索输入历史
    /// </summary>
    public async Task<List<InputHistoryItem>> SearchAsync(string searchText, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<InputHistoryItem>();
            
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.SearchByUsernameAsync(username, searchText, limit);
            
            return entities.Select(e => new InputHistoryItem
            {
                Id = e.Id,
                Text = e.Text ?? string.Empty,
                Timestamp = e.Timestamp
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索输入历史失败: {SearchText}", searchText);
            return new List<InputHistoryItem>();
        }
    }

    /// <summary>
    /// 保存输入历史
    /// </summary>
    public async Task<bool> SaveAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            
            var username = _userContextService.GetCurrentUsername();
            return await _repository.SaveInputAsync(username, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存输入历史失败");
            return false;
        }
    }

    /// <summary>
    /// 清空输入历史
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
            _logger.LogError(ex, "清空输入历史失败");
            return false;
        }
    }
}
