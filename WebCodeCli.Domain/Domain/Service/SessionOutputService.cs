using System.Text.Json;
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
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            _logger.LogDebug("获取会话输出状态: SessionId={SessionId}, Username={Username}", sessionId, username);
            
            var entity = await _repository.GetBySessionIdAndUsernameAsync(sessionId, username);
            
            if (entity == null)
            {
                _logger.LogDebug("会话输出状态不存在: SessionId={SessionId}", sessionId);
                return null;
            }
            
            _logger.LogDebug("找到会话输出状态: SessionId={SessionId}, EventsJson长度={EventsJsonLength}, IsJsonlActive={IsJsonlActive}", 
                sessionId, entity.EventsJson?.Length ?? 0, entity.IsJsonlOutputActive);
            
            var state = new OutputPanelState
            {
                SessionId = entity.SessionId,
                RawOutput = entity.RawOutput,
                EventsJson = entity.EventsJson,
                DisplayedEventCount = entity.DisplayedEventCount,
                IsJsonlOutputActive = entity.IsJsonlOutputActive,
                ActiveThreadId = entity.ActiveThreadId ?? string.Empty,
                UpdatedAt = entity.UpdatedAt,
                JsonlEvents = new List<OutputJsonlEvent>()
            };
            
            // 反序列化 EventsJson 到 JsonlEvents
            if (!string.IsNullOrWhiteSpace(entity.EventsJson))
            {
                try
                {
                    var events = JsonSerializer.Deserialize<List<OutputJsonlEvent>>(entity.EventsJson, JsonOptions);
                    if (events != null)
                    {
                        state.JsonlEvents = events;
                        _logger.LogDebug("反序列化成功: 共 {Count} 个事件", events.Count);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "反序列化 EventsJson 失败: {SessionId}", sessionId);
                }
            }
            
            return state;
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
            _logger.LogDebug("保存会话输出状态: SessionId={SessionId}, Username={Username}, Events数量={EventCount}", 
                state.SessionId, username, state.JsonlEvents?.Count ?? 0);
            
            // 序列化 JsonlEvents 到 EventsJson
            string? eventsJson = null;
            if (state.JsonlEvents is { Count: > 0 })
            {
                try
                {
                    eventsJson = JsonSerializer.Serialize(state.JsonlEvents, JsonOptions);
                    _logger.LogDebug("序列化 EventsJson 成功: 长度={Length}", eventsJson?.Length ?? 0);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "序列化 JsonlEvents 失败: {SessionId}", state.SessionId);
                }
            }
            
            var entity = new SessionOutputEntity
            {
                SessionId = state.SessionId,
                Username = username,
                RawOutput = state.RawOutput,
                EventsJson = eventsJson,
                DisplayedEventCount = state.DisplayedEventCount,
                IsJsonlOutputActive = state.IsJsonlOutputActive,
                ActiveThreadId = state.ActiveThreadId,
                UpdatedAt = DateTime.Now
            };

            var result = await _repository.SaveOrUpdateAsync(entity);
            _logger.LogDebug("保存结果: {Result}", result);
            return result;
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
