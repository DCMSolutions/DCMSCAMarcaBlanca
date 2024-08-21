namespace BlazorApp1.Server
{
    public class Config
    {
        public string? COM { get; set; }
        public string? Empresa { get; set; }
        public string? URLDefault { get; set; }
        public string? URLActual { get; set; }
        public string? Texto1 { get; set; }
        public string? Texto2 { get; set; }
        public string? Texto3 { get; set; }
        public string? EnlaceInfo { get; set; }
        public string? MensajeDisconnect { get; set; }
        public string? MensajeDenegadoDefault { get; set; }
        public bool? HasMensajeDenegado { get; set; }
        public int? TiempoDesconectado { get; set; }
        public int? TiempoAceptado { get; set; }
        public int? TiempoDenegado { get; set; }
        public int? AltoPestaña { get; set; }
        public int? AnchoPestaña { get; set; }
    }
}
