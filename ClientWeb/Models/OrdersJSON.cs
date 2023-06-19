using Newtonsoft.Json;

namespace ClientWeb.Models
{
    public class OrdersJSON
    {
        [JsonProperty("ИД_Студента")]
        public string? IdStudent { get; set; }

        [JsonProperty("Код")]
        public string? OrderCode { get; set; }

        [JsonProperty("Дата_приказа")]
        public DateTime? OrderDate { get; set; }

        [JsonProperty("Заголовок_приказа")]
        public string? OrderHeader { get; set; }

        [JsonProperty("Формулировка")]
        public string? Order { get; set; }
    }
}
