using CompanyName.Application.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CompanyName.Web.Bos.Application;

public class ApplicationBo
{
    public int app_id { get; set; }
    public int? parent_app_id { get; set; }
    public string? parent_app_id_str {
        get => parent_app_id?.ToString();
        set => parent_app_id = int.TryParse(value, out int result) ? result : null;
    }
    public List<DropdownItem> main_applications { get; set; } = new();

    public string app_req_id { get; set; } = string.Empty;
    public List<DropdownItem> app_types { get; set; } = new();
    
    [Required(ErrorMessage = "Application Type is required.")]
    public string app_type { get; set; } = string.Empty;
    
    public List<DropdownItem> app_seasons { get; set; } = new();
    
    [Required(ErrorMessage = "Season is required.")]
    public string? app_season { get; set; }
    
    [Required(ErrorMessage = "Assessment Note is required.")]
    public string? app_assessment { get; set; }
    
    [Required(ErrorMessage = "Years is required.")]
    public string? app_years { get; set; }
    
    public List<DropdownItem> app_documents { get; set; } = new();
    
    [Required(ErrorMessage = "Document Status is required.")]
    public string? app_document { get; set; }
    
    public string? app_stateid { get; set; }
    public string? app_statusid { get; set; }
    public string? app_userid { get; set; }

    // Child BOs added via Composition instead of Inheritance
    public DetailBo Detail { get; set; } = new DetailBo();
    public BusinessBo Business { get; set; } = new BusinessBo();
    public List<DocumentBo> Documents { get; set; } = new List<DocumentBo>();
}

public class DetailBo
{
    public int app_detail_id { get; set; }
    
    [Required(ErrorMessage = "Applicant Name is required.")]
    public string? app_name { get; set; }
    
    public string? app_other_name { get; set; }
    
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime? app_dob { get; set; }
    
    [Required(ErrorMessage = "Place of Birth is required.")]
    public string? app_pob { get; set; }
    
    [Required(ErrorMessage = "Age is required.")]
    public int? app_age { get; set; }
    
    public List<DropdownItem> app_genders { get; set; } = new();
    
    [Required(ErrorMessage = "Gender is required.")]
    public string? app_gender { get; set; }
    
    public List<DropdownItem> app_races { get; set; } = new();
    
    [Required(ErrorMessage = "Race is required.")]
    public string? app_race { get; set; }
    
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string? app_email { get; set; }
    
    public string? app_old_ic { get; set; }
    
    [Required(ErrorMessage = "New IC Number is required.")]
    public string? app_new_ic { get; set; }
    
    [Required(ErrorMessage = "Home Address is required.")]
    public string? app_address { get; set; }
    
    public string? app_country_code { get; set; }
    
    [Required(ErrorMessage = "Phone Number is required.")]
    public string? app_phone_no { get; set; }
}

public class BusinessBo
{
    public int app_business_id { get; set; }
    
    [Required(ErrorMessage = "Company Name is required.")]
    public string? app_comp_name { get; set; }
    
    [Required(ErrorMessage = "Company Address is required.")]
    public string? app_comp_address { get; set; }
    
    [Required(ErrorMessage = "Temporary Magazine Address is required.")]
    public string? app_maga_addrees { get; set; }
    
    public List<DropdownItem> app_categorys { get; set; } = new();
    
    [Required(ErrorMessage = "Category is required.")]
    public string? app_category { get; set; }
}

public class DocumentBo
{
    public int app_doc_id { get; set; }
    public string? doc_remarks { get; set; }
}
