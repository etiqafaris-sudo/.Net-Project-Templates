using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.UploadFiles.Queries.GetAttachmentsByAppDocId;

public sealed record GetAttachmentsByAppDocIdQuery(int AppDocId) : IRequest<Result<IReadOnlyList<AttachmentListItem>>>;

public sealed record AttachmentListItem(
    int DocId,
    int AppDocId,
    string? DocName,
    string? DocType,
    string? DocPath);

internal sealed class GetAttachmentsByAppDocIdQueryHandler
    : IRequestHandler<GetAttachmentsByAppDocIdQuery, Result<IReadOnlyList<AttachmentListItem>>>
{
    private readonly ITemplateDbContext _context;

    public GetAttachmentsByAppDocIdQueryHandler(ITemplateDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<IReadOnlyList<AttachmentListItem>>> Handle(
        GetAttachmentsByAppDocIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.AppDocId <= 0)
        {
            return Result<IReadOnlyList<AttachmentListItem>>.Success(Array.Empty<AttachmentListItem>());
        }

        var items = await _context.GlAttachments.AsNoTracking()
            .Where(x => x.app_doc_id == request.AppDocId && x.IsDeleted == 0)
            .OrderByDescending(x => x.AudAddDate)
            .Select(x => new AttachmentListItem(
                x.doc_id,
                x.app_doc_id,
                x.doc_name,
                x.doc_type,
                x.doc_path))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AttachmentListItem>>.Success(items);
    }
}
