using CompanyName.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyName.Domain.Entities;

[Table("gl_portal_menus")]
public sealed class GlPortalMenu : AuditableEntity
{
    [Key]
    [Column("menu_id")]
    public int Id { get; set; }

    [Column("menu_name")]
    [MaxLength(150)]
    public string? MenuName { get; set; }

    [Column("menu_url")]
    [MaxLength(1000)]
    public string? MenuUrl { get; set; }

    [Column("icon")]
    [MaxLength(15)]
    public string? Icon { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("order_no")]
    public int? OrderNo { get; set; }
}
