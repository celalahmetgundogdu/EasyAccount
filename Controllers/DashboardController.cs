using EasyAccount.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EasyAccount.Controllers
{
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            // Son 7 günün başlangıç ve bitiş tarihlerini belirliyorum.
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            // Seçilen işlemleri, kategorileri ile birlikte veritabanından çekiyorum.
            List<Transaction> SelectedTransactions = await _context.Transaction
                .Include(x => x.Category) // Kategorileri de dahil ediyorum.
                .Where(y => y.Date >= StartDate && y.Date <= EndDate) // Tarih filtresi uyguluyorum.
                .ToListAsync(); // Asenkron olarak veritabanından alıyorum.

            // Toplam gelir hesaplaması yapıyorum.
            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income") // Gelir türündeki işlemleri filtreliyorum.
                .Sum(j => j.Amount); // Bu işlemlerin miktarlarını topluyorum.
            ViewBag.TotalIncome = TotalIncome.ToString("C0"); // Toplam geliri ViewBag'e atıyorum ve para birimi formatında görüntülüyorum.

            // Toplam gider hesaplaması yapıyorum.
            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense") // Gider türündeki işlemleri filtreliyorum.
                .Sum(j => j.Amount); // Bu işlemlerin miktarlarını topluyorum.
            ViewBag.TotalExpense = TotalExpense.ToString("C0"); // Toplam gideri ViewBag'e atıyorum ve para birimi formatında görüntülüyorum.

            // Gelir ve gider arasındaki farkı hesaplıyorum (bakiye).
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1; // Negatif para birimi formatını ayarlıyorum.
            ViewBag.Balance = String.Format(culture, "{0:C0}", Balance); // Bakiyeyi ViewBag'e atıyorum ve uygun formatta görüntülüyorum.

            // Giderler için yuvarlak tablo verisini hazırlıyorum.
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense") // Gider türündeki işlemleri filtreliyorum.
                .GroupBy(j => j.Category.CategoryId) // Kategorilere göre grupluyorum.
                .Select(k => new
                {
                    // Kategori başlığı ve ikonu birleştiriyorum.
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    // Her kategori için toplam gider miktarını hesaplıyorum.
                    amount = k.Sum(j => j.Amount),
                    // Bu toplam miktarı formatlıyorum.
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount) // Giderleri miktarına göre azalan sırada sıralıyorum.
                .ToList(); // Listeye çeviriyorum.

            // Gelir ve gider tablosu için veri hazırlıyorum.

            // Gelir tablosu için
            // Seçilen işlemleri temel alarak, "Income" türündeki işlemleri gruplayıp topluyoruz ve bu verileri spline grafiği için uygun formata dönüştürüyoruz.
            List<SplineChartData> IncomeSummary = SelectedTransactions
                // Öncelikle işlemleri "Income" kategorisine göre filtreliyoruz.
                .Where(i => i.Category.Type == "Income")
                // Daha sonra işlemleri tarihine göre grupluyoruz.
                .GroupBy(j => j.Date)
                // Her grup için yeni bir SplineChartData nesnesi oluşturuyoruz.
                .Select(k => new SplineChartData()
                {
                    // Gün alanını grup içerisindeki ilk işlemin tarihini "dd-MMM" formatında stringe çevirerek belirliyoruz.
                    day = k.First().Date.ToString("dd-MMM"),
                    // Gelir alanını, gruptaki tüm işlemlerin miktarlarının toplamını alarak belirliyoruz.
                    income = k.Sum(l => l.Amount)
                })
                // Son olarak, bu yeni oluşturulan SplineChartData nesnelerini listeye çeviriyoruz.
                .ToList();

            // Gider tablosu için
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                // Öncelikle işlemleri "Expense" kategorisine göre filtreliyoruz.
                .Where(i => i.Category.Type == "Expense")
                // Daha sonra işlemleri tarihine göre grupluyoruz.
                .GroupBy(j => j.Date)
                // Her grup için yeni bir SplineChartData nesnesi oluşturuyoruz.
                .Select(k => new SplineChartData()
                {
                    // Gün alanını grup içerisindeki ilk işlemin tarihini "dd-MMM" formatında stringe çevirerek belirliyoruz.
                    day = k.First().Date.ToString("dd-MMM"),
                    // Gider alanını, gruptaki tüm işlemlerin miktarlarının toplamını alarak belirliyoruz.
                    expense = k.Sum(l => l.Amount)
                })
                // Son olarak, bu yeni oluşturulan SplineChartData nesnelerini listeye çeviriyoruz.
                .ToList();

            // Gelir ve gider dengesini oluşturmak için.
            string[] Last7Days = Enumerable.Range(0, 7)
                // Son 7 günü "dd-MMM" formatında stringe çevirerek bir diziye dönüştürüyoruz.
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            // Gelir ve gider verilerini birleştirerek spline grafik verisini oluşturuyorum.
            ViewBag.SplineChartData = from day in Last7Days
                                          // Her gün için gelir ve gider verilerini birleştiriyorum.
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty() // Gelir yoksa sıfır olarak ayarlıyorum.
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty() // Gider yoksa sıfır olarak ayarlıyorum.
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income, // Gelir verisi
                                          expense = expense == null ? 0 : expense.expense, // Gider verisi
                                      };

            // En son yapılan işlemleri getiriyorum.
            ViewBag.RecentTransactions = await _context.Transaction
                .Include(i => i.Category) // Kategorileri de dahil ediyorum.
                .OrderByDescending(j => j.Date) // Tarihe göre azalan sırada sıralıyorum.
                .Take(5) // Son 5 işlemi alıyorum.
                .ToListAsync(); // Asenkron olarak veritabanından alıyorum.

            return View(); // View'ı döndürüyorum.

        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}