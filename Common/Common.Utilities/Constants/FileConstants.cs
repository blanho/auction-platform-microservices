namespace Common.Utilities.Constants;

public static class FileConstants
{
    public static class ContentTypes
    {
        public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ExcelLegacy = "application/vnd.ms-excel";
        public const string Csv = "text/csv";
        public const string Json = "application/json";
        public const string Pdf = "application/pdf";
        public const string Xml = "application/xml";
        public const string Zip = "application/zip";
        public const string OctetStream = "application/octet-stream";
    }

    public static class Extensions
    {
        public const string Excel = ".xlsx";
        public const string ExcelLegacy = ".xls";
        public const string Csv = ".csv";
        public const string Json = ".json";
        public const string Pdf = ".pdf";
        public const string Xml = ".xml";
        public const string Zip = ".zip";
    }

    public static class AllowedExtensions
    {
        public static readonly string[] Excel = { ".xlsx", ".xls" };
        public static readonly string[] Images = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" };
        public static readonly string[] Documents = { ".pdf", ".doc", ".docx", ".txt", ".rtf" };
        public static readonly string[] Archives = { ".zip", ".rar", ".7z", ".tar", ".gz" };
    }
}
