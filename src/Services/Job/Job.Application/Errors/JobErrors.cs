namespace Jobs.Application.Errors;

public static class JobErrors
{
    public static class Job
    {
        public static Error NotFound => Error.Create("Job.NotFound", "Job not found");
        public static Error NotFoundById(Guid id) => Error.Create("Job.NotFound", $"Job with ID {id} was not found");
        public static Error DuplicateCorrelationId(string correlationId) => Error.Create("Job.Duplicate", $"A job with correlation ID '{correlationId}' already exists");
        public static Error InvalidType(string type) => Error.Create("Job.InvalidType", $"'{type}' is not a valid job type");
        public static Error CannotCancel(string status) => Error.Create("Job.CannotCancel", $"Cannot cancel a job with status '{status}'");
        public static Error CannotRetry(string status) => Error.Create("Job.CannotRetry", $"Cannot retry a job with status '{status}'");
        public static Error StartFailed(string reason) => Error.Create("Job.StartFailed", $"Failed to start job: {reason}");
        public static Error ProcessingFailed(string reason) => Error.Create("Job.ProcessingFailed", $"Job processing failed: {reason}");
    }

    public static class JobItem
    {
        public static Error NotFound => Error.Create("JobItem.NotFound", "Job item not found");
        public static Error NotFoundById(Guid id) => Error.Create("JobItem.NotFound", $"Job item with ID {id} was not found");
        public static Error AlreadyProcessed(Guid id) => Error.Create("JobItem.AlreadyProcessed", $"Job item {id} has already been processed");
        public static Error ProcessingFailed(string reason) => Error.Create("JobItem.ProcessingFailed", $"Failed to process job item: {reason}");
    }
}
