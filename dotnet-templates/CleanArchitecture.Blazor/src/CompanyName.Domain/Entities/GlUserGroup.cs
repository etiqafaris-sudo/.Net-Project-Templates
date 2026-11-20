using CompanyName.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyName.Domain.Entities;

[Table("gl_user_groups")]
public sealed class GlUserGroup : AuditableEntity
{
    [Key]
    [Column("group_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("group_code")]
    [MaxLength(5)]
    public string? GroupCode { get; set; }

    [Column("group_name")]
    [MaxLength(100)]
    public string? GroupName { get; set; }    
}
