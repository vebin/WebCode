using SqlSugar;

namespace WebCodeCli.Domain.Repositories.Base.UserSetting;

/// <summary>
/// 用户设置实体
/// </summary>
[SugarTable("UserSetting")]
public class UserSettingEntity
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
    /// 设置键
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// 设置值（JSON格式支持复杂类型）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Value { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
