using BuildingBlocks.Application.Abstractions;

namespace Jobs.Application.Errors;

public static class JobErrors
{
    public static class Job
    {
        public static Error NotFound => Error.Create("Job.NotFound", "Job not found");
        public static Error NotFoundById(Guid id) => Error.Create("Job.NotFound", $"Job with ID {id} was not found");
        public static Error DuplicateCorrelationId(string correlationId) => LocalizableError.Localizable("Job.Duplicate", $"A job with correlation ID '{correlationId}' already exists", correlationId);
        public static Error InvalidType(string type) => LocalizableError.Localizable("Job.InvalidType", $"'{type}' is not a valid job type", type);
        public static Error CannotCancel(string status) => LocalizableError.Localizable("Job.CannotCancel", $"Cannot cancel a job with status '{status}'", status);
        public static Error CannotRetry(string status) => LocalizableError.Localizable("Job.CannotRetry", $"Cannot retry a job with status '{status}'", status);
        public static Error StartFailed(string reason) => LocalizableError.Localizable("Job.StartFailed", $"Failed to start job: {reason}", reason);
        public static Error ProcessingFailed(string reason) => LocalizableError.Localizable("Job.ProcessingFailed", $"Job processing failed: {reason}", reason);
    }

    public static class JobItem
    {
        public static Error NotFound => Error.Create("JobItem.NotFound", "Job item not found");
        public static Error NotFoundById(Guid id) => Error.Create("JobItem.NotFound", $"Job item with ID {id} was not found");
        public static Error AlreadyProcessed(Guid id) => LocalizableError.Localizable("JobItem.AlreadyProcessed", $"Job item {id} has already been processed", id);
        public static Error ProcessingFailed(string reason) => LocalizableError.Localizable("JobItem.ProcessingFailed", $"Failed to process job item: {reason}", reason);
    }
}
