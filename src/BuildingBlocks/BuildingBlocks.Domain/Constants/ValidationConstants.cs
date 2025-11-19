namespace BuildingBlocks.Domain.Constants;

public static class ValidationConstants
{

    public static class StringLength
    {

        public const int Short = 50;

        public const int Standard = 100;

        public const int Medium = 200;

        public const int Username = 256;

        public const int Reason = 500;

        public const int Long = 500;

        public const int Extended = 1000;

        public const int Large = 2000;

        public const int ExtraLarge = 5000;
    }

    public static class MinLength
    {

        public const int Required = 1;

        public const int ShortCode = 2;

        public const int Name = 3;
    }

    public static class NumericRange
    {

        public const int MinRating = 1;

        public const int MaxRating = 5;

        public const decimal MinPrice = 0m;

        public const decimal MaxPrice = 999999999.99m;

        public const int MinQuantity = 1;

        public const int MaxQuantity = 10000;
    }

    public static class CollectionSize
    {

        public const int MaxImages = 10;

        public const int MaxTags = 20;

        public const int MaxPageSize = 100;

        public const int DefaultPageSize = 20;

        public const int MaxBulkOperationSize = 100;

        public const int MaxImportSize = 1000;
    }

    public static class Messages
    {
        public static string MaxLength(string fieldName, int maxLength)
            => $"{fieldName} must not exceed {maxLength} characters";

        public static string MinLength(string fieldName, int minLength)
            => $"{fieldName} must be at least {minLength} characters";

        public static string Required(string fieldName)
            => $"{fieldName} is required";

        public static string InvalidFormat(string fieldName)
            => $"{fieldName} has an invalid format";

        public static string OutOfRange(string fieldName, decimal min, decimal max)
            => $"{fieldName} must be between {min} and {max}";
    }
}
