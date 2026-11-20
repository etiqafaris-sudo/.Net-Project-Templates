namespace CompanyName.Application.Common.Interfaces;

public interface IAttachmentFileService
{
    Task<string> SaveAsync(int appDocId, string fileName, byte[] content, CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
