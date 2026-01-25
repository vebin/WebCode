using WebCodeCli.Domain.Domain.Model;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 会话输出服务接口
/// </summary>
public interface ISessionOutputService
{
    /// <summary>
    /// 获取会话输出状态
    /// </summary>
    Task<OutputPanelState?> GetBySessionIdAsync(string sessionId);
    
    /// <summary>
    /// 保存会话输出状态
    /// </summary>
    Task<bool> SaveAsync(OutputPanelState state);
    
    /// <summary>
    /// 删除会话输出状态
    /// </summary>
    Task<bool> DeleteBySessionIdAsync(string sessionId);
}
