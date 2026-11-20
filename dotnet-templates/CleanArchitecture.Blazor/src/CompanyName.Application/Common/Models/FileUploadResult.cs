namespace CompanyName.Application.Common.Models;

public sealed class FileUploadResult
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[]? FileContent { get; set; }
    public long Size { get; set; }
}
