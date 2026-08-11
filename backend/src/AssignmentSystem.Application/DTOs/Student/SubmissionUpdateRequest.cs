namespace AssignmentSystem.Application.DTOs.Student;

public record SubmissionUpdateRequest(string? Content, string? AttachmentUrl) : ISubmissionPayload;