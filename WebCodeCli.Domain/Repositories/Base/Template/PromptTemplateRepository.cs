using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.Template;

/// <summary>
/// 提示模板仓储实现
/// </summary>
[ServiceDescription(typeof(IPromptTemplateRepository), ServiceLifetime.Scoped)]
public class PromptTemplateRepository : Repository<PromptTemplateEntity>, IPromptTemplateRepository
{
    public PromptTemplateRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名获取所有模板
    /// </summary>
    public async Task<List<PromptTemplateEntity>> GetByUsernameAsync(string username)
    {
        return await GetListAsync(x => x.Username == username);
    }

    /// <summary>
    /// 根据用户名和分类获取模板
    /// </summary>
    public async Task<List<PromptTemplateEntity>> GetByUsernameAndCategoryAsync(string username, string category)
    {
        return await GetListAsync(x => x.Username == username && x.Category == category);
    }

    /// <summary>
    /// 根据ID和用户名获取模板
    /// </summary>
    public async Task<PromptTemplateEntity?> GetByIdAndUsernameAsync(string id, string username)
    {
        return await GetFirstAsync(x => x.Id == id && x.Username == username);
    }

    /// <summary>
    /// 根据ID和用户名删除模板
    /// </summary>
    public async Task<bool> DeleteByIdAndUsernameAsync(string id, string username)
    {
        return await DeleteAsync(x => x.Id == id && x.Username == username);
    }

    /// <summary>
    /// 获取用户的收藏模板
    /// </summary>
    public async Task<List<PromptTemplateEntity>> GetFavoritesByUsernameAsync(string username)
    {
        return await GetListAsync(x => x.Username == username && x.IsFavorite);
    }

    /// <summary>
    /// 获取用户的自定义模板
    /// </summary>
    public async Task<List<PromptTemplateEntity>> GetCustomByUsernameAsync(string username)
    {
        return await GetListAsync(x => x.Username == username && x.IsCustom);
    }
}
