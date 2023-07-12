using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace App.Models;

public class AppUser :IdentityUser{

    [Column(TypeName = "LONGTEXT")]
    [StringLength(255)]
    public string? HomeAddress {get;set;}

    [DataType(DataType.Date)]
    public DateTime? BirthDate {get;set;}
}