# Laboratorio 10 — Pedidos con validación de stock (Database First)

Aplicación ASP.NET Core MVC (.NET 8) + Entity Framework Core 8 que registra pedidos de clientes sobre la base
**Negocios** (SQL Server), valida el stock en el servidor y guarda cabecera, detalles y descuento de stock en una
sola transacción. Se implementó la **Alternativa A** del enunciado: pedido de cliente con `PEDIDO` y `DETALLEPEDIDO`.

## Requisitos

- .NET SDK 8 (Visual Studio 2022 17.8+ en Windows, o `dotnet` en Linux)
- SQL Server 2019/2022 con la base `Negocios` restaurada desde `Negocios.sql`
- Herramienta EF: `dotnet tool install -g dotnet-ef`
- Paquetes (ya en el `.csproj`): `Microsoft.EntityFrameworkCore.SqlServer`, `.Tools` y `.Design` 8.0.32

## Cadena de conexión

`appsettings.json` trae un ejemplo **sin la contraseña real**:

```json
"ConnectionStrings": {
  "Negocios": "Server=localhost,1433;Database=Negocios;User Id=lab10_app;Password=TU_CLAVE;TrustServerCertificate=True"
}
```

La contraseña real se guarda fuera del repositorio con *user-secrets*:

```bash
dotnet user-secrets set "ConnectionStrings:Negocios" "Server=localhost,1433;Database=Negocios;User Id=lab10_app;Password=<tu clave>;TrustServerCertificate=True"
```

En Windows con autenticación integrada puede usarse: `Server=.;Database=Negocios;Trusted_Connection=True;TrustServerCertificate=True`.

## Usuario de base de datos (mínimo privilegio)

La app no usa `sa`. Se creó un usuario que solo lee y escribe datos en Negocios:

```sql
CREATE LOGIN lab10_app WITH PASSWORD = '<tu clave>';
USE Negocios;
CREATE USER lab10_app FOR LOGIN lab10_app;
ALTER ROLE db_datareader ADD MEMBER lab10_app;
ALTER ROLE db_datawriter ADD MEMBER lab10_app;
GRANT VIEW DEFINITION TO lab10_app;   -- necesario para el scaffold
```

## Database First: comando usado

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Negocios" Microsoft.EntityFrameworkCore.SqlServer -o Models -c NegociosContext --context-dir Data --no-onconfiguring
```

En la Consola del Administrador de Paquetes de Visual Studio:

```powershell
Scaffold-DbContext "Name=ConnectionStrings:Negocios" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context NegociosContext -ContextDir Data -NoOnConfiguring
```

`Data/PluralizadorEspanol.cs` hace que las clases salgan en español correcto (`Categoria`, `Pais`, `Proveedores`)
en vez de `Categorium`, `Pai` o `Proveedors`. Las clases generadas en `Models/` **no se editan**: la lógica está en
`Services/PedidoService.cs` y el formulario usa `ViewModels/PedidoCrearVM.cs`.

## Ejecutar

```bash
cd Lab10Negocios
dotnet run --urls http://localhost:5210
```

Abrir http://localhost:5210 → *Productos*, *Pedidos*, *Nuevo pedido*. En Visual Studio: F5.

## Pruebas

Con la app corriendo: `./pruebas.sh` (Linux; usa `sqlcmd` dentro del contenedor Docker `mssql-lab09` para revisar la base).
Ejecuta los 6 casos mínimos del laboratorio más uno de concurrencia, y deja el stock del producto 3 como estaba.
Los casos 1 y 5 registran pedidos reales y descuentan stock (eso es lo que prueban).

Capturas en `evidencias/`.

## Estructura

| Archivo | Qué hace |
| --- | --- |
| `Data/NegociosContext.cs`, `Models/*.cs` | Generados por scaffold (no editar) |
| `Services/PedidoService.cs` | Reglas de negocio: validaciones, precio desde la base, transacción y descuento de stock |
| `ViewModels/PedidoCrearVM.cs` | Datos del formulario y de la confirmación |
| `Controllers/ProductosController.cs` | RF02: productos con búsqueda por nombre y categoría |
| `Controllers/PedidosController.cs` | RF03–RF10: crear, listar y ver pedidos |
| `Views/Pedidos/Create.cshtml` | Formulario con filas de productos dinámicas |
