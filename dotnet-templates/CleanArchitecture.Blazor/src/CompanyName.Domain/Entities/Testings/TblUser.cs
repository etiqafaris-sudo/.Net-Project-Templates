using CompanyName.Domain.Common;

namespace CompanyName.Domain.Entities.Testings;

public partial class TblUser : AuditableEntity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }
}
