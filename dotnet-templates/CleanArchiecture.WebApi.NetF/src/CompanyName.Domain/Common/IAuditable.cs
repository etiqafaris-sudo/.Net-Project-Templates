namespace CompanyName.Domain.Common;

public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
    byte[] RowVersion { get; set; }
}