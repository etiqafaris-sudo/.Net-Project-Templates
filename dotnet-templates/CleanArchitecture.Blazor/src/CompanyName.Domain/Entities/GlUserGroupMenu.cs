using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyName.Domain.Entities;

[Table("gl_user_group_menus")]
public class GlUserGroupMenu : AuditableEntity
{
    [Key]
    [Column("group_menu_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("group_id")]
    public int? GroupId { get; set; }

    [Column("menu_id")]
    public int? MenuId { get; set; }
}
