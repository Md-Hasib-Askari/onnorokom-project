using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Pagination;

namespace AssignmentSystem.Tests;

public class CursorCodecTests
{
    [Fact]
    public void Timestamp_EncodesAndDecodesBackToTheSameKey()
    {
        var key = new DateTimeOffset(2026, 8, 10, 14, 32, 0, TimeSpan.Zero);
        var id = Guid.NewGuid();

        var (decodedTimestamp, decodedId) = CursorCodec.DecodeTimestamp(CursorCodec.Encode(key, id));

        Assert.Equal(key, decodedTimestamp);
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void Timestamp_WithSubMillisecondPrecision_SurvivesRoundTripExactly()
    {
        // The database stores microsecond precision. Truncating to milliseconds
        // would make the decoded boundary key smaller than the real key and
        // re-fetch the boundary row on the next page.
        var key = new DateTimeOffset(2026, 8, 10, 14, 32, 0, 123, TimeSpan.Zero)
            .AddTicks(4567);
        var id = Guid.NewGuid();

        var (decodedTimestamp, decodedId) = CursorCodec.DecodeTimestamp(CursorCodec.Encode(key, id));

        Assert.Equal(key, decodedTimestamp);
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void String_EncodesAndDecodesBackToTheSameKey()
    {
        var id = Guid.NewGuid();

        var (decodedValue, decodedId) = CursorCodec.DecodeString(CursorCodec.Encode("Alice", id));

        Assert.Equal("Alice", decodedValue);
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void StringPair_EncodesAndDecodesBackToTheSameKey()
    {
        var id = Guid.NewGuid();

        var (decodedFirst, decodedSecond, decodedId) =
            CursorCodec.DecodeStringPair(CursorCodec.Encode("Section A", "Alice", id));

        Assert.Equal("Section A", decodedFirst);
        Assert.Equal("Alice", decodedSecond);
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void InvalidBase64_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => CursorCodec.DecodeTimestamp("not-a-base64-cursor!!"));
    }

    [Fact]
    public void TamperedPayload_ThrowsDomainException()
    {
        // Valid base64, but not JSON, so the shape check must reject it.
        Assert.Throws<DomainException>(() => CursorCodec.DecodeTimestamp(Convert.ToBase64String("garbage"u8)));
    }

    [Fact]
    public void TimestampCursor_PassedToStringDecoder_ThrowsDomainException()
    {
        var cursor = CursorCodec.Encode(DateTimeOffset.UtcNow, Guid.NewGuid());

        Assert.Throws<DomainException>(() => CursorCodec.DecodeString(cursor));
        Assert.Throws<DomainException>(() => CursorCodec.DecodeStringPair(cursor));
    }

    [Fact]
    public void StringCursor_PassedToTimestampDecoder_SurvivesRoundTripAsExpected()
    {
        // The timestamp decoder reads the Key member, which a string cursor leaves at zero.
        var cursor = CursorCodec.Encode("Alice", Guid.NewGuid());
        var (timestamp, _) = CursorCodec.DecodeTimestamp(cursor);

        Assert.Equal(new DateTimeOffset(0, TimeSpan.Zero), timestamp);
    }
}
