using Newtonsoft.Json;

namespace ClientWeb.Models
{
    public class RecordBookJSON
    {
        [JsonProperty("Дисциплина")]
        public string? Discipline { get; set; }

        [JsonProperty("Период_контроля")]
        public string? ControlPeriod { get; set; }

        [JsonProperty("Вид_контроля")]
        public string? ControlType { get; set; }

        [JsonProperty("Оценка")]
        public string? Grade { get; set; }

        [JsonProperty("Дата_занятия")]
        public DateTime? Date { get; set; }

        [JsonProperty("Время_начала")]
        public DateTime? StartTime { get; set; }

        [JsonProperty("Преподаватель")]
        public string? Teacher { get; set; }

        [JsonProperty("ИД_преподавателя")]
        public string? IdTeacher { get; set; }
    }
}
