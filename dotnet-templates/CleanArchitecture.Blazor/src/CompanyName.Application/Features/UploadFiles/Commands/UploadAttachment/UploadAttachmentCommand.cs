using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using Mediator;

namespace CompanyName.Application.Features.UploadFiles.Commands.UploadAttachment;

public sealed record UploadAttachmentCommand(
    int AppDocId,
    string FileName,
    string ContentType,
    byte[] FileContent) : IRequest<Result<UploadAttachmentResponse>>;

public sealed record UploadAttachmentResponse(int DocId, string? DocPath);

internal sealed class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, Result<UploadAttachmentResponse>>
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly ITemplateDbContext _context;
    private readonly IAttachmentFileService _attachmentFileService;

    public UploadAttachmentCommandHandler(
        ITemplateDbContext context,
        IAttachmentFileService attachmentFileService)
    {
        _context = context;
        _attachmentFileService = attachmentFileService;
    }

    public async ValueTask<Result<UploadAttachmentResponse>> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AppDocId <= 0)
        {
            return Result<UploadAttachmentResponse>.Failure("Application document ID is required.");
        }

        if (request.FileContent is null || request.FileContent.Length == 0)
        {
            return Result<UploadAttachmentResponse>.Failure("File content is empty.");
        }

        if (request.FileContent.Length > MaxFileSize)
        {
            return Result<UploadAttachmentResponse>.Failure("File size exceeds the 5 MB limit.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<UploadAttachmentResponse>.Failure("File name is required.");
        }

        try
        {
            var relativePath = await _attachmentFileService.SaveAsync(
                request.AppDocId,
                request.FileName,
                request.FileContent,
                cancellationToken);

            var extension = Path.GetExtension(request.FileName);
            var docType = string.IsNullOrWhiteSpace(request.ContentType)
                ? extension
                : request.ContentType;

            var attachment = new GLAtatchment
            {
                app_doc_id = request.AppDocId,
                doc_name = Path.GetFileName(request.FileName),
                doc_type = docType,
                doc_path = relativePath,
                IsActive = 1,
                IsDeleted = 0
            };
            attachment.SetAuditable(OpTypeEnum.Create);

            await _context.GlAttachments.AddAsync(attachment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<UploadAttachmentResponse>.Success(new UploadAttachmentResponse(attachment.doc_id, attachment.doc_path));
        }
        catch
        {
            return Result<UploadAttachmentResponse>.Failure("An error occurred while uploading the attachment.");
        }
    }
}
