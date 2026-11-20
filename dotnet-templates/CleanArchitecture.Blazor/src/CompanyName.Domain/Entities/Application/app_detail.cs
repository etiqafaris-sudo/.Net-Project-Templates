using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyName.Domain.Entities.Application;

[Table("app_detail")]
public class app_detail : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("app_detail_id")]
    public int app_detail_id { get; set; }

    [Column("app_id")]
    public int app_id { get; set; }

    [Column("app_name")]
    public string? app_name { get; set; }

    [Column("app_other_name")]
    public string? app_other_name { get; set; }

    [Column("app_dob")]
    public DateTime? app_dob { get; set; }

    [Column("app_pob")]
    public string? app_pob { get; set; }

    [Column("app_age")]
    public int? app_age { get; set; }

    [Column("app_gender")]
    public string? app_gender { get; set; }

    [Column("app_race")]
    public string? app_race { get; set; }

    [Column("app_email")]
    public string? app_email { get; set; }

    [Column("app_old_ic")]
    public string? app_old_ic { get; set; }

    [Column("app_new_ic")]
    public string? app_new_ic { get; set; }

    [Column("app_address")]
    public string? app_address { get; set; }

    [Column("app_country_code")]
    public string? app_country_code { get; set; }

    [Column("app_phone_no")]
    public string? app_phone_no { get; set; }
}
