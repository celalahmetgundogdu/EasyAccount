using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAccount.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        //Category Id si
        [Range(1,int.MaxValue,ErrorMessage ="Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        // Category classındaki category Id ile foreign primary key ilişkisi kurmak için yaptım. 
        // Think as constructor. İnternette navigational property olarak geçiyor. 
        // Db migrasyonu yapıldıktan sonra public category category bir column oluşturmayacak.

        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than zero.")]
        public int Amount { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string? Note { get; set; }
        //nullable olması için ekledim yani not eklemek zorunda değilim. 
        // Modeldeki diğer ögelerde nullability yok fyi. 
        public DateTime Date {  get; set; } = DateTime.Now;
        // Datetime ı o anki tarihe eşitledim çünkü kaydettiğim anı tutacak. 
        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "- " : "+ ") + Amount.ToString("C0");
            }
        }

    }
}
