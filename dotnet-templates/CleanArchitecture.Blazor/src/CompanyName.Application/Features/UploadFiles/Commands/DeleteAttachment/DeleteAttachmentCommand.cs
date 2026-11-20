using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.UploadFiles.Commands.DeleteAttachment;

public sealed record DeleteAttachmentCommand(int DocId) : IRequest<Result<bool>>;

internal sealed class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, Result<bool>>
{
    private readonly ITemplateDbContext _context;
    private readonly IAttachmentFileService _attachmentFileService;

    public DeleteAttachmentCommandHandler(
        ITemplateDbContext context,
        IAttachmentFileService attachmentFileService)
    {
        _context = context;
        _attachmentFileService = attachmentFileService;
    }

    public async ValueTask<Result<bool>> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _context.GlAttachments.FirstOrDefaultAsync(
            x => x.doc_id == request.DocId && x.IsDeleted == 0,
            cancellationToken);

        if (attachment is null)
        {
            return Result<bool>.Failure($"Attachment with ID {request.DocId} was not found.");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(attachment.doc_path))
            {
                await _attachmentFileService.DeleteAsync(attachment.doc_path, cancellationToken);
            }

            attachment.IsDeleted = 1;
            attachment.SetAuditable(OpTypeEnum.Delete);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Failure("An error occurred while deleting the attachment.");
        }
    }
}
