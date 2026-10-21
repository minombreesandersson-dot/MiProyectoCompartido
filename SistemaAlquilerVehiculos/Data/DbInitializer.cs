using SistemaAlquilerVehiculos.Models;

namespace SistemaAlquilerVehiculos.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Marcas.Any())
            {
                return;
            }

            var marcas = new Marca[]
            {
                new Marca { Nombre = "Toyota", PaisOrigen = "Japón" },
                new Marca { Nombre = "Hyundai", PaisOrigen = "Corea del Sur" },
                new Marca { Nombre = "Ford", PaisOrigen = "Estados Unidos" }
            };
            context.Marcas.AddRange(marcas);
            context.SaveChanges();

            var modelos = new ModeloVehiculo[]
            {
                new ModeloVehiculo { Nombre = "Yaris", Anio = 2022, PrecioDia = 100.00m, MarcaId = marcas[0].Id },
                new ModeloVehiculo { Nombre = "Corolla", Anio = 2023, PrecioDia = 130.00m, MarcaId = marcas[0].Id },
                new ModeloVehiculo { Nombre = "RAV4", Anio = 2023, PrecioDia = 200.00m, MarcaId = marcas[0].Id },
                new ModeloVehiculo { Nombre = "Elantra", Anio = 2021, PrecioDia = 110.00m, MarcaId = marcas[1].Id },
                new ModeloVehiculo { Nombre = "Tucson", Anio = 2023, PrecioDia = 180.00m, MarcaId = marcas[1].Id },
                new ModeloVehiculo { Nombre = "Fiesta", Anio = 2020, PrecioDia = 90.00m, MarcaId = marcas[2].Id },
                new ModeloVehiculo { Nombre = "Explorer", Anio = 2022, PrecioDia = 250.00m, MarcaId = marcas[2].Id }
            };
            context.Modelos.AddRange(modelos);

            var clientes = new Cliente[]
            {
                new Cliente { Documento = "72819203", Nombres = "Juan", Apellidos = "Pérez", Telefono = "987654321", Correo = "juan.perez@email.com" },
                new Cliente { Documento = "45123890", Nombres = "Maria", Apellidos = "Gómez", Telefono = "912345678", Correo = "maria.gomez@email.com" }
            };
            context.Clientes.AddRange(clientes);

            context.SaveChanges();
        }
    }
}