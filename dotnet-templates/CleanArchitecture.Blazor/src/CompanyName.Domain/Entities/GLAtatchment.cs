using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyName.Domain.Entities;

[Table("gl_attachment")]
public sealed class GLAtatchment : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("doc_id")]
    public int doc_id { get; set; }

    [Column("app_doc_id")]
    public int app_doc_id { get; set; }

    [Column("doc_name")]
    [MaxLength(250)]
    public string? doc_name { get; set; }

    [Column("doc_type")]
    [MaxLength(25)]
    public string? doc_type { get; set; }

    [Column("doc_path")]
    [MaxLength(1000)]
    public string? doc_path { get; set; }
}
