namespace ClientWeb.ViewModels.SubjectViewModel
{
    public partial class Attestation : object
    {
        public string Group { get; set; } = default!;
        public string NumberAttestation { get; set; } = default!;
    }
    
    public partial class SubjectViewModel : object
    {
        public string SubjectName { get; set; } = default!;
        public List<Attestation> Attestations { get; set; } = new();
    }
}