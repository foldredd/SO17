using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SO17.Application.Common;

public static partial class Validation
{
    public static void Ensure(object request) => Validator.ValidateObject(request, new ValidationContext(request), true);

    public static string Phone(string value)
    {
        var normalized = PhoneSeparators().Replace(value.Trim(), "");
        if (!SupportedCountryPhone().IsMatch(normalized))
            throw new AppException(400, "Оберіть країну зі списку та введіть номер без коду країни.");
        return normalized;
    }

    public static void Version(Guid current, Guid expected)
    {
        if (expected == Guid.Empty || current != expected)
            throw AppException.Conflict("Запис уже змінено. Оновіть сторінку та повторіть дію.");
    }

    [GeneratedRegex(@"[\s()\-]")] private static partial Regex PhoneSeparators();
    [GeneratedRegex(@"^\+(?:380|44|1|48|49)[0-9]{7,10}$")] private static partial Regex SupportedCountryPhone();
}
