using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ClientWeb.Data
{
    public class Student
    {
        [JsonPropertyName("Код")] public System.String Code { get; set; } = default!;
        [JsonPropertyName("ФИО")] public System.String Name { get; set; } = default!;
        [JsonPropertyName("Группа")] public System.String Group { get; set; } = default!;
        [JsonPropertyName("Курс")] public System.String Сourse { get; set; } = default!;
    }

    public sealed class Statements : System.Object
    {
        [JsonPropertyName("Курс")] public System.String Course { get; set; } = default!;
        [JsonPropertyName("Дисциплина")] public System.String Subject { get; set; } = default!;
        [JsonPropertyName("Оценка")] public System.Int32 Rating { get; set; } = default!;
        public override string ToString() => $"Дисциплина: {Subject}\nОценка: {Rating}\n";
    }

    [JsonArrayAttribute]
    public sealed class CollectionStatements : System.Object
    {
        [JsonPropertyName("Курсы")][JsonInclude] public List<Statements> Statement { get; set; } = default!;
        public CollectionStatements() { }
    }
}
