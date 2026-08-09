using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISectionSubjectRepository
{
    Task<List<SectionSubject>> GetBySectionAsync(Guid sectionId, CancellationToken ct = default);
    Task<SectionSubject?> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default);
    Task AddAsync(SectionSubject sectionSubject, CancellationToken ct = default);
    Task UpdateAsync(SectionSubject sectionSubject, CancellationToken ct = default);
    Task SoftDeleteForSectionAsync(Guid sectionId, CancellationToken ct = default);
    Task SoftDeleteForSubjectAsync(Guid subjectId, CancellationToken ct = default);
}
