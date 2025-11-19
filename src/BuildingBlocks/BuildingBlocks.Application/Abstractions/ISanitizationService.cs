namespace BuildingBlocks.Application.Abstractions;

public interface ISanitizationService
{

    string SanitizeHtml(string? html);

    string SanitizeText(string? text);
}
