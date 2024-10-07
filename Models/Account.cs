using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BankMvcEf.Models;


public class Account
{
    [Key]
    public int Id { get; set;} 
    
    [Required]
    public decimal Balance { get; set; } = 0;
    public string? ConvenienceName { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } =null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public List<Transaction> OutgoingTransactions { get; set; } = null!;
    public List<Transaction> IncomingTransactions { get; set; } = null!;

    public bool Closed {get; set;} = false;

}
