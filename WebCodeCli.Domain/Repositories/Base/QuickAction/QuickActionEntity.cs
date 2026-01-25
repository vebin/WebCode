using SqlSugar;

namespace WebCodeCli.Domain.Repositories.Base.QuickAction;

/// <summary>
/// 快捷操作实体
/// </summary>
[SugarTable("QuickAction")]
public class QuickActionEntity
{
    /// <summary>
    /// 快捷操作ID（主键）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名（多用户支持）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Username { get; set; } = "default";
    
    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Title { get; set; }
    
    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// 提示词
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Prompt { get; set; }
    
    /// <summary>
    /// 排序顺序
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public int Order { get; set; } = 0;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public bool IsEnabled { get; set; } = true;
}
