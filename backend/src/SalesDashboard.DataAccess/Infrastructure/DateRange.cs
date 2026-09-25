using System.Globalization;

namespace SalesDashboard.DataAccess.Infrastructure;

internal sealed class RequestValidationException(string key, string message) : Exception(message)
{
    public string Key { get; } = key;
}

public sealed record DateRange(DateOnly From, DateOnly To)
{
    /// <summary>Идентификатор бизнес-часового пояса приложения.</summary>
    public const string ZoneId = "Europe/Moscow";
    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById(ZoneId);
    public int Days => To.DayNumber - From.DayNumber + 1;
    public DateTime StartUtc => ToUtc(From);
    public DateTime EndUtc => ToUtc(To.AddDays(1));
    public DateRange Previous => new(From.AddDays(-Days), From.AddDays(-1));
    /// <summary>Переводит полночь бизнес-даты в UTC.</summary>
    public static DateTime ToUtc(DateOnly date) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), Zone);
    /// <summary>Возвращает текущую дату в бизнес-часовом поясе.</summary>
    public static DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Zone).DateTime);

    /// <summary>Разбирает и проверяет параметры периода запроса.</summary>
    public static DateRange Parse(string? from, string? to, string? preset, TimeProvider clock)
    {
        if (from is not null || to is not null)
        {
            if (preset is not null)
                throw new RequestValidationException("preset", "Укажите либо preset, либо from и to.");
            return Validate(ParseDate(from, "from"), ParseDate(to, "to"));
        }
        var today = Today(clock);
        var month = new DateOnly(today.Year, today.Month, 1);
        var result = (preset ?? "last30Days") switch
        {
            "today" => new DateRange(today, today),
            "last7Days" => new DateRange(today.AddDays(-6), today),
            "last30Days" => new DateRange(today.AddDays(-29), today),
            "thisMonth" => new DateRange(month, today),
            "lastMonth" => new DateRange(month.AddMonths(-1), month.AddDays(-1)),
            _ => throw new RequestValidationException("preset", "Неизвестный период.")
        };
        return Validate(result.From, result.To);
    }

    /// <summary>Разбирает дату в формате YYYY-MM-DD.</summary>
    public static DateOnly ParseDate(string? value, string key)
    {
        if (!DateOnly.TryParseExact(
                value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new RequestValidationException(key, "Требуется дата YYYY-MM-DD.");
        return date;
    }

    private static DateRange Validate(DateOnly from, DateOnly to)
    {
        if (from > to) throw new RequestValidationException("to", "Дата to должна быть не раньше from.");
        if (from.Year < 1900 || to.Year > 9998)
            throw new RequestValidationException("from", "Поддерживаются даты с 1900 по 9998 год.");
        var range = new DateRange(from, to);
        if (range.Days > 3660)
            throw new RequestValidationException("to", "Максимальная длительность периода — 3660 дней.");
        return range;
    }
}

