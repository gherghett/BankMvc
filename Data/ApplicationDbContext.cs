using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcWithIdentityAndEFCore.Models;
using SQLitePCL;
namespace MvcWithIdentityAndEFCore.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // DbSet för din applikationsdata
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    //public DbSet<ApplicationUser> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.ApplicationUser) // Each Account has one ApplicationUser
            .WithMany(au => au.Accounts)     // An ApplicationUser can have many Accounts
            .HasForeignKey(a => a.ApplicationUserId); // Specify the foreign key if not convention-based

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromAcc)
            .WithMany(a => a.OutgoingTransactions)
            .HasForeignKey(t => t.FromAccId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToAcc)
            .WithMany(a => a.IncomingTransactions)
            .HasForeignKey(t => t.ToAccId)
            .OnDelete(DeleteBehavior.Restrict);
    
    }
}
