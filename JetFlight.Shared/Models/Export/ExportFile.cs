namespace JetFlight.Shared.Models.Export
{
    public class ExportFile
    {
        public Stream Stream { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }
    }
}
