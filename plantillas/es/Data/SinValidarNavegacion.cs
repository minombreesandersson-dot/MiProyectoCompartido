using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace __NOMBRE__.Data;

// El scaffold genera "public virtual Cliente Cliente { get; set; } = null!;" y ASP.NET la vuelve obligatoria,
// pero el formulario solo envía ClienteId: Create/Edit fallaban sin mostrar ningún error.
// Esta regla deja de validar las propiedades de navegación (se valida su Id, que sí está en el formulario).
public class SinValidarNavegacion : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        var tipo = context.Key.ModelType;
        if (context.Key.MetadataKind != ModelMetadataKind.Property || tipo == typeof(string) || !tipo.IsClass
            || tipo.Namespace != context.Key.ContainerType?.Namespace) return;

        context.ValidationMetadata.ValidateChildren = false;
        foreach (var requerido in context.ValidationMetadata.ValidatorMetadata.OfType<RequiredAttribute>().ToList())
            context.ValidationMetadata.ValidatorMetadata.Remove(requerido);
    }
}
