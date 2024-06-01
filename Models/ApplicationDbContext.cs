using Microsoft.EntityFrameworkCore;

namespace EasyAccount.Models
{
    public class ApplicationDbContext: DbContext // EF core un içinde gelen DbContext classı  ile matchledim
    {
        public ApplicationDbContext(DbContextOptions options):base(options)
        {
         //Constructor
         
        }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Category> Category { get; set; }
        // Eğer sql server içerisinde bir anlam ifade etmesini istiyorsam bu bağlantı kurulmalı. 
        // 2 tableın oluşacağını buradan anladık.
    }
}
