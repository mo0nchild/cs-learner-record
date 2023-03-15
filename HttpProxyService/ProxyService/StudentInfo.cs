using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HttpProxyService.ProxyService
{
    [type: System.ObsoleteAttribute()]
    public sealed class StudentInfoData : System.Object
    {
        [JsonPropertyName("Код")] public System.String Code { get; set; } = default!;
        [JsonPropertyName("ФИО")] public System.String Name { get; set; } = default!;
        [JsonPropertyName("Группа")] public System.String Group { get; set; } = default!;
        [JsonPropertyName("Курс")] public System.String Сourse { get; set; } = default!;

        public override string ToString() => $"Код: {Code}\r\nФИО: {Name}\r\nГруппа {Group}\r\n{Сourse}";
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
