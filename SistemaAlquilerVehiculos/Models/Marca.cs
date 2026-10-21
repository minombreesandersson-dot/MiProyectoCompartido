using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerVehiculos.Models
{
    public class Marca
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no debe superar los 50 caracteres.")]
        [Display(Name = "Nombre de la Marca")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país de origen es obligatorio.")]
        [StringLength(50, ErrorMessage = "El país no debe superar los 50 caracteres.")]
        [Display(Name = "País de Origen")]
        public string PaisOrigen { get; set; } = string.Empty;

        public virtual ICollection<ModeloVehiculo> Modelos { get; set; } = new List<ModeloVehiculo>();
    }
}