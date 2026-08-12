namespace AssignmentSystem.Application.Common.Pagination;

/// <summary>
/// One page of a cursor-paginated list. <see cref="NextCursor"/> is null when the last page has
/// been reached; <see cref="HasMore"/> reports whether further pages exist without decoding the
/// cursor, which is what the Load More button keys off.
/// </summary>
public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, string? nextCursor, bool hasMore)
    {
        Items = items;
        NextCursor = nextCursor;
        HasMore = hasMore;
    }

    public IReadOnlyList<T> Items { get; }

    /// <summary>Opaque cursor to pass as <c>?cursor=</c> for the next page, or null when done.</summary>
    public string? NextCursor { get; }

    public bool HasMore { get; }

    /// <summary>
    /// Builds a page from a keyset query that fetched <c>limit + 1</c> rows. The extra row proves
    /// another page exists, and its key becomes the cursor, so the next page starts right after it.
    /// </summary>
    public static PagedResult<T> FromRows(
        IReadOnlyList<T> rows,
        int limit,
        Func<T, string> encodeCursor)
    {
        if (rows.Count <= limit)
        {
            return new PagedResult<T>(rows, null, hasMore: false);
        }

        var page = rows.Take(limit).ToList();
        var last = rows[limit - 1];
        return new PagedResult<T>(page, encodeCursor(last), hasMore: true);
    }

    /// <summary>Small-result shortcut: everything fits on one page, so there is never a cursor.</summary>
    public static PagedResult<T> FromAll(IEnumerable<T> items)
    {
        var list = items.ToList();
        return new PagedResult<T>(list, null, hasMore: false);
    }

    /// <summary>Maps the items while keeping the paging envelope (cursor, has-more) untouched.</summary>
    public PagedResult<TMapped> Map<TMapped>(Func<T, TMapped> selector)
    {
        return new PagedResult<TMapped>(
            Items.Select(selector).ToList(),
            NextCursor,
            HasMore);
    }
}
