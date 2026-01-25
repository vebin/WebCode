using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebCodeCli.Domain.Common.Extensions;
using WebCodeCli.Domain.Domain.Model;
using WebCodeCli.Domain.Repositories.Base.Template;

namespace WebCodeCli.Domain.Domain.Service;

/// <summary>
/// 提示模板服务实现
/// </summary>
[ServiceDescription(typeof(IPromptTemplateService), ServiceLifetime.Scoped)]
public class PromptTemplateService : IPromptTemplateService
{
    private readonly IPromptTemplateRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<PromptTemplateService> _logger;

    public PromptTemplateService(
        IPromptTemplateRepository repository,
        IUserContextService userContextService,
        ILogger<PromptTemplateService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有模板
    /// </summary>
    public async Task<List<PromptTemplate>> GetAllAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetByUsernameAsync(username);
            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模板列表失败");
            return new List<PromptTemplate>();
        }
    }

    /// <summary>
    /// 根据分类获取模板
    /// </summary>
    public async Task<List<PromptTemplate>> GetByCategoryAsync(string category)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetByUsernameAndCategoryAsync(username, category);
            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据分类获取模板失败: {Category}", category);
            return new List<PromptTemplate>();
        }
    }

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    public async Task<PromptTemplate?> GetByIdAsync(string id)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entity = await _repository.GetByIdAndUsernameAsync(id, username);
            return entity != null ? MapToModel(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模板失败: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// 保存模板
    /// </summary>
    public async Task<bool> SaveAsync(PromptTemplate template)
    {
        try
        {
            if (template == null || string.IsNullOrWhiteSpace(template.Id))
            {
                _logger.LogWarning("保存模板失败: 无效的模板数据");
                return false;
            }

            var username = _userContextService.GetCurrentUsername();
            var entity = MapToEntity(template);
            entity.Username = username;
            entity.UpdatedAt = DateTime.Now;

            return await _repository.InsertOrUpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存模板失败: {Id}", template?.Id);
            return false;
        }
    }

    /// <summary>
    /// 删除模板
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            return await _repository.DeleteByIdAndUsernameAsync(id, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除模板失败: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// 获取收藏模板
    /// </summary>
    public async Task<List<PromptTemplate>> GetFavoritesAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var entities = await _repository.GetFavoritesByUsernameAsync(username);
            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取收藏模板失败");
            return new List<PromptTemplate>();
        }
    }

    /// <summary>
    /// 初始化默认模板
    /// </summary>
    public async Task<bool> InitDefaultTemplatesAsync()
    {
        try
        {
            var username = _userContextService.GetCurrentUsername();
            var existingTemplates = await _repository.GetByUsernameAsync(username);
            
            if (existingTemplates.Any())
            {
                _logger.LogDebug("用户已有模板，跳过初始化默认模板");
                return true;
            }

            _logger.LogInformation("开始初始化默认模板...");

            var defaultTemplates = GetDefaultTemplates();
            foreach (var template in defaultTemplates)
            {
                template.Username = username;
                await _repository.InsertAsync(template);
            }

            _logger.LogInformation("成功初始化 {Count} 个默认模板", defaultTemplates.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化默认模板失败");
            return false;
        }
    }

    private List<PromptTemplateEntity> GetDefaultTemplates()
    {
        var now = DateTime.Now;
        return new List<PromptTemplateEntity>
        {
            new()
            {
                Id = "optimize-code",
                Title = "优化代码",
                Content = "请优化以下代码的性能和可读性，并说明优化的原因：\n\n{{code}}",
                Category = "optimization",
                Icon = "🔧",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code" }),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = "add-comments",
                Title = "添加注释",
                Content = "请为以下代码添加详细的中文注释，包括函数说明、参数说明和关键逻辑说明：\n\n{{code}}",
                Category = "documentation",
                Icon = "📝",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code" }),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = "fix-bug",
                Title = "修复 Bug",
                Content = "请帮我分析并修复以下代码中的 Bug，并解释问题原因：\n\n{{code}}\n\n错误信息：{{error}}",
                Category = "debugging",
                Icon = "🐛",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code", "error" }),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = "refactor-code",
                Title = "重构代码",
                Content = "请重构以下代码，提高代码质量和可维护性，遵循 SOLID 原则：\n\n{{code}}",
                Category = "refactoring",
                Icon = "🔄",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code" }),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = "generate-tests",
                Title = "生成测试",
                Content = "请为以下代码生成单元测试用例，使用 {{framework}} 测试框架：\n\n{{code}}",
                Category = "testing",
                Icon = "🧪",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code", "framework" }),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = "code-review",
                Title = "代码审查",
                Content = "请进行代码审查，指出潜在问题和改进建议，包括：\n1. 代码质量\n2. 安全性\n3. 性能\n4. 可维护性\n\n{{code}}",
                Category = "review",
                Icon = "👁️",
                IsCustom = false,
                IsFavorite = false,
                VariablesJson = JsonSerializer.Serialize(new[] { "code" }),
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }

    private PromptTemplate MapToModel(PromptTemplateEntity entity)
    {
        List<string>? variables = null;
        if (!string.IsNullOrEmpty(entity.VariablesJson))
        {
            try
            {
                variables = JsonSerializer.Deserialize<List<string>>(entity.VariablesJson);
            }
            catch { }
        }

        return new PromptTemplate
        {
            Id = entity.Id,
            Title = entity.Title ?? string.Empty,
            Content = entity.Content ?? string.Empty,
            Category = entity.Category ?? string.Empty,
            Icon = entity.Icon ?? string.Empty,
            IsCustom = entity.IsCustom,
            IsFavorite = entity.IsFavorite,
            Variables = variables ?? new List<string>(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private PromptTemplateEntity MapToEntity(PromptTemplate model)
    {
        return new PromptTemplateEntity
        {
            Id = model.Id,
            Title = model.Title,
            Content = model.Content,
            Category = model.Category,
            Icon = model.Icon,
            IsCustom = model.IsCustom,
            IsFavorite = model.IsFavorite,
            VariablesJson = model.Variables != null ? JsonSerializer.Serialize(model.Variables) : null,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
