namespace WebCodeCli.Domain.Domain.Model;

/// <summary>
/// 输出结果区域（Tab=输出结果）的持久化状态。
/// </summary>
public class OutputPanelState
{
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 非 JSONL 模式下的原始输出（会作为 Markdown 渲染）。
    /// JSONL 模式下也可能用于展示最终 assistant 文本。
    /// </summary>
    public string? RawOutput { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用 JSONL 流式输出模式。
    /// </summary>
    public bool IsJsonlOutputActive { get; set; }

    /// <summary>
    /// JSONL 模式下的当前 thread id。
    /// </summary>
    public string ActiveThreadId { get; set; } = string.Empty;

    /// <summary>
    /// JSONL 事件列表（用于“命令执行/工具调用”等卡片展示）。
    /// </summary>
    public List<OutputJsonlEvent> JsonlEvents { get; set; } = new();

    /// <summary>
    /// 事件 JSON 字符串（用于数据库存储）
    /// </summary>
    public string? EventsJson { get; set; }

    /// <summary>
    /// 显示的事件数量
    /// </summary>
    public int DisplayedEventCount { get; set; } = 20;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class OutputJsonlEvent
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public OutputJsonlUsageDetail? Usage { get; set; }
    public bool IsUnknown { get; set; }
}

public class OutputJsonlUsageDetail
{
    public long? InputTokens { get; set; }
    public long? CachedInputTokens { get; set; }
    public long? OutputTokens { get; set; }
}
/// <summary>
/// 输出事件组（用于 OutputResultPanel 组件）
/// </summary>
public class OutputEventGroup
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // "command_execution" | "tool_call" | "single"
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCollapsible { get; set; }
    public List<OutputEvent> Items { get; set; } = new();
}

/// <summary>
/// 输出事件（用于 OutputResultPanel 组件）
/// </summary>
public class OutputEvent
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ItemType { get; set; }
    public TokenUsage? Usage { get; set; }
}

/// <summary>
/// Token 使用情况
/// </summary>
public class TokenUsage
{
    public int? InputTokens { get; set; }
    public int? CachedInputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
}