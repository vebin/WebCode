using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.SessionOutput;

/// <summary>
/// 会话输出仓储实现
/// </summary>
[ServiceDescription(typeof(ISessionOutputRepository), ServiceLifetime.Scoped)]
public class SessionOutputRepository : Repository<SessionOutputEntity>, ISessionOutputRepository
{
    public SessionOutputRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据会话ID获取输出状态
    /// </summary>
    public async Task<SessionOutputEntity?> GetBySessionIdAsync(string sessionId)
    {
        return await GetFirstAsync(x => x.SessionId == sessionId);
    }

    /// <summary>
    /// 根据会话ID和用户名获取输出状态
    /// </summary>
    public async Task<SessionOutputEntity?> GetBySessionIdAndUsernameAsync(string sessionId, string username)
    {
        return await GetFirstAsync(x => x.SessionId == sessionId && x.Username == username);
    }

    /// <summary>
    /// 根据会话ID删除输出状态
    /// </summary>
    public async Task<bool> DeleteBySessionIdAsync(string sessionId)
    {
        return await DeleteAsync(x => x.SessionId == sessionId);
    }

    /// <summary>
    /// 保存或更新输出状态
    /// </summary>
    public async Task<bool> SaveOrUpdateAsync(SessionOutputEntity entity)
    {
        entity.UpdatedAt = DateTime.Now;
        
        // 先检查是否存在
        var existing = await GetBySessionIdAsync(entity.SessionId);
        if (existing != null)
        {
            // 更新现有记录
            return await UpdateAsync(entity);
        }
        else
        {
            // 插入新记录
            return await InsertAsync(entity);
        }
    }
}
