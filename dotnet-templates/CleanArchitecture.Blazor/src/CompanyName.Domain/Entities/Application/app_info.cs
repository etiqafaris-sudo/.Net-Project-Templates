using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyName.Domain.Entities.Application;

[Table("app_info")]
public class app_info : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("app_id")]
    public int app_id { get; set; }

    [Column("parent_app_id")]
    public int? parent_app_id { get; set; }

    [Column("app_req_id")]
    public string? app_req_id { get; set; }

    [Column("app_type")]
    public string? app_type { get; set; }

    [Column("app_season")]
    public string? app_season { get; set; }

    [Column("app_assessment")]
    public string? app_assessment { get; set; }

    [Column("app_years")]
    public string? app_years { get; set; }

    [Column("app_document")]
    public string? app_document { get; set; }

    [Column("app_stateid")]
    public string? app_stateid { get; set; }

    [Column("app_statusid")]
    public string? app_statusid { get; set; }

    [Column("app_userid")]
    public string? app_userid { get; set; }
}
