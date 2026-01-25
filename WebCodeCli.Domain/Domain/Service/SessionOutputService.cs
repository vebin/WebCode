using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebCodeCli.Domain.Common.Extensions;
using WebCodeCli.Domain.Domain.Model;
using WebCodeCli.Domain.Repositories.Base.SessionOutput;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 会话输出服务实现
/// </summary>
[ServiceDescription(typeof(ISessionOutputService), ServiceLifetime.Scoped)]
public class SessionOutputService : ISessionOutputService
{
    private readonly ISessionOutputRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<SessionOutputService> _logger;

    public SessionOutputService(
        ISessionOutputRepository repository,
        IUserContextService userContextService,
        ILogger<SessionOutputService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// 获取会话输出状态
    /// </summary>
    public async Task<OutputPanelState?> GetBySessionIdAsync(string sessionId)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entity = await _repository.GetBySessionIdAndUsernameAsync(sessionId, username);
            
            if (entity == null)
                return null;
            
            return new OutputPanelState
            {
                SessionId = entity.SessionId,
                RawOutput = entity.RawOutput,
                EventsJson = entity.EventsJson,
                DisplayedEventCount = entity.DisplayedEventCount,
                UpdatedAt = entity.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话输出状态失败: {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// 保存会话输出状态
    /// </summary>
    public async Task<bool> SaveAsync(OutputPanelState state)
    {
        try
        {
            if (state == null || string.IsNullOrWhiteSpace(state.SessionId))
            {
                _logger.LogWarning("保存输出状态失败: 无效的状态数据");
                return false;
            }

            var username = _userContextService.GetCurrentUsername();
            var entity = new SessionOutputEntity
            {
                SessionId = state.SessionId,
                Username = username,
                RawOutput = state.RawOutput,
                EventsJson = state.EventsJson,
                DisplayedEventCount = state.DisplayedEventCount,
                UpdatedAt = DateTime.Now
            };

            return await _repository.SaveOrUpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存会话输出状态失败: {SessionId}", state?.SessionId);
            return false;
        }
    }

    /// <summary>
    /// 删除会话输出状态
    /// </summary>
    public async Task<bool> DeleteBySessionIdAsync(string sessionId)
    {
        try
        {
            return await _repository.DeleteBySessionIdAsync(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除会话输出状态失败: {SessionId}", sessionId);
            return false;
        }
    }
}
