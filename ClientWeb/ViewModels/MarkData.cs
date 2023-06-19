namespace ClientWeb.ViewModels
{
    public sealed class MarkData : Object
    {
        public string MarkValue { get; set; } = default!;
        public string Gradebook { get; set; } = default!;
    }

    public sealed class StatementData : Object
    {
        public string StatementNumber { get; set; } = default!;
        public List<MarkData> MarksList { get; set; } = new();
    }
}
