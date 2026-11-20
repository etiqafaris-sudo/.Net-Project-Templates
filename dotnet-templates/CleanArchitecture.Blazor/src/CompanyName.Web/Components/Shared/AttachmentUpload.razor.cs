using CompanyName.Application.Features.UploadFiles.Commands.DeleteAttachment;
using CompanyName.Application.Features.UploadFiles.Commands.UploadAttachment;
using CompanyName.Application.Features.UploadFiles.Queries.GetAttachmentsByAppDocId;
using CompanyName.Web.Models.UploadFiles;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;

namespace CompanyName.Web.Components.Shared;

public partial class AttachmentUpload
{
    private readonly string _fileInputId = $"attachment-file-{Guid.NewGuid():N}";
    private List<GlAttachmentDto> attachments = new();
    private bool isUploading;
    private string? selectedFileName;
    private string? selectedFileSizeLabel;

    [Parameter]
    public int AppDocId { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;

    private int MaxFileSizeMb => (int)(MaxFileSize / (1024 * 1024));

    [Parameter]
    public EventCallback OnAttachmentsChanged { get; set; }

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        await LoadAttachmentsAsync();
    }

    private async Task LoadAttachmentsAsync()
    {
        if (AppDocId <= 0)
        {
            attachments = new List<GlAttachmentDto>();
            return;
        }

        var result = await Mediator.Send(new GetAttachmentsByAppDocIdQuery(AppDocId));
        if (result.IsSuccess && result.Value is not null)
        {
            attachments = result.Value.Select(a => new GlAttachmentDto
            {
                DocId = a.DocId,
                AppDocId = a.AppDocId,
                DocName = a.DocName,
                DocType = a.DocType,
                DocPath = a.DocPath
            }).ToList();
        }
    }

    private async Task BrowseFilesAsync()
    {
        if (ReadOnly || AppDocId <= 0 || isUploading)
        {
            return;
        }

        await JsRuntime.InvokeVoidAsync("triggerFileInput", _fileInputId);
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        if (ReadOnly || AppDocId <= 0)
        {
            return;
        }

        var upload = await HandleFileUploadAsync(e, MaxFileSize);
        if (upload?.FileContent is null)
        {
            return;
        }

        selectedFileName = upload.FileName;
        selectedFileSizeLabel = FormatFileSize(upload.Size);

        isUploading = true;
        try
        {
            var result = await Mediator.Send(new UploadAttachmentCommand(
                AppDocId,
                upload.FileName,
                upload.ContentType,
                upload.FileContent));

            if (result.IsFailure)
            {
                await JSErrorAsync(result.Error ?? L.GetLangValue("lbl_create_error"));
                return;
            }

            await JSSuccessAsync(L.GetLangValue("lbl_create_success"));
            selectedFileName = null;
            selectedFileSizeLabel = null;
            await LoadAttachmentsAsync();
            await OnAttachmentsChanged.InvokeAsync();
        }
        finally
        {
            isUploading = false;
        }
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.#} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):0.#} MB";
    }

    private static string GetFileIcon(GlAttachmentDto item)
    {
        var name = item.DocName ?? item.DocType ?? string.Empty;
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "picture_as_pdf",
            ".doc" or ".docx" => "description",
            ".xls" or ".xlsx" => "table_chart",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "image",
            ".zip" or ".rar" => "folder_zip",
            _ => "insert_drive_file"
        };
    }

    private async Task DeleteAttachmentAsync(GlAttachmentDto item)
    {
        var confirmResult = await DialogService.Confirm(
            L.GetLangValue("lbl_confirm_delete"),
            L.GetLangValue("lbl_confirm_delete_title"),
            new ConfirmOptions { OkButtonText = L.GetLangValue("lbl_yes"), CancelButtonText = L.GetLangValue("lbl_cancel") });

        if (confirmResult != true)
        {
            return;
        }

        var result = await Mediator.Send(new DeleteAttachmentCommand(item.DocId));
        if (result.IsFailure)
        {
            await JSErrorAsync(result.Error ?? L.GetLangValue("lbl_delete_error"));
            return;
        }

        await JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
        await LoadAttachmentsAsync();
        await OnAttachmentsChanged.InvokeAsync();
    }

    private async Task OpenDownload(GlAttachmentDto item)
    {
        if (string.IsNullOrWhiteSpace(item.DocPath))
        {
            return;
        }

        var url = item.DocPath.StartsWith('/') ? item.DocPath : $"/{item.DocPath}";
        await JsRuntime.InvokeVoidAsync("open", url, "_blank");
    }
}
