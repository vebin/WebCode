using AntSK.Domain.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WebCodeCli.Domain.Common.Extensions;

namespace WebCodeCli.Domain.Repositories.Base.UserSetting;

/// <summary>
/// 用户设置仓储实现
/// </summary>
[ServiceDescription(typeof(IUserSettingRepository), ServiceLifetime.Scoped)]
public class UserSettingRepository : Repository<UserSettingEntity>, IUserSettingRepository
{
    public UserSettingRepository(ISqlSugarClient context = null) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名和键获取设置
    /// </summary>
    public async Task<UserSettingEntity?> GetByKeyAsync(string username, string key)
    {
        return await GetFirstAsync(x => x.Username == username && x.Key == key);
    }

    /// <summary>
    /// 根据用户名获取所有设置
    /// </summary>
    public async Task<List<UserSettingEntity>> GetAllByUsernameAsync(string username)
    {
        return await GetListAsync(x => x.Username == username);
    }

    /// <summary>
    /// 设置值（不存在则创建，存在则更新）
    /// </summary>
    public async Task<bool> SetValueAsync(string username, string key, string? value)
    {
        var existing = await GetByKeyAsync(username, key);
        
        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.Now;
            return await UpdateAsync(existing);
        }
        else
        {
            var entity = new UserSettingEntity
            {
                Username = username,
                Key = key,
                Value = value,
                UpdatedAt = DateTime.Now
            };
            return await InsertAsync(entity);
        }
    }

    /// <summary>
    /// 获取设置值
    /// </summary>
    public async Task<string?> GetValueAsync(string username, string key)
    {
        var entity = await GetByKeyAsync(username, key);
        return entity?.Value;
    }

    /// <summary>
    /// 删除设置
    /// </summary>
    public async Task<bool> DeleteByKeyAsync(string username, string key)
    {
        return await DeleteAsync(x => x.Username == username && x.Key == key);
    }
}
