using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CompanyName.Domain.Common;

namespace CompanyName.Domain.Entities;

[Table("gl_users")]
public sealed class GlUser : AuditableEntity
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("email")]
    [MaxLength(200)]
    public string? Email { get; set; }

    [Column("username")]
    [MaxLength(80)]
    public string? Username { get; set; }

    [Column("password")]
    [MaxLength(100)]
    public string? Password { get; set; }

    [Column("fullname")]
    [MaxLength(200)]
    public string? Fullname { get; set; }

    [Column("ic_no")]
    [MaxLength(20)]
    public string? IcNo { get; set; }

    [Column("role")]
    [MaxLength(5)]
    public string? Role { get; set; }
}
