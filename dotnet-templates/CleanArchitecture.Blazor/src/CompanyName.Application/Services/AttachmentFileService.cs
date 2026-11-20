using CompanyName.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace CompanyName.Application.Services;

public sealed class AttachmentFileService : IAttachmentFileService
{
    private const string UploadFolder = "uploads";
    private readonly IWebHostEnvironment _environment;

    public AttachmentFileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(int appDocId, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var relativeDir = Path.Combine(UploadFolder, appDocId.ToString());
        var physicalDir = Path.Combine(_environment.WebRootPath, relativeDir);
        Directory.CreateDirectory(physicalDir);

        var physicalPath = Path.Combine(physicalDir, uniqueName);
        await File.WriteAllBytesAsync(physicalPath, content, cancellationToken);

        return Path.Combine(relativeDir, uniqueName).Replace('\\', '/');
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, normalized);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}
