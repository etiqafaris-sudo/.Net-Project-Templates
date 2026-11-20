using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Entities;

[Table("gl_parameters")]
public sealed class GlParameter : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("gl_parameter_id")]
    public int GlParameterId { get; set; }

    [Column("parameter_id")]
    public string ParameterId { get; set; } = string.Empty;

    [Column("parameter_code")]
    [MaxLength(5)]
    public string? ParameterCode { get; set; }

    [Column("parameter_type_id")]
    public int ParameterTypeId { get; set; }

    [Column("parameter_description")]
    [MaxLength(250)]
    public string? ParameterDescription { get; set; }

    [Column("parameter_description_en")]
    [MaxLength(250)]
    public string? ParameterDescriptionEn { get; set; }

    [Column("sort")]
    public int? Sort { get; set; }
}
