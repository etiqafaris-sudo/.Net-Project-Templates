namespace CompanyName.Web.Models.UploadFiles;

public sealed class GlAttachmentDto
{
    public int DocId { get; set; }
    public int AppDocId { get; set; }
    public string? DocName { get; set; }
    public string? DocType { get; set; }
    public string? DocPath { get; set; }
}
