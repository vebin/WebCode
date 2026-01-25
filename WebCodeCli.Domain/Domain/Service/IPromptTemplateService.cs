using WebCodeCli.Domain.Domain.Model;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 提示模板服务接口
/// </summary>
public interface IPromptTemplateService
{
    /// <summary>
    /// 获取所有模板
    /// </summary>
    Task<List<PromptTemplate>> GetAllAsync();
    
    /// <summary>
    /// 根据分类获取模板
    /// </summary>
    Task<List<PromptTemplate>> GetByCategoryAsync(string category);
    
    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    Task<PromptTemplate?> GetByIdAsync(string id);
    
    /// <summary>
    /// 保存模板
    /// </summary>
    Task<bool> SaveAsync(PromptTemplate template);
    
    /// <summary>
    /// 删除模板
    /// </summary>
    Task<bool> DeleteAsync(string id);
    
    /// <summary>
    /// 获取收藏模板
    /// </summary>
    Task<List<PromptTemplate>> GetFavoritesAsync();
    
    /// <summary>
    /// 初始化默认模板
    /// </summary>
    Task<bool> InitDefaultTemplatesAsync();
}
