using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyName.Domain.Entities.Application;

[Table("app_document")]
public class app_document : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("app_doc_id")]
    public int app_doc_id { get; set; }

    [Column("app_id")]
    public int app_id { get; set; }

    [Column("doc_remarks")]
    public string? doc_remarks { get; set; }
}
