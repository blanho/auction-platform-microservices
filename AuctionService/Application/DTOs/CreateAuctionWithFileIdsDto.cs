using System.ComponentModel.DataAnnotations;

namespace AuctionService.Application.DTOs;

/// <summary>
/// DTO for creating an auction with pre-uploaded file IDs (two-phase upload)
/// Phase 1: Upload files to UtilityService /api/files/upload/batch -> get file IDs (Status=1)
/// Phase 2: Create auction with file IDs -> files confirmed (Status=2)
/// </summary>
public class CreateAuctionWithFileIdsDto
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string Make { get; set; }

    [Required]
    public required string Model { get; set; }

    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    public required string Color { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    /// <summary>
    /// Pre-uploaded file IDs from UtilityService (Status=1 Temporary)
    /// These will be confirmed and moved to permanent storage (Status=2) after auction creation
    /// </summary>
    public List<Guid>? FileIds { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int ReservePrice { get; set; }

    [Required]
    public DateTimeOffset AuctionEnd { get; set; }
}
