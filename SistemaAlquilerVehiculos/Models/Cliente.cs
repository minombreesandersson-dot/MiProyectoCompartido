using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerVehiculos.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "El documento debe tener entre 8 y 12 caracteres.")]
        [Display(Name = "Documento de Identidad")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(60)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(60)]
        public string Apellidos { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Cliente")]
        public string NombreCompleto => $"{Nombres} {Apellidos} ({Documento})";

        public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    }
}