using Newtonsoft.Json;

namespace ClientWeb.Models
{
    public partial class StudentJSON
    {
        [JsonProperty("Код")]
        public int IdStudent { get; set; }

        [JsonProperty("Зачетная_книга")]
        public string? Gradebook { get; set; }

        [JsonProperty("Фамилия")]
        public string? LastName { get; set; }

        [JsonProperty("Имя")]
        public string? FirstName { get; set; }

        [JsonProperty("Отчество")]
        public string? Patronomic { get; set; }

        [JsonProperty("Основание_обучения")]
        public string? TuitionType { get; set; }

        [JsonProperty("Статус_студента")]
        public string? StudentCondition { get; set; }

        [JsonProperty("Специальность")]
        public string? Speciality { get; set; }

        [JsonProperty("Профиль")]
        public string? Specialization { get; set; }

        [JsonProperty("Группа")]
        public string? Group { get; set; }

        [JsonProperty("Курс")]
        public string? Course { get; set; }

        [JsonProperty("Подготовка")]
        public string? TrainingLevel { get; set; }

        [JsonProperty("Факультет")]
        public string? Faculty { get; set; }

        [JsonProperty("Форма_обучения")]
        public string? FormEducation { get; set; }

    }
}
