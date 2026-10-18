namespace BuildingBlocks.Application.Localization;

public sealed record LocalizedMessage(string Key, params string[] Arguments);
