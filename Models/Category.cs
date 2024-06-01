using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAccount.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        //column db sütunu olduğu için. Hepsi nvarchar 50 char olarak tutulacak
        public string Title {  get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Icon { get; set; } = "";
        [Column(TypeName = "nvarchar(50)")]
        public string Type { get; set; } = "Expense";
        // Hala nullable sadece default değer olarak expensi atadın. bunu yapmamdaki asıl sebep
        // birçok insan gidere expense/gider gibi değerler atıyor. 

    }
}
