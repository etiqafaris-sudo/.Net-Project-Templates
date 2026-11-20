using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyName.Domain.Entities.Application;

[Table("app_business")]
public class app_business : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("app_business_id")]
    public int app_business_id { get; set; }

    [Column("app_id")]
    public int app_id { get; set; }

    [Column("app_comp_name")]
    public string? app_comp_name { get; set; }

    [Column("app_comp_address")]
    public string? app_comp_address { get; set; }

    [Column("app_maga_addrees")]
    public string? app_maga_addrees { get; set; }

    [Column("app_category")]
    public string? app_category { get; set; }
}
