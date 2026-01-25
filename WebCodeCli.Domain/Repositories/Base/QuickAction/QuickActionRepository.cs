using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.QuickAction;

/// <summary>
/// 快捷操作仓储实现
/// </summary>
[ServiceDescription(typeof(IQuickActionRepository), ServiceLifetime.Scoped)]
public class QuickActionRepository : Repository<QuickActionEntity>, IQuickActionRepository
{
    public QuickActionRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名获取所有快捷操作（按顺序排序）
    /// </summary>
    public async Task<List<QuickActionEntity>> GetByUsernameAsync(string username)
    {
        return await GetDB().Queryable<QuickActionEntity>()
            .Where(x => x.Username == username)
            .OrderBy(x => x.Order, OrderByType.Asc)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID和用户名获取快捷操作
    /// </summary>
    public async Task<QuickActionEntity?> GetByIdAndUsernameAsync(string id, string username)
    {
        return await GetFirstAsync(x => x.Id == id && x.Username == username);
    }

    /// <summary>
    /// 根据ID和用户名删除快捷操作
    /// </summary>
    public async Task<bool> DeleteByIdAndUsernameAsync(string id, string username)
    {
        return await DeleteAsync(x => x.Id == id && x.Username == username);
    }

    /// <summary>
    /// 根据用户名清空所有快捷操作
    /// </summary>
    public async Task<bool> ClearByUsernameAsync(string username)
    {
        return await DeleteAsync(x => x.Username == username);
    }

    /// <summary>
    /// 批量保存快捷操作（先清空再插入）
    /// </summary>
    public async Task<bool> SaveAllAsync(string username, List<QuickActionEntity> actions)
    {
        try
        {
            // 使用事务
            await GetDB().Ado.BeginTranAsync();
            
            // 先清空该用户的所有快捷操作
            await DeleteAsync(x => x.Username == username);
            
            // 插入新的快捷操作
            if (actions != null && actions.Count > 0)
            {
                foreach (var action in actions)
                {
                    action.Username = username;
                }
                await InsertRangeAsync(actions);
            }
            
            await GetDB().Ado.CommitTranAsync();
            return true;
        }
        catch
        {
            await GetDB().Ado.RollbackTranAsync();
            return false;
        }
    }
}
