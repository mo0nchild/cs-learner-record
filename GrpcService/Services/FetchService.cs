using Grpc.Core;
using GrpcService;

namespace GrpcService.Services
{
    public class FetchService : FetchData.FetchDataBase
    {
        public const string file = "C:\\Users\\Byter\\Documents\\Projects\\1C Enterprise\\University";
        private readonly ILogger<FetchService> _logger;
        public FetchService(ILogger<FetchService> logger) => this._logger = logger;

        public override Task<ProfileData> GetProfileData(RequestData request, ServerCallContext context)
        {
            var com_connector = new V83.COMConnector();
            var result = com_connector.Connect($"File=\'{FetchService.file}\';Usr=\'Администратор\';pwd=\'\';");

            var query = result.NewObject("Запрос");
            query.Текст = @$"
                ВЫБРАТЬ

                ФизическиеЛица.Код КАК Код,
                СостояниеСтудентов.ЗачетнаяКнига.Наименование КАК ЗачётнаяКнига,
                ФизическиеЛица.Фамилия КАК Фамилия,
                ФизическиеЛица.Имя КАК Имя,
                ФизическиеЛица.Отчество КАК Отчество,

                СостояниеСтудентов.Основа.Наименование КАК ОснованиеОбучения,
                СостояниеСтудентов.Состояние.Наименование КАК Статусстудента,
                СостояниеСтудентов.Специальность.Наименование КАК Специальность,
                СостояниеСтудентов.Специализация.Наименование КАК Профиль,
                СостояниеСтудентов.Группа.Наименование КАК Группа,
                СостояниеСтудентов.Курс.Наименование КАК Курс,
                СостояниеСтудентов.УчебныйПлан.УровеньПодготовки.Наименование КАК Подготовка,
                СостояниеСтудентов.Факультет.Наименование КАК Факультет,
                СостояниеСтудентов.ФормаОбучения.Наименование КАК Формаобучения

            ИЗ
                РегистрСведений.СостояниеСтудентов КАК СостояниеСтудентов
            ВНУТРЕННЕЕ СОЕДИНЕНИЕ Справочник.ФизическиеЛица КАК ФизическиеЛица
                ПО (СостояниеСтудентов.ФизическоеЛицо = ФизическиеЛица.Ссылка)
            ВНУТРЕННЕЕ СОЕДИНЕНИЕ Справочник.СостоянияФизическихЛиц КАК СостоянияФизическихЛиц
                ПО (СостоянияФизическихЛиц.Ссылка = СостояниеСтудентов.Состояние)
            ГДЕ    СостоянияФизическихЛиц.Наименование = ""Является студентом""
                И СостояниеСтудентов.ЗачетнаяКнига.Наименование = ""{request.StudentGradeBook}""";

            var refer = query.Выполнить().Выбрать();
            var profile = new ProfileData();

            if (refer.Следующий == true)
            {
                profile.Id = refer.Код;
                profile.FirstName = refer.Имя;
                profile.LastName = refer.Отчество;
                profile.Group = refer.Группа;
                profile.TuitionType = refer.ОснованиеОбучения;
                profile.TuitionForm = refer.Формаобучения;
                profile.TrainingLevel = refer.Подготовка;
                profile.Speciality = refer.Специальность;
                profile.Specialization =  DBNull.Value.Equals(refer.Профиль) ? "" : refer.Профиль;
                profile.Course = refer.Курс;
                profile.StudentStatus = refer.Статусстудента;
                profile.Patronymic = refer.Отчество;

                this._logger.LogInformation((string)refer.Код);
                this._logger.LogInformation((string)refer.Имя);
                this._logger.LogInformation((string)refer.Отчество);
                this._logger.LogInformation((string)refer.Группа);
                this._logger.LogInformation((string)refer.ОснованиеОбучения);
                this._logger.LogInformation((string)refer.Формаобучения);
                this._logger.LogInformation((string)refer.Подготовка);
                this._logger.LogInformation((string)refer.Специальность);
                this._logger.LogInformation((string)refer.Профиль);
                this._logger.LogInformation((string)refer.Курс);
                this._logger.LogInformation((string)refer.Статусстудента);
            }
            return Task.FromResult(profile);
        }

        public override Task<ProfileStatement> GetProfileStatement(RequestData request, ServerCallContext context)
        {
            var com_connector = new V83.COMConnector();
            var result = com_connector.Connect($"File=\'{FetchService.file}\';Usr=\'Администратор\';pwd=\'\';");

            var query = result.NewObject("Запрос");
            query.Текст = @$"
                    ВЫБРАТЬ 
                    Дисциплина, 
                    ПериодКонтроля КАК Период, 
                    ВидКонтроля, 
                    Отметка, 
                    ДатаЗанятия, 
                    ВремяНачала, 
                    Преподаватель.Ссылка.Код КАК КодПреподавателя,
                    Преподаватель.Ссылка.Наименование КАК Преподаватель
                    ИЗ Документ.Ведомость
                    ВНУТРЕННЕЕ СОЕДИНЕНИЕ  Документ.Ведомость.ДанныеПоАттестации
                    ПО Документ.Ведомость.Ссылка =  Документ.Ведомость.ДанныеПоАттестации.Ссылка
                    ВНУТРЕННЕЕ СОЕДИНЕНИЕ  Документ.Ведомость.Преподаватели
                    ПО Документ.Ведомость.Ссылка =  Документ.Ведомость.Преподаватели.Ссылка
                    ГДЕ ЗачетнаяКнига.Ссылка.Наименование = ""{request.StudentGradeBook}""
                    УПОРЯДОЧИТЬ ПО ПериодКонтроля ВОЗР
            ";

            var refer = query.Выполнить().Выбрать();

            var profileStatement = new ProfileStatement();

            IList<ProfileStatement.Types.StatementInfo> statements = new List<ProfileStatement.Types.StatementInfo>();
            while (refer.Следующий == true)
            {
                ProfileStatement.Types.StatementInfo statement = new ProfileStatement.Types.StatementInfo();
                statement.Subject = refer.Дисциплина.Наименование;
                statement.TeacherId = refer.КодПреподавателя;
                statement.SemestrNumber = refer.Период.Наименование;
                statement.TeacherFullName = refer.Преподаватель;
                statement.Assessment = refer.Отметка.Наименование;
                statement.TypeAttestation = refer.ВидКонтроля.Наименование;
                statement.Date = Convert.ToString(refer.ДатаЗанятия);
                //Timestamp.FromDateTime(DateTime.UtcNow)

                Console.WriteLine(refer.Дисциплина.Наименование);
                statements.Add(statement);
            }
            return Task.FromResult(profileStatement);
        }

    }
}



