using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WebCodeCli.Domain.Domain.Service;

namespace WebCodeCli.Components;

public partial class AutoCompleteDropdown : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IInputHistoryService InputHistoryService { get; set; } = default!;

    [Parameter] public EventCallback<string> OnSuggestionSelected { get; set; }
    [Parameter] public string TargetElementId { get; set; } = "input-message";

    private List<Suggestion> _suggestions = new();
    private bool _isVisible = false;
    private int _selectedIndex = 0;
    private string _searchText = string.Empty;
    private bool _showTemplateHint = false;
    private string _positionLeft = "0px";
    private string _positionTop = "0px";
    private string _positionWidth = "400px";

    public async Task ShowSuggestions(string searchText, List<Suggestion> suggestions)
    {
        _searchText = searchText;
        _suggestions = suggestions.Take(10).ToList(); // 最多显示10条
        _selectedIndex = 0;
        _isVisible = _suggestions.Any();
        _showTemplateHint = !searchText.Contains("@");

        if (_isVisible)
        {
            await UpdatePosition();
        }

        StateHasChanged();
    }

    public void Hide()
    {
        _isVisible = false;
        _suggestions.Clear();
        _selectedIndex = 0;
        StateHasChanged();
    }

    public async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (!_isVisible || !_suggestions.Any())
        {
            return;
        }

        switch (e.Key)
        {
            case "ArrowDown":
                _selectedIndex = (_selectedIndex + 1) % _suggestions.Count;
                StateHasChanged();
                break;

            case "ArrowUp":
                _selectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : _suggestions.Count - 1;
                StateHasChanged();
                break;

            case "Enter":
                if (_selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
                {
                    await SelectSuggestion(_selectedIndex);
                }
                break;

            case "Escape":
                Hide();
                break;
        }
    }

    private async Task SelectSuggestion(int index)
    {
        if (index < 0 || index >= _suggestions.Count)
        {
            return;
        }

        var suggestion = _suggestions[index];
        
        // 更新使用次数
        suggestion.UsageCount++;

        // 保存到历史记录
        if (suggestion.Type == SuggestionType.Template)
        {
            await SaveToHistory(suggestion.Text);
        }

        // 触发选择事件
        if (OnSuggestionSelected.HasDelegate)
        {
            await OnSuggestionSelected.InvokeAsync(suggestion.Text);
        }

        Hide();
    }

    private async Task SaveToHistory(string text)
    {
        try
        {
            await InputHistoryService.SaveAsync(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存输入历史失败: {ex.Message}");
        }
    }

    private async Task UpdatePosition()
    {
        try
        {
            // 获取输入框的位置和尺寸
            var rect = await JSRuntime.InvokeAsync<ElementRect>("eval", 
                $@"(() => {{
                    const el = document.getElementById('{TargetElementId}');
                    if (!el) return null;
                    const rect = el.getBoundingClientRect();
                    return {{
                        left: rect.left,
                        top: rect.bottom + window.scrollY,
                        width: rect.width
                    }};
                }})()");

            if (rect != null)
            {
                _positionLeft = $"{rect.Left}px";
                _positionTop = $"{rect.Top + 5}px"; // 5px 间距
                _positionWidth = $"{Math.Max(rect.Width, 300)}px";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新位置失败: {ex.Message}");
        }
    }

    private string GetPositionStyle()
    {
        return $"left: {_positionLeft}; top: {_positionTop}; width: {_positionWidth};";
    }

    private string GetSuggestionClass(bool isSelected)
    {
        return isSelected
            ? "bg-gradient-to-r from-blue-50 to-indigo-50 border-l-4 border-blue-500"
            : "hover:bg-gray-50 border-l-4 border-transparent";
    }

    private string GetTypeTagClass(SuggestionType type)
    {
        return type == SuggestionType.History
            ? "bg-gray-100 text-gray-600"
            : "bg-purple-100 text-purple-600";
    }

    private string GetTypeLabel(SuggestionType type)
    {
        return type == SuggestionType.History ? "历史" : "模板";
    }

    private MarkupString HighlightMatch(string text, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new MarkupString(text);
        }

        var index = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return new MarkupString(text);
        }

        var before = text.Substring(0, index);
        var match = text.Substring(index, searchText.Length);
        var after = text.Substring(index + searchText.Length);

        var highlighted = $"{before}<mark class=\"bg-yellow-200 text-gray-900 font-semibold\">{match}</mark>{after}";
        return new MarkupString(highlighted);
    }

    private class ElementRect
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
    }
}

public class Suggestion
{
    public string Text { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SuggestionType Type { get; set; }
    public int UsageCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public enum SuggestionType
{
    History,
    Template
}

