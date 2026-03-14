using BuildingBlocks.Application.Localization;

namespace BuildingBlocks.Application.Validation;

public sealed class LocalizedValidationMessages
{
    private readonly ILocalizationService _localizer;

    public LocalizedValidationMessages(ILocalizationService localizer)
    {
        _localizer = localizer;
    }

    public string Required(string fieldName)
        => _localizer[LocalizationKeys.Validation.Required, fieldName];

    public string MaxLength(string fieldName, int maxLength)
        => _localizer[LocalizationKeys.Validation.MaxLength, fieldName, maxLength];

    public string MinLength(string fieldName, int minLength)
        => _localizer[LocalizationKeys.Validation.MinLength, fieldName, minLength];

    public string InvalidEmail()
        => _localizer[LocalizationKeys.Validation.InvalidEmail];

    public string InvalidFormat(string fieldName)
        => _localizer[LocalizationKeys.Validation.InvalidFormat, fieldName];

    public string MustBePositive(string fieldName)
        => _localizer[LocalizationKeys.Validation.MustBePositive, fieldName];

    public string MustBeInRange(string fieldName, object min, object max)
        => _localizer[LocalizationKeys.Validation.MustBeInRange, fieldName, min, max];

    public string InvalidDate(string fieldName)
        => _localizer[LocalizationKeys.Validation.InvalidDate, fieldName];

    public string FutureDate(string fieldName)
        => _localizer[LocalizationKeys.Validation.FutureDate, fieldName];

    public string PastDate(string fieldName)
        => _localizer[LocalizationKeys.Validation.PastDate, fieldName];

    public string NotEmpty(string fieldName)
        => _localizer[LocalizationKeys.Validation.Required, fieldName];

    public string GreaterThan(string fieldName, object value)
        => _localizer[LocalizationKeys.Validation.MustBePositive, fieldName];

    public string LessThanOrEqual(string fieldName, object max)
        => _localizer[LocalizationKeys.Validation.MaxLength, fieldName, max];
}
