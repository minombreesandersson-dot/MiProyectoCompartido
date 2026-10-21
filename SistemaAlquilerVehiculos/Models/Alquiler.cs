using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerVehiculos.Models
{
    public class Alquiler
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un modelo de vehículo.")]
        [Display(Name = "Modelo de Vehículo")]
        public int ModeloVehiculoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        [Display(Name = "Días Facturados")]
        public int Dias { get; set; }

        [Display(Name = "Precio Día Aplicado (S/)")]
        public decimal PrecioDiaAplicado { get; set; }

        [Range(0, 30, ErrorMessage = "El descuento debe estar entre el 0% y el 30%.")]
        [Display(Name = "Descuento (%)")]
        public decimal DescuentoPorcentaje { get; set; }

        [Display(Name = "Costo Total (S/)")]
        public decimal CostoTotal { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Registrado";

        public virtual Cliente? Cliente { get; set; }
        public virtual ModeloVehiculo? ModeloVehiculo { get; set; }
    }
}