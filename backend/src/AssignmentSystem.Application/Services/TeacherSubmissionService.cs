using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class TeacherSubmissionService(
    ISubmissionRepository submissionRepository,
    IAssignmentRepository assignmentRepository,
    IProfileRepository profileRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ITeacherSubmissionService
{
    public async Task<PagedResult<TeacherSubmissionDto>> GetForAssignmentAsync(
        Guid assignmentId,
        PageRequest page,
        string? cursor,
        CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAssignmentAsync(assignmentId, ct);

        string? afterFullName = null;
        Guid? afterId = null;
        if (cursor is not null)
        {
            (afterFullName, afterId) = CursorCodec.DecodeString(cursor);
        }

        var submissions = await submissionRepository.GetPageByAssignmentAsync(
            assignmentId, page.Limit, afterFullName, afterId, ct);

        var profiles = await profileRepository.GetStudentsByUserIdsAsync(
            submissions.Items.Select(s => s.StudentId), ct);
        var rollNumberByStudentId = profiles.ToDictionary(p => p.AuthUserId, p => p.RollNumber);

        return submissions.Map(
            s => ToDto(s, assignment, rollNumberByStudentId.GetValueOrDefault(s.StudentId)));
    }

    public async Task<TeacherSubmissionDto> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken ct = default)
    {
        var (submission, assignment) = await LoadOwnedSubmissionAsync(submissionId, ct);

        if (request.Marks > assignment.MaxMarks)
        {
            throw new DomainException(
                $"Marks cannot exceed the assignment maximum of {assignment.MaxMarks}.");
        }

        submission.Grade(request.Marks, request.Feedback, currentUser.GetRequiredUserId());
        await submissionRepository.UpdateAsync(submission, ct);
        await unitOfWork.SaveAsync(ct);

        return await ToDtoAsync(submission, assignment, ct);
    }

    public async Task<TeacherSubmissionDto> ReturnAsync(Guid submissionId, CancellationToken ct = default)
    {
        var (submission, assignment) = await LoadOwnedSubmissionAsync(submissionId, ct);

        if (submission.Status != SubmissionStatus.Graded)
        {
            throw new DomainException("Only a graded submission can be returned for revision.");
        }

        submission.ReturnForRevision();
        await submissionRepository.UpdateAsync(submission, ct);
        await unitOfWork.SaveAsync(ct);

        return await ToDtoAsync(submission, assignment, ct);
    }

    private async Task<Assignment> LoadOwnedAssignmentAsync(Guid assignmentId, CancellationToken ct)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId, ct)
            ?? throw new EntityNotFoundException($"Assignment with id {assignmentId} was not found.");

        AssignmentGuards.EnsureOwnedBy(assignment, currentUser.GetRequiredUserId());
        return assignment;
    }

    /// <summary>
    /// A submission is only reachable through its assignment, so ownership of the parent is the
    /// only check needed: there is no separate notion of owning a submission.
    /// </summary>
    private async Task<(Submission Submission, Assignment Assignment)> LoadOwnedSubmissionAsync(
        Guid submissionId,
        CancellationToken ct)
    {
        var submission = await submissionRepository.GetByIdAsync(submissionId, ct)
            ?? throw new EntityNotFoundException($"Submission with id {submissionId} was not found.");

        var assignment = await LoadOwnedAssignmentAsync(submission.AssignmentId, ct);
        return (submission, assignment);
    }

    private async Task<TeacherSubmissionDto> ToDtoAsync(Submission submission, Assignment assignment, CancellationToken ct)
    {
        var profile = await profileRepository.GetStudentByUserIdAsync(submission.StudentId, ct);
        return ToDto(submission, assignment, profile?.RollNumber);
    }

    private static TeacherSubmissionDto ToDto(Submission submission, Assignment assignment, string? rollNumber) => new(
        submission.Id,
        submission.StudentId,
        submission.Student?.FullName,
        rollNumber,
        submission.Content,
        submission.AttachmentUrl,
        submission.Status,
        assignment.IsPastDeadline(submission.SubmittedAt),
        submission.Marks,
        submission.Feedback,
        submission.SubmittedAt,
        submission.GradedAt);
}