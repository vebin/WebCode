using WebCodeCli.Domain.Repositories.Base;

namespace WebCodeCli.Domain.Repositories.Base.Template;

/// <summary>
/// 提示模板仓储接口
/// </summary>
public interface IPromptTemplateRepository : IRepository<PromptTemplateEntity>
{
    /// <summary>
    /// 根据用户名获取所有模板
    /// </summary>
    Task<List<PromptTemplateEntity>> GetByUsernameAsync(string username);
    
    /// <summary>
    /// 根据用户名和分类获取模板
    /// </summary>
    Task<List<PromptTemplateEntity>> GetByUsernameAndCategoryAsync(string username, string category);
    
    /// <summary>
    /// 根据ID和用户名获取模板
    /// </summary>
    Task<PromptTemplateEntity?> GetByIdAndUsernameAsync(string id, string username);
    
    /// <summary>
    /// 根据ID和用户名删除模板
    /// </summary>
    Task<bool> DeleteByIdAndUsernameAsync(string id, string username);
    
    /// <summary>
    /// 获取用户的收藏模板
    /// </summary>
    Task<List<PromptTemplateEntity>> GetFavoritesByUsernameAsync(string username);
    
    /// <summary>
    /// 获取用户的自定义模板
    /// </summary>
    Task<List<PromptTemplateEntity>> GetCustomByUsernameAsync(string username);
}
