
using BankMvcEf.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Transaction
{
    [Key]
    public int Id { get; set;} 

    [ForeignKey(nameof(FromAcc))]
    public int FromAccId { get; set;}
    public Account FromAcc { get; set;} = null!;
    [ForeignKey(nameof(ToAcc))]
    public int ToAccId { get; set;}
    public Account ToAcc { get; set;} = null!;
    
    public DateTime Time { get; set;} 
    public decimal Amount { get; set;}
}