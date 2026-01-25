using SqlSugar;

namespace WebCodeCli.Domain.Repositories.Base.InputHistory;

/// <summary>
/// 输入历史实体
/// </summary>
[SugarTable("InputHistory")]
public class InputHistoryEntity
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    
    /// <summary>
    /// 用户名（多用户支持）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Username { get; set; } = "default";
    
    /// <summary>
    /// 输入文本
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Text { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
