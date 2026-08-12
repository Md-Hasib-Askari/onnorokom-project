using AssignmentSystem.Application.Common.Pagination;

namespace AssignmentSystem.Tests;

public class PagedResultTests
{
    [Fact]
    public void FromRows_WithFewerThanLimitRows_HasNoCursor()
    {
        var result = PagedResult<int>.FromRows([1, 2, 3], limit: 5, encodeCursor: _ => "cursor");

        Assert.Equal([1, 2, 3], result.Items);
        Assert.Null(result.NextCursor);
        Assert.False(result.HasMore);
    }

    [Fact]
    public void FromRows_WithExactlyLimitRows_HasNoCursor()
    {
        var result = PagedResult<int>.FromRows([1, 2, 3], limit: 3, encodeCursor: _ => "cursor");

        Assert.Equal(3, result.Items.Count);
        Assert.Null(result.NextCursor);
        Assert.False(result.HasMore);
    }

    [Fact]
    public void FromRows_WithExtraRow_ExposesCursorAndHasMore()
    {
        var result = PagedResult<int>.FromRows([1, 2, 3, 4], limit: 3, encodeCursor: last => $"cursor:{last}");

        Assert.Equal([1, 2, 3], result.Items);
        Assert.Equal("cursor:3", result.NextCursor);
        Assert.True(result.HasMore);
    }

    [Fact]
    public void FromAll_NeverHasCursor()
    {
        var result = PagedResult<int>.FromAll([1, 2]);

        Assert.Equal(2, result.Items.Count);
        Assert.Null(result.NextCursor);
        Assert.False(result.HasMore);
    }

    [Fact]
    public void Map_TransformsItemsAndKeepsEnvelope()
    {
        var result = PagedResult<int>.FromRows([1, 2, 3, 4], limit: 3, encodeCursor: last => $"cursor:{last}");

        var mapped = result.Map(value => value * 10);

        Assert.Equal([10, 20, 30], mapped.Items);
        Assert.Equal("cursor:3", mapped.NextCursor);
        Assert.True(mapped.HasMore);
    }
}
