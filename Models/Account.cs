using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MvcWithIdentityAndEFCore.Models;


public class Account
{
    [Key]
    public int Id { get; set;} 
    //public string Name? { get; set;} = null!;
    [Required]
    public decimal Balance { get; set; } = 0;

    [Required]
    public string ApplicationUserId { get; set; } =null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;

}
