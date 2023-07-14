using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts;

public class Contact {
    [Key]
    public int  Id {get;set;}
    [Required(ErrorMessage = "Phai nhap {0}")]
    [Column(TypeName="nvarchar")]
    [StringLength(50)]
    [DisplayName("Ho ten")]
    public string FullName{get;set;}
    [EmailAddress]
    [Required(ErrorMessage = "Phai nhap{0}")]
    public string Email{get;set;}

    public DateTime DateSent {get;set;}
    public string Message {get;set;}
    [Phone]
    [StringLength(50)]
    public string Phone{get;set;}
}