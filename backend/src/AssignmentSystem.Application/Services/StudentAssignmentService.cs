using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Student;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class StudentAssignmentService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    IProfileRepository profileRepository,
    ICurrentUser currentUser) : IStudentAssignmentService
{
    public async Task<List<StudentAssignmentListItemDto>> GetMyAssignmentsAsync(CancellationToken ct = default)
    {
        var studentId = currentUser.GetRequiredUserId();
        var profile = await LoadProfileAsync(studentId, ct);

        var assignments = await assignmentRepository.GetPublishedForSectionAsync(profile.SectionId, ct);

        var submissions = await submissionRepository.GetByStudentAndAssignmentIdsAsync(
            studentId, assignments.Select(a => a.Id), ct);
        var submissionByAssignmentId = submissions.ToDictionary(s => s.AssignmentId);

        return assignments
            .Select(a => ToListItem(a, submissionByAssignmentId.GetValueOrDefault(a.Id)))
            .ToList();
    }

    public async Task<StudentAssignmentDetailDto> GetByIdAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var studentId = currentUser.GetRequiredUserId();
        var profile = await LoadProfileAsync(studentId, ct);
        var assignment = await LoadVisibleAssignmentAsync(assignmentId, profile, ct);
        var submission = await submissionRepository.GetByAssignmentAndStudentAsync(assignmentId, studentId, ct);

        return ToDetail(assignment, submission);
    }

    public async Task<StudentAssignmentDetailDto> SubmitAsync(
        Guid assignmentId,
        SubmissionCreateRequest request,
        CancellationToken ct = default)
    {
        return await SaveAsync(assignmentId, request.Content, request.AttachmentUrl, requireExisting: false, ct);
    }

    public async Task<StudentAssignmentDetailDto> UpdateSubmissionAsync(
        Guid assignmentId,
        SubmissionUpdateRequest request,
        CancellationToken ct = default)
    {
        return await SaveAsync(assignmentId, request.Content, request.AttachmentUrl, requireExisting: true, ct);
    }

    /// <summary>
    /// Submit and edit are the same write behind the same guards: there is one row per student per
    /// assignment, so the only real difference is whether a row is expected to exist already.
    /// </summary>
    private async Task<StudentAssignmentDetailDto> SaveAsync(
        Guid assignmentId,
        string? content,
        string? attachmentUrl,
        bool requireExisting,
        CancellationToken ct)
    {
        var studentId = currentUser.GetRequiredUserId();
        var profile = await LoadProfileAsync(studentId, ct);
        var assignment = await LoadVisibleAssignmentAsync(assignmentId, profile, ct);

        if (!assignment.IsAcceptingSubmissions(DateTimeOffset.UtcNow))
        {
            throw new DomainException(
                "The deadline for this assignment has passed and late submissions are not allowed.");
        }

        var submission = await submissionRepository.GetByAssignmentAndStudentAsync(assignmentId, studentId, ct);

        if (submission is null)
        {
            if (requireExisting)
            {
                throw new EntityNotFoundException("You have not submitted to this assignment yet.");
            }

            submission = Submission.Create(assignmentId, studentId, content, attachmentUrl);
            await submissionRepository.AddAsync(submission, ct);
        }
        else
        {
            if (submission.Status == SubmissionStatus.Graded)
            {
                throw new DomainException(
                    "This submission has been graded and can no longer be changed. Ask your teacher to return it for revision.");
            }

            submission.Revise(content, attachmentUrl);
            await submissionRepository.UpdateAsync(submission, ct);
        }

        return ToDetail(assignment, submission);
    }

    private async Task<StudentProfile> LoadProfileAsync(Guid studentId, CancellationToken ct)
    {
        return await profileRepository.GetStudentByUserIdAsync(studentId, ct)
               ?? throw new ForbiddenException(
                   "Your student profile is not set up yet. Ask an admin to place you in a section.");
    }

    /// <summary>
    /// A draft assignment, or one belonging to another section, is reported as not found rather than
    /// forbidden. A 403 would confirm the id is real, which is enough to probe another section's
    /// work by guessing ids; a student has no legitimate way to learn such an id in the first place.
    /// </summary>
    private async Task<Assignment> LoadVisibleAssignmentAsync(
        Guid assignmentId,
        StudentProfile profile,
        CancellationToken ct)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId, ct);

        if (assignment is null
            || assignment.Status != AssignmentStatus.Published
            || assignment.SectionId != profile.SectionId)
        {
            throw new EntityNotFoundException($"Assignment with id {assignmentId} was not found.");
        }

        return assignment;
    }

    private static StudentAssignmentListItemDto ToListItem(Assignment assignment, Submission? submission) => new(
        assignment.Id,
        assignment.Title,
        assignment.Subject?.Name,
        assignment.Teacher?.FullName,
        assignment.Deadline,
        assignment.MaxMarks,
        assignment.AllowLateSubmission,
        assignment.IsPastDeadline(DateTimeOffset.UtcNow),
        submission?.Status,
        IsLate(assignment, submission),
        submission?.Marks);

    private static StudentAssignmentDetailDto ToDetail(Assignment assignment, Submission? submission)
    {
        var isOpen = assignment.IsAcceptingSubmissions(DateTimeOffset.UtcNow);

        return new StudentAssignmentDetailDto(
            assignment.Id,
            assignment.Title,
            assignment.Subject?.Name,
            assignment.Teacher?.FullName,
            assignment.Deadline,
            assignment.MaxMarks,
            assignment.AllowLateSubmission,
            assignment.IsPastDeadline(DateTimeOffset.UtcNow),
            submission?.Status,
            IsLate(assignment, submission),
            submission?.Marks,
            assignment.Description,
            submission?.Feedback,
            submission?.AttachmentUrl,
            submission?.Content,
            submission?.SubmittedAt,
            CanSubmit: isOpen && submission is null,
            CanEdit: isOpen && submission is not null && submission.Status != SubmissionStatus.Graded);
    }

    private static bool IsLate(Assignment assignment, Submission? submission) =>
        submission is not null && assignment.IsPastDeadline(submission.SubmittedAt);
}