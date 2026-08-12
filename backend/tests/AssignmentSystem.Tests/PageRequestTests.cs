using AssignmentSystem.Application.Common.Pagination;

namespace AssignmentSystem.Tests;

public class PageRequestTests
{
    [Fact]
    public void NullLimit_FallsBackToDefault()
    {
        Assert.Equal(PageRequest.DefaultPageSize, new PageRequest(null).Limit);
    }

    [Fact]
    public void ZeroOrNegativeLimit_FallsBackToDefault()
    {
        Assert.Equal(PageRequest.DefaultPageSize, new PageRequest(0).Limit);
        Assert.Equal(PageRequest.DefaultPageSize, new PageRequest(-5).Limit);
    }

    [Fact]
    public void WithinRangeLimit_IsPreserved()
    {
        Assert.Equal(7, new PageRequest(7).Limit);
    }

    [Fact]
    public void AboveCapLimit_IsClamped()
    {
        Assert.Equal(PageRequest.MaxPageSize, new PageRequest(500).Limit);
        Assert.Equal(PageRequest.MaxPageSize, new PageRequest(PageRequest.MaxPageSize).Limit);
    }
}
