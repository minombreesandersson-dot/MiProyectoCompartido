using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerVehiculos.Models
{
    public class ModeloVehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del modelo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no debe superar los 50 caracteres.")]
        [Display(Name = "Modelo")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Ingrese un año válido.")]
        [Display(Name = "Año")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "El precio por día es obligatorio.")]
        [Range(0.01, 10000.00, ErrorMessage = "El precio debe ser un valor positivo.")]
        [Display(Name = "Precio por Día (S/)")]
        public decimal PrecioDia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una marca.")]
        [Display(Name = "Marca")]
        public int MarcaId { get; set; }

        public virtual Marca? Marca { get; set; }
        public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    }
}