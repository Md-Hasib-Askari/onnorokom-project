namespace AssignmentSystem.Application.Common.Pagination;

/// <summary>
/// One client's page-size request. Absent, a negative, or a zero limit falls back to the default;
/// anything above the cap is clamped. The validation is deliberately quiet because the contract is
/// "give me a sensible page" rather than "reject my request".
/// </summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public PageRequest(int? limit)
    {
        Limit = NormalizeLimit(limit);
    }

    /// <summary>The normalized page size, always between 1 and <see cref="MaxPageSize"/>.</summary>
    public int Limit { get; }

    private static int NormalizeLimit(int? limit)
    {
        if (limit is null or <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(limit.Value, MaxPageSize);
    }
}
