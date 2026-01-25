using SqlSugar;

namespace WebCodeCli.Domain.Repositories.Base.Template;

/// <summary>
/// 提示模板实体
/// </summary>
[SugarTable("PromptTemplate")]
public class PromptTemplateEntity
{
    /// <summary>
    /// 模板ID（主键）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名（多用户支持）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Username { get; set; } = "default";
    
    /// <summary>
    /// 模板标题
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Title { get; set; }
    
    /// <summary>
    /// 模板内容
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Content { get; set; }
    
    /// <summary>
    /// 分类
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? Category { get; set; }
    
    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// 是否为自定义模板
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public bool IsCustom { get; set; } = false;
    
    /// <summary>
    /// 是否收藏
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public bool IsFavorite { get; set; } = false;
    
    /// <summary>
    /// 变量列表（JSON数组）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? VariablesJson { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
