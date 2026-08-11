namespace AssignmentSystem.Application.DTOs.Student;

public record SubmissionCreateRequest(string? Content, string? AttachmentUrl) : ISubmissionPayload;