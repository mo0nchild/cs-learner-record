using ClientWeb.Helpers;
using ClientWeb.Models;
using ClientWeb.Models.TeacherModels;
using ClientWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Diagnostics;
using TransferLibrary.Export;
using TransferLibrary.NetworkTransfer;

namespace ClientWeb.Controllers
{
    [Authorize(Policy = "OnlyForTeacher")]
    public class TeacherController : Controller
    {
        protected ILogger<TeacherController> Logger { get; set; } = default!;

        private readonly UsersContext _context = default!;
        private readonly IRabbitTransfer rabbitTransfer = default!;

        private readonly string secureKey = default!;

        public TeacherController(UsersContext context, IRabbitTransfer rabbitTransfer, ILogger<TeacherController> logger,
            IConfiguration configuration)
        {
            _context = context; this.rabbitTransfer = rabbitTransfer;
            this.Logger = logger;
            this.secureKey = configuration["SECURE_KEY"] ?? HashHelper.DefaultKey;
        }
        private List<TModel>? GetRabbitData<TModel>(EmployeeRequestType requestType, string teacherId) where TModel : new()
        {
            using var cancel_sourse = new CancellationTokenSource();
            try {
                var employeeData = rabbitTransfer.GetEmployee(requestType, teacherId, cancel_sourse.Token);
                if (employeeData.Result == null) { this.Logger.LogInformation("\n\tCANCELED\n"); }

                this.Logger.LogInformation("JSON employeeData: " + employeeData.Result.JsonRecord.ToJson());
                return JsonConvert.DeserializeObject<List<TModel>?>(employeeData.Result.JsonRecord.ToJson());
            }
            catch (AggregateException error) when (error.InnerException is TransferException)
            {
                this.Logger.Log(LogLevel.Error, $"\n{error.Message}");
            }
            return new List<TModel>();
        }
        public async Task<IActionResult> Profile()
        {
            if (_context.Users == null) { return NotFound(); }
            var _id = Convert.ToInt32(HttpContext.User.FindFirst("IdUser")!.Value);

            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == _id);
            var viewModel = this.GetRabbitData<TeacherProfileModel>(EmployeeRequestType.Profile, user.Id)?[0];

            return this.View(viewModel ?? new TeacherProfileModel());
        }
        public async Task<IActionResult> Attestation(string? statementNumber = default!, string? error = default!)
        {
            statementNumber = (statementNumber == null) ? null : HashHelper.DeShifrovka(statementNumber, this.secureKey);

            var teacherId = int.Parse(this.HttpContext.User.FindFirst("IdUser")?.Value ?? "-1");
            if (teacherId == -1) return this.NotFound("Куки авторизации не найдены");

            if (error != null) this.ModelState.AddModelError("", error);
            using (var cancellationSource = new CancellationTokenSource()) 
            {
                IRabbitTransfer.MessageJson? marksresponce = default!;
                try {
                    marksresponce = await this.rabbitTransfer.MarksList(cancellationSource.Token);
                    if (marksresponce == null) throw new TransferException("Данные об оценках незагружены");
                }
                catch (TransferException errorMessage)
                {
                    this.Logger.LogError(errorMessage.Message); return this.NotFound(errorMessage.Message);
                }
                var marksList = JsonConvert.DeserializeObject<List<MarkModel>>(marksresponce.JsonRecord.ToJson());

                this.ViewData["MarksList"] = marksList!.Where(item => item.Mark != null)
                    .Select(item => item.Mark).ToList();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == teacherId);
            var viewModel = this.GetRabbitData<TeacherAttestationModel>(EmployeeRequestType.Attestation, user.Id)
                ?? new List<TeacherAttestationModel>();
            viewModel = viewModel.Where(item => item.NumberAttestation != null).ToList();

            if (statementNumber == null) this.ViewData["StatementIndex"] = (viewModel.Count <= 0 ? null : 0);
            else this.ViewData["StatementIndex"] = (viewModel.Count <= 0 ? null 
                    : viewModel.ToList().FindIndex(item => item.NumberAttestation == statementNumber));

            this.ViewData["StatementNumber"] = statementNumber;
            return this.View(viewModel ?? new List<TeacherAttestationModel>());
        }

        [HttpPost]
        public async Task<IActionResult> SaveMarks([FromForm] StatementData model)
        {
            var statementWithoutSecure = HashHelper.DeShifrovka(model.StatementNumber, this.secureKey);

            var teacherId = int.Parse(this.HttpContext.User.FindFirst("IdUser")?.Value ?? "-1");
            if (teacherId == -1) return this.NotFound("Куки авторизации не найдены");

            var user = await _context.Users.FirstOrDefaultAsync(m => m.IdUser == teacherId);
            var viewModel = this.GetRabbitData<TeacherAttestationModel>(EmployeeRequestType.Attestation, user.Id);

            if(viewModel == null) return this.RedirectToAction("Attestation", new 
            { 
                error = "Невозможно установить данные", statementNumber = model.StatementNumber
            });
            viewModel = viewModel.Where(item => item.NumberAttestation != null).ToList();

            var currentAttestation = viewModel.FirstOrDefault(item => item.NumberAttestation == statementWithoutSecure);
            if (currentAttestation == null) return this.RedirectToAction("Attestation", new 
            { 
                error = "Ведомость не найдена", statementNumber = model.StatementNumber
            });
            var bufferFound = new List<MarkData>();
            foreach (var item in currentAttestation.StudentList)
            {
                foreach (var record in model.MarksList)
                {
                    if ((item.Gradebook == record.Gradebook && item.Mark == record.MarkValue) ||
                        item.Gradebook != record.Gradebook) continue;

                    bufferFound.Add(new MarkData() { Gradebook = record.Gradebook, MarkValue = record.MarkValue });
                }
            }
            using var cancellationSourse = new CancellationTokenSource();
            foreach(var item in bufferFound)
            {
                var requestMessage = new ExportTransfer.MarkData(item.MarkValue, statementWithoutSecure, item.Gradebook);

                try { await this.rabbitTransfer.SetMark(requestMessage, cancellationSourse.Token); }
                catch(TransferException error) { this.Logger.LogError(error.Message); }
            }
            return this.RedirectToAction("Attestation", new { statementNumber = model.StatementNumber });
        }
    }
}
