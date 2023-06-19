using Newtonsoft.Json;

namespace ClientWeb.Models.TeacherModels
{
    public partial class MarkModel : object
    {
        [JsonProperty("Оценка")]
        public string Mark { get; set; } = default!;
    }
    public partial class StudentAttestationData : object
    {
        [JsonProperty("Фамилия")]
        public string Surname { get; set; } = default!;

        [JsonProperty("Имя")]
        public string Name { get; set; } = default!;

        [JsonProperty("Отчество")]
        public string Partonymic { get; set; } = default!;

        [JsonProperty("Датарождения")]
        public string Birthday { get; set; } = default!;

        [JsonProperty("Пол")]
        public string Gender { get; set; } = default!;

        [JsonProperty("Факультет")]
        public string Faculty { get; set; } = default!;

        [JsonProperty("Курс")]
        public string Course { get; set; } = default!;

        [JsonProperty("Группа")]
        public string Group { get; set; } = default!;

        [JsonProperty("Зачетная_книга")]
        public string Gradebook { get; set; } = default!;

        [JsonProperty("Оценка")]
        public string Mark { get; set; } = default!;
    }

    public partial class TeacherAttestationModel : object
    {
        [JsonProperty("Дисциплина")]
        public string Subject { get; set; } = default!;

        [JsonProperty("Период_контроля")]
        public string SemesterNumber { get; set; } = default!;

        [JsonProperty("Вид_контроля")]
        public string TypeControl { get; set; } = default!;

        [JsonProperty("Система_оценивания")]
        public string TypeAttestation { get; set; } = default!;

        [JsonProperty("Дата_занятия")]
        public string ClassDate { get; set; } = default!;

        [JsonProperty("Время_начала")]
        public string StartDate { get; set; } = default!;

        [JsonProperty("НомерВедомости")]
        public string NumberAttestation { get; set; } = default!;

        [JsonProperty("Группа")]
        public string Group {get;set;} = default!;

        [JsonProperty("Студенты")]
        public List<StudentAttestationData> StudentList { get; set; } = new();

    }
}
