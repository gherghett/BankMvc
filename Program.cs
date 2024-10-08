using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankMvcEf.Data;
using BankMvcEf.Models;
using BankMvcEf.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Default roles, using strings as key
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Lägger till autentisering
app.UseAuthorization(); // OBS viktigt att de är i denna ordningen

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// // Det här behövs bara köras en gång,
// // Det körs här för demonstration
// bool JagHarEnAnvändareMedRättEpost = false; //ändra efter du skapat användare
// string epost = "gherghetta@gmail.com"; 

// if(JagHarEnAnvändareMedRättEpost)
// {
//     using (var scope = app.Services.CreateScope())
//     {
//         //
//         var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//         string[] roleNames = { "Leader", "Parent", "Scout" };

//         foreach (var roleName in roleNames)
//         {
//             if (!await roleManager.RoleExistsAsync(roleName))
//             {
//                 await roleManager.CreateAsync(new IdentityRole(roleName));
//             }
//         }

//         // Lägg till en roll till en användare
//         var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
//         var me = await userManager.FindByEmailAsync(epost);
//         await userManager.AddToRoleAsync(me!, "Leader");
//         //await userManager.RemoveFromRoleAsync(me!, "Leader");
//         var roles = await userManager.GetRolesAsync(me!);
//         Console.Write("---Roles----------: ");
//         Console.WriteLine(string.Join(", ", roles));
//     }
// }

app.Run();
