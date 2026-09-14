using System.ComponentModel.DataAnnotations;

namespace SO17.Application.Common;

public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);

public sealed class PageQuery
{
    [Range(1, 100000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 12;
    [StringLength(100)] public string? Search { get; set; }
    public string Term => Search?.Trim() ?? "";
    public int Skip => (Page - 1) * PageSize;
}
