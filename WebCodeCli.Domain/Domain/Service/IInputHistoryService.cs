namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 输入历史服务接口
/// </summary>
public interface IInputHistoryService
{
    /// <summary>
    /// 获取最近的输入历史
    /// </summary>
    Task<List<InputHistoryItem>> GetRecentAsync(int limit = 50);
    
    /// <summary>
    /// 搜索输入历史
    /// </summary>
    Task<List<InputHistoryItem>> SearchAsync(string searchText, int limit = 10);
    
    /// <summary>
    /// 保存输入历史
    /// </summary>
    Task<bool> SaveAsync(string text);
    
    /// <summary>
    /// 清空输入历史
    /// </summary>
    Task<bool> ClearAsync();
}

/// <summary>
/// 输入历史项
/// </summary>
public class InputHistoryItem
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
