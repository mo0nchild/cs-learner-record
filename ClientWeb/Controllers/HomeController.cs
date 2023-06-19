using ClientWeb.Helpers;
using ClientWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
using TransferLibrary.Export;
using TransferLibrary.NetworkTransfer;

using Xceed.Words.NET;
using Xceed.Document.NET;
using System.Drawing;

namespace ClientWeb.Controllers
{
    public class HomeController : Controller
    {
        public ILogger<HomeController> Logger { get; set; } = default!;

        private readonly UsersContext _context;
        private readonly IServiceProvider service_provider;
        private readonly string secureKey = default!;

        public HomeController(UsersContext context, IServiceProvider service_provider, ILogger<HomeController> logger,
            IConfiguration configuration)
        {
            _context = context; this.service_provider = service_provider;
            this.secureKey = configuration["SECURE_KEY"] ?? HashHelper.DefaultKey;
            this.Logger = logger;
        }
        protected IRabbitTransfer TransferController { get; private set; } = default!;

        [Authorize(Policy = "OnlyForAdmin")]
        public async Task<IActionResult> Admin()
        {
            string _idAdmin = HttpContext.User.FindFirst("IdUser").Value;
            int idAdmin = Convert.ToInt32(_idAdmin);

            if (_context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == idAdmin);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        [Authorize(Policy = "OnlyForStudent")]
        public async Task<IActionResult> Student()
        {
            string _id = HttpContext.User.FindFirst("IdUser").Value;
            int id = Convert.ToInt32(_id);

            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == id);
            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;
            using var cancel_sourse = new CancellationTokenSource();
            try
            {
                var studentData = rabbit_client.GetStudent(StudentRequestType.Profile, user.Id, cancel_sourse.Token);
                StudentJSON? student = JsonConvert.DeserializeObject<StudentJSON?>(studentData.Result.JsonRecord[0].ToJson());

                if (studentData.Result == null) { Console.WriteLine("\n\tCANCELED\n"); }

                Console.WriteLine("\nJSON studentData: " + studentData.Result.JsonRecord[0].ToJson() + "\n");

                return View(student);
            }
            catch (AggregateException error) when (error.InnerException is TransferException)
            {
                Console.WriteLine($"\n{error.Message}");
                return View();
            }
        }
        [Authorize(Policy = "OnlyForStudent")]
        public async Task<IActionResult> RecordBook()
        {
            string _id = HttpContext.User.FindFirst("IdUser").Value;
            int id = Convert.ToInt32(_id);
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == id);
            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;

            using var cancel_sourse = new CancellationTokenSource();
            List<RecordBookJSON> books = new List<RecordBookJSON>();
            try
            {
                var recordBookData = rabbit_client.GetStudent(StudentRequestType.Statements, user.Id, cancel_sourse.Token);

                Console.WriteLine("\nJSON recordBookData: " + recordBookData.Result.JsonRecord[0].ToJson() + "\n");
                for (int i = 0; i < recordBookData.Result.JsonRecord.Count() - 2; i++)
                {
                    RecordBookJSON? bookData = JsonConvert.DeserializeObject<RecordBookJSON?>(recordBookData.Result.JsonRecord[i].ToJson());
                    books.Add(bookData);
                }

                if (books != null)
                    return View(books);
                else
                {
                    return NotFound();
                }
            }
            catch (AggregateException error) when (error.InnerException is TransferException)
            {
                Console.WriteLine($"\n{error.Message}");
                return View();
            }
        }
        [Authorize(Policy = "OnlyForStudent")]
        public async Task<IActionResult> Orders(string? orderIndex = default!)
        {
            var orderWithoutSecure = orderIndex != default ? HashHelper.DeShifrovka(orderIndex, this.secureKey) : default;
            string _id = HttpContext.User.FindFirst("IdUser").Value;
            int id = Convert.ToInt32(_id);
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == id);
            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;
            var viewModel = new List<OrdersJSON>();

            using (var cancellationSource = new CancellationTokenSource())
            {
                IRabbitTransfer.MessageJson? ordersResponce = default!;
                try {
                    ordersResponce = await rabbit_client.GetStudent(StudentRequestType.Orders, user.Id, cancellationSource.Token);
                    if (ordersResponce == null) throw new TransferException("Данные об приказах незагружены");
                }
                catch (TransferException errorMessage)
                {
                    this.Logger.LogError(errorMessage.Message); return this.NotFound(errorMessage.Message);
                }
                viewModel = JsonConvert.DeserializeObject<List<OrdersJSON>>(ordersResponce!.JsonRecord.ToJson())!
                    .Where(item => item.OrderCode != null).ToList();
            }
            if (orderWithoutSecure == null) this.ViewData["OrderIndex"] = (viewModel.Count <= 0 ? null : 0);
            else this.ViewData["OrderIndex"] = (viewModel.Count <= 0 ? null
                    : viewModel.ToList().FindIndex(item => item.OrderCode == orderWithoutSecure));

            return this.View(viewModel ?? new List<OrdersJSON>());
        }

        [Authorize(Policy = "OnlyForStudent")]
        public async Task<IActionResult> GetDocument()
        {
            string _id = HttpContext.User.FindFirst("IdUser").Value;
            int id = Convert.ToInt32(_id);
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == id);
            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;

            var statementsList = new List<RecordBookJSON>();
            var ordersList = new List<OrdersJSON>();
            var studentRecord = new StudentJSON();

            using (var cancellationSource = new CancellationTokenSource())
            {
                IRabbitTransfer.MessageJson? ordersResponce = default!, profileResponce = default!, 
                    statementsResponce = default!;
                try {
                    profileResponce = await rabbit_client.GetStudent(StudentRequestType.Profile, user.Id, cancellationSource.Token);
                    ordersResponce = await rabbit_client.GetStudent(StudentRequestType.Orders, user.Id, cancellationSource.Token);
                    statementsResponce = await rabbit_client.GetStudent(StudentRequestType.Statements, user.Id, cancellationSource.Token);

                    if (ordersResponce == null || statementsResponce == null || profileResponce == null)
                    {  throw new TransferException("Данные об приказах незагружены"); }
                }
                catch (TransferException errorMessage)
                {
                    this.Logger.LogError(errorMessage.Message); return this.NotFound(errorMessage.Message);
                }
                for (int i = 0; i < statementsResponce.JsonRecord.Count() - 2; i++)
                {
                    var bookData = JsonConvert.DeserializeObject<RecordBookJSON>(statementsResponce.JsonRecord[i].ToJson())!;
                    statementsList.Add(bookData);
                }
                ordersList = JsonConvert.DeserializeObject<List<OrdersJSON>>(ordersResponce!.JsonRecord.ToJson())!
                    .Where(item => item.OrderCode != null).ToList();
                studentRecord = JsonConvert.DeserializeObject<StudentJSON>(profileResponce!.JsonRecord[0].ToJson());
            }
            using var bufferResult = new MemoryStream();
            using (var document = DocX.Create(bufferResult, DocumentTypes.Document))
            {
                var p = document.InsertParagraph();

                p.Append("Данные профиля:")
                .Font(new Xceed.Document.NET.Font("Arial")).FontSize(14).Color(Color.Black).Bold();

                document.Save();
            }
            return base.File(bufferResult.GetBuffer(), "application/octet-stream", "Отчет.docx");
        }
    }
}