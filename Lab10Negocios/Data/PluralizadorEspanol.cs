using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Lab10Negocios.Data;

// dotnet ef dbcontext scaffold usa estas reglas para nombrar clases y DbSet:
// tabla Alquileres -> clase Alquiler -> DbSet Alquileres (el pluralizador por defecto, en inglés, da "Alquilere").
// ponytail: reglas básicas del español; irregulares como Países -> "Paise" hay que renombrarlos a mano.
public class DisenoEspanol : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services) =>
        services.AddSingleton<IPluralizer, PluralizadorEspanol>();
}

public class PluralizadorEspanol : IPluralizer
{
    const string Vocales = "aeiouáéíóú";

    public string Pluralize(string palabra)
    {
        if (string.IsNullOrEmpty(palabra) || palabra.EndsWith('s')) return palabra;
        if (Vocales.Contains(char.ToLower(palabra[^1]))) return palabra + "s";   // Marca -> Marcas
        if (palabra.EndsWith('z')) return palabra[..^1] + "ces";                 // Lapiz -> Lapices
        return palabra + "es";                                                   // Alquiler -> Alquileres
    }

    public string Singularize(string palabra)
    {
        if (string.IsNullOrEmpty(palabra) || palabra.Length < 4 || !palabra.EndsWith('s')) return palabra;
        if (palabra.EndsWith("is", StringComparison.OrdinalIgnoreCase)) return palabra;   // País, Análisis: ya singular
        if (palabra.EndsWith("ces")) return palabra[..^3] + "z";                 // Lapices -> Lapiz
        // Alquileres, Ciudades, Roles -> quitar "es"; Detalles, Padres, Clientes -> quitar solo "s"
        if (palabra.EndsWith("es") && "lrndjy".Contains(palabra[^3]) && Vocales.Contains(char.ToLower(palabra[^4])))
            return palabra[..^2];
        return palabra[..^1];                                                    // Marcas -> Marca
    }
}
