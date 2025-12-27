using NotificationService.Application.DTOs;

namespace NotificationService.Application.Services;

public interface INotificationOrchestrator
{
    Task<OrchestrationResult> OrchestrateAsync(
        OrchestrationRequest request,
        CancellationToken cancellationToken = default);
}
