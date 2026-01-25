using SqlSugar;

namespace WebCodeCli.Domain.Repositories.Base.SessionOutput;

/// <summary>
/// 会话输出状态实体
/// </summary>
[SugarTable("SessionOutput")]
public class SessionOutputEntity
{
    /// <summary>
    /// 会话ID（主键）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, Length = 64)]
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名（多用户支持）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Username { get; set; } = "default";
    
    /// <summary>
    /// 原始输出内容
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? RawOutput { get; set; }
    
    /// <summary>
    /// 事件列表（JSON格式）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? EventsJson { get; set; }
    
    /// <summary>
    /// 显示的事件数量
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public int DisplayedEventCount { get; set; } = 20;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
