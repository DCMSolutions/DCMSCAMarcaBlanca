using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Shared.Model
{
    public class Config
    {
        [Required]
        [RegularExpression(@"^COM\d{2,}$", ErrorMessage = "El puerto COM tiene que tener el formato 'COMXX' donde XX es un número.")]
        public string? COM { get; set; }
        public string? Empresa { get; set; }

        [Url(ErrorMessage = "Tiene que ser una URL válida.")]
        public string? URLDefault { get; set; }

        [RegularExpression(@"^(https?:\/\/[^\s]+)?$", ErrorMessage = "Tiene que ser una URL válida o dejarse en blanco.")]
        public string? URLActual { get; set; }

        [Url(ErrorMessage = "Tiene que ser una URL válida.")]
        public string? URLImagenEncabezado { get; set; }

        [Url(ErrorMessage = "Tiene que ser una URL válida.")]
        public string? URLImagenPie { get; set; }
        public string? Texto1 { get; set; }
        public string? Texto2 { get; set; }
        public string? Texto3 { get; set; }

        [Url(ErrorMessage = "Tiene que ser una URL válida.")]
        public string? EnlaceInfo { get; set; }
        public string? MensajeDisconnect { get; set; }
        public string? MensajeDenegadoDefault { get; set; }
        public bool? HasMensajeDenegado { get; set; }
        public int? TiempoDesconectado { get; set; }
        public int? TiempoAceptado { get; set; }
        public int? TiempoDenegado { get; set; }
        public int? AltoPestaña { get; set; }
        public int? AnchoPestaña { get; set; }
        public int? HorizontalPestaña { get; set; }
        public int? VerticalPestaña { get; set; }
    }
}
