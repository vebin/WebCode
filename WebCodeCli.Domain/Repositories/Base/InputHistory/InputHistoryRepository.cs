using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.InputHistory;

/// <summary>
/// 输入历史仓储实现
/// </summary>
[ServiceDescription(typeof(IInputHistoryRepository), ServiceLifetime.Scoped)]
public class InputHistoryRepository : Repository<InputHistoryEntity>, IInputHistoryRepository
{
    public InputHistoryRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名获取最近的输入历史
    /// </summary>
    public async Task<List<InputHistoryEntity>> GetRecentByUsernameAsync(string username, int limit = 50)
    {
        return await GetDB().Queryable<InputHistoryEntity>()
            .Where(x => x.Username == username)
            .OrderBy(x => x.Timestamp, OrderByType.Desc)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 根据用户名搜索输入历史
    /// </summary>
    public async Task<List<InputHistoryEntity>> SearchByUsernameAsync(string username, string searchText, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<InputHistoryEntity>();
        
        return await GetDB().Queryable<InputHistoryEntity>()
            .Where(x => x.Username == username && x.Text != null && x.Text.Contains(searchText))
            .OrderBy(x => x.Timestamp, OrderByType.Desc)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 根据用户名清空输入历史
    /// </summary>
    public async Task<bool> ClearByUsernameAsync(string username)
    {
        return await DeleteAsync(x => x.Username == username);
    }

    /// <summary>
    /// 保存输入历史
    /// </summary>
    public async Task<bool> SaveInputAsync(string username, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        
        var entity = new InputHistoryEntity
        {
            Username = username,
            Text = text.Trim(),
            Timestamp = DateTime.Now
        };
        
        return await InsertAsync(entity);
    }
}
