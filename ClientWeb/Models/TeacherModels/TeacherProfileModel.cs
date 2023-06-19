using Newtonsoft.Json;

namespace ClientWeb.Models.TeacherModels
{
    public partial class TeacherProfileModel
    {
        [JsonProperty("Табельный_номер")]
        public string? TeacherId { get; set; }

        [JsonProperty("Фамилия")]
        public string? LastName { get; set; }

        [JsonProperty("Имя")]
        public string? FirstName { get; set; }

        [JsonProperty("Отчество")]
        public string? Patronomic { get; set; }

        [JsonProperty("Пол")]
        public string? Sex { get; set; }

        [JsonProperty("ДатаРождения")]
        public string? Birthday { get; set; }

        [JsonProperty("Должность")]
        public string? Post { get; set; }

        [JsonProperty("Ставка")]
        public string? Bet { get; set; }

        [JsonProperty("ВидНачисления")]
        public string? TypeOfAccrual { get; set; }

        [JsonProperty("Кафедра")]
        public string? Department { get; set; }

        [JsonProperty("Подразделение")]
        public string? Devision { get; set; }

        /*public string? Degree { get; set; }*/
        /*public string? Title { get; set; }*/

        public string? DateTime { get; set; }
    }
}
