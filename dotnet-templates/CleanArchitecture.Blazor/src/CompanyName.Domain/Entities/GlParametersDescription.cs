using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Entities;

[Table("gl_parameters_description")]
public sealed class GlParametersDescription : AuditableEntity
{
    [Key]
    [Column("parameter_type_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ParameterTypeId { get; set; }

    [Column("parameter_id")]
    public string? ParameterId { get; set; }

    [Column("parameter_description")]
    [MaxLength(250)]
    public string? ParameterDescription { get; set; }

    [Column("sort")]
    public int? Sort { get; set; }
}
