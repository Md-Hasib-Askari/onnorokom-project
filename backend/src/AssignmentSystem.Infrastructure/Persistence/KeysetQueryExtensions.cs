using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

/// <summary>
/// Keyset pagination over an <c>IQueryable</c>: fetch <c>limit + 1</c> rows after the cursor key,
/// keeping only <c>limit</c> of them. The extra row is what proves <c>HasMore</c> upstream; callers
/// pass the whole row list to <c>PagedResult.FromRows</c>, which strips it and builds the cursor.
/// Sorting is fixed server-side per endpoint; these helpers only ever filter to the slice after
/// the cursor and order by the keyset so the slice is stable.
/// </summary>
public static class KeysetQueryExtensions
{
    /// <summary>Keyset on <c>(DateTimeOffset key, Id)</c>, ascending or descending.</summary>
    public static IQueryable<T> ApplyKeysetPaging<T>(
        this IQueryable<T> source,
        Expression<Func<T, DateTimeOffset>> keySelector,
        DateTimeOffset? afterKey,
        Guid? afterId,
        bool descending,
        int limit)
        where T : class
    {
        if (afterKey is not null)
        {
            source = source.Where(BuildAfterCondition(keySelector, afterKey.Value, afterId, descending));
        }

        source = descending
            ? source.OrderByDescending(keySelector).ThenByDescending(e => EF.Property<Guid>(e, "Id"))
            : source.OrderBy(keySelector).ThenBy(e => EF.Property<Guid>(e, "Id"));

        return source.Take(limit + 1);
    }

    /// <summary>Keyset on <c>(string key, Id)</c>, ascending or descending.</summary>
    public static IQueryable<T> ApplyKeysetPaging<T>(
        this IQueryable<T> source,
        Expression<Func<T, string>> keySelector,
        string? afterKey,
        Guid? afterId,
        bool descending,
        int limit)
        where T : class
    {
        if (afterKey is not null)
        {
            source = source.Where(BuildAfterCondition(keySelector, afterKey, afterId, descending));
        }

        source = descending
            ? source.OrderByDescending(keySelector).ThenByDescending(e => EF.Property<Guid>(e, "Id"))
            : source.OrderBy(keySelector).ThenBy(e => EF.Property<Guid>(e, "Id"));

        return source.Take(limit + 1);
    }

    /// <summary>Keyset on <c>(string key1, string key2, Id)</c>, ascending or descending.</summary>
    public static IQueryable<T> ApplyKeysetPaging<T>(
        this IQueryable<T> source,
        Expression<Func<T, string>> firstKeySelector,
        Expression<Func<T, string>> secondKeySelector,
        string? afterFirstKey,
        string? afterSecondKey,
        Guid? afterId,
        bool descending,
        int limit)
        where T : class
    {
        if (afterFirstKey is not null)
        {
            source = source.Where(BuildAfterCondition(
                firstKeySelector, secondKeySelector, afterFirstKey, afterSecondKey, afterId, descending));
        }

        source = descending
            ? source.OrderByDescending(firstKeySelector).ThenByDescending(secondKeySelector).ThenByDescending(e => EF.Property<Guid>(e, "Id"))
            : source.OrderBy(firstKeySelector).ThenBy(secondKeySelector).ThenBy(e => EF.Property<Guid>(e, "Id"));

        return source.Take(limit + 1);
    }

    private static Expression<Func<T, bool>> BuildAfterCondition<T>(
        Expression<Func<T, DateTimeOffset>> keySelector,
        DateTimeOffset afterKey,
        Guid? afterId,
        bool descending)
    {
        var parameter = keySelector.Parameters[0];
        var keyBody = keySelector.Body;
        var idBody = Expression.Property(parameter, "Id");

        Expression comparison = descending
            ? Expression.LessThan(keyBody, Expression.Constant(afterKey))
            : Expression.GreaterThan(keyBody, Expression.Constant(afterKey));
        var equality = Expression.Equal(keyBody, Expression.Constant(afterKey));

        Expression body;
        if (afterId is null)
        {
            body = comparison;
        }
        else
        {
            Expression idComparison;
            if (descending)
            {
                idComparison = Expression.LessThan(idBody, Expression.Constant(afterId.Value));
            }
            else
            {
                idComparison = Expression.GreaterThan(idBody, Expression.Constant(afterId.Value));
            }
            body = Expression.OrElse(comparison, Expression.AndAlso(equality, idComparison));
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static readonly System.Reflection.MethodInfo StringCompareMethod =
        typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])
            ?? throw new InvalidOperationException("string.Compare(string, string) was not found.");

    private static Expression<Func<T, bool>> BuildAfterCondition<T>(
        Expression<Func<T, string>> keySelector,
        string afterKey,
        Guid? afterId,
        bool descending)
    {
        var parameter = keySelector.Parameters[0];
        var keyBody = keySelector.Body;
        var idBody = Expression.Property(parameter, "Id");

        // Strings have no >/< operators in C#, and instance CompareTo is not
        // translatable; static string.Compare is translated to a SQL string
        // comparison, which matches the collation used by ORDER BY.
        Expression comparison = descending
            ? Expression.LessThan(StringCompare(keyBody, afterKey), Expression.Constant(0))
            : Expression.GreaterThan(StringCompare(keyBody, afterKey), Expression.Constant(0));
        var equality = Expression.Equal(keyBody, Expression.Constant(afterKey));

        Expression body;
        if (afterId is null)
        {
            body = comparison;
        }
        else
        {
            Expression idComparison;
            if (descending)
            {
                idComparison = Expression.LessThan(idBody, Expression.Constant(afterId.Value));
            }
            else
            {
                idComparison = Expression.GreaterThan(idBody, Expression.Constant(afterId.Value));
            }
            body = Expression.OrElse(comparison, Expression.AndAlso(equality, idComparison));
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression StringCompare(Expression value, string after)
    {
        return Expression.Call(StringCompareMethod, value, Expression.Constant(after));
    }

    private static Expression<Func<T, bool>> BuildAfterCondition<T>(
        Expression<Func<T, string>> firstKeySelector,
        Expression<Func<T, string>> secondKeySelector,
        string afterFirstKey,
        string? afterSecondKey,
        Guid? afterId,
        bool descending)
    {
        var parameter = firstKeySelector.Parameters[0];
        var firstBody = firstKeySelector.Body;
        var secondBody = secondKeySelector.Body;
        var idBody = Expression.Property(parameter, "Id");

        Expression firstComparison = descending
            ? Expression.LessThan(StringCompare(firstBody, afterFirstKey), Expression.Constant(0))
            : Expression.GreaterThan(StringCompare(firstBody, afterFirstKey), Expression.Constant(0));
        var firstEquality = Expression.Equal(firstBody, Expression.Constant(afterFirstKey));

        Expression secondAfter = Expression.Constant(false);
        if (afterSecondKey is not null)
        {
            Expression secondComparison = descending
                ? Expression.LessThan(StringCompare(secondBody, afterSecondKey), Expression.Constant(0))
                : Expression.GreaterThan(StringCompare(secondBody, afterSecondKey), Expression.Constant(0));
            var secondEquality = Expression.Equal(secondBody, Expression.Constant(afterSecondKey));

            Expression idComparison = afterId is null
                ? Expression.Constant(true)
                : (descending
                    ? Expression.LessThan(idBody, Expression.Constant(afterId.Value))
                    : Expression.GreaterThan(idBody, Expression.Constant(afterId.Value)));

            secondAfter = Expression.OrElse(secondComparison, Expression.AndAlso(secondEquality, idComparison));
        }

        // after (k1 > a1) OR (k1 == a1 AND ((k2 > a2) OR (k2 == a2 AND id > aid)))
        var firstAfter = Expression.AndAlso(firstEquality, secondAfter);
        var body = Expression.OrElse(firstComparison, firstAfter);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
