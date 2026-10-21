#!/usr/bin/env bash
# Crea un proyecto ASP.NET Core MVC + EF Core (Database First, SQLite) con CRUD generado.
# Uso: ./crear-proyecto.sh [NombreProyecto] [ruta/a/base.db]
#   Sin argumentos pregunta el nombre (ventana si hay zenity) y usa plantillas/alquiler-vehiculos.db
#   (mismo esquema y datos iniciales que SistemaAlquilerVehiculos, en SQLite).
set -euo pipefail
cd "$(dirname "$0")"
export PATH="$PATH:$HOME/.dotnet/tools"

NOMBRE="${1:-}"
DB_ORIGEN="${2:-plantillas/alquiler-vehiculos.db}"
EF_VER=10.0.13   # mismo que MiAppWeb

if [ -z "$NOMBRE" ]; then
  if [ -n "${DISPLAY:-}" ] && command -v zenity >/dev/null; then
    NOMBRE=$(zenity --entry --title="Nuevo proyecto MVC" --text="Nombre del proyecto (sin espacios):" --entry-text="SistemaAlquilerVehiculosDbFirst") || exit 0
  else
    read -rp "Nombre del proyecto (Enter = SistemaAlquilerVehiculosDbFirst): " NOMBRE
    NOMBRE="${NOMBRE:-SistemaAlquilerVehiculosDbFirst}"
  fi
fi
[[ "$NOMBRE" =~ ^[A-Za-z][A-Za-z0-9_]*$ ]] || { echo "Nombre inválido: '$NOMBRE'"; exit 1; }
[ -e "$NOMBRE" ] && { echo "Ya existe la carpeta $NOMBRE"; exit 1; }
[ -f "$DB_ORIGEN" ] || { echo "No encuentro la base de datos $DB_ORIGEN"; exit 1; }

echo "==> 1/6 Herramientas globales (dotnet-ef, dotnet-aspnet-codegenerator)"
dotnet tool list -g | grep -q '^dotnet-ef ' || dotnet tool install -g dotnet-ef
dotnet tool list -g | grep -q '^dotnet-aspnet-codegenerator ' || dotnet tool install -g dotnet-aspnet-codegenerator

echo "==> 2/6 Proyecto MVC $NOMBRE"
dotnet new mvc -n "$NOMBRE" -f net10.0
dotnet sln MiProyectoCompartido.slnx add "$NOMBRE/$NOMBRE.csproj"
cp "$DB_ORIGEN" "$NOMBRE/app.db"
cd "$NOMBRE"

echo "==> 3/6 Paquetes NuGet"
for p in Microsoft.EntityFrameworkCore.Sqlite Microsoft.EntityFrameworkCore.Design \
         Microsoft.EntityFrameworkCore.Tools Microsoft.EntityFrameworkCore.SqlServer; do
  dotnet add package "$p" -v "$EF_VER"
done
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
../agregar-paquetes.sh "$NOMBRE" $(seq 1 13)   # los 13 paquetes extra (incluye ADO.NET y Dapper)

echo "==> 4/6 Scaffolding Database First (modelos + DbContext)"
# Nombres en español (Alquileres -> Alquiler). El código usa EF Design, así que se habilita su "compile"
cp -r ../plantillas/es/. . && sed -i "s/__NOMBRE__/$NOMBRE/g" Data/PluralizadorEspanol.cs Data/SinValidarNavegacion.cs
sed -i '/EntityFrameworkCore.Design"/,/<\/PackageReference>/{/<IncludeAssets>/d}' "$NOMBRE.csproj"
dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite \
  -o Models -c AppDbContext --context-dir Data --no-onconfiguring --force

# Cadena de conexión en appsettings.json y registro en Program.cs
sed -i 's|"AllowedHosts": "\*"|"AllowedHosts": "*",\n  "ConnectionStrings": {\n    "DefaultConnection": "Data Source=app.db"\n  }|' appsettings.json
sed -i "1i using Microsoft.EntityFrameworkCore;\nusing $NOMBRE.Data;" Program.cs
sed -i 's|builder.Services.AddControllersWithViews();|&\nbuilder.Services.AddDbContext<AppDbContext>(o =>\n    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));\nbuilder.Services.AddScoped<AdoService>();|' Program.cs
# Sin esto, Create/Edit de tablas con claves foráneas fallan en silencio (ver Data/SinValidarNavegacion.cs)
sed -i 's|AddControllersWithViews();|AddControllersWithViews(o => o.ModelMetadataDetailsProviders.Add(new SinValidarNavegacion()));|' Program.cs

# ADO.NET: servicio, controlador y vista de ejemplo (/Ado), antes del CRUD porque el generador compila
cp -r ../plantillas/ado/. .
sed -i "s/__NOMBRE__/$NOMBRE/g" Data/AdoService.cs Controllers/AdoController.cs

# Enlace en el menú superior de la app
nav() { sed -i "0,/<\/ul>/s|<\/ul>|    <li class=\"nav-item\"><a class=\"nav-link text-dark\" asp-controller=\"$1\" asp-action=\"Index\">$2</a></li>\n                    </ul>|" Views/Shared/_Layout.cshtml; }

echo "==> 5/6 CRUD (controlador + vistas) por cada tabla"
for f in Models/*.cs; do
  m=$(basename "$f" .cs)
  case "$m" in ErrorViewModel|Efmigrations*) continue ;; esac   # Efmigrations* = tablas internas de EF
  dotnet-aspnet-codegenerator controller -name "${m}Controller" -m "$m" -dc AppDbContext \
    --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries
  nav "$m" "$m"
done

nav Ado "ADO.NET"

# Nombres en vez de Ids en listas desplegables y columnas de relaciones (Cliente "1" -> "Juan")
for f in Models/*.cs; do
  grep -oP 'public virtual \K\w+ \w+(?= \{ get; set; \} = null!;)' "$f" || true
done | sort -u | while read -r clase nav; do
  campo=$(grep -oP 'public string \K(Nombres?|Descripcion)\b' "Models/$clase.cs" | head -1 || true)
  [ -n "$campo" ] || campo=$(grep -oP 'public string \K\w+' "Models/$clase.cs" | head -1 || true)
  [ -n "$campo" ] || continue
  set=$(grep -oP "DbSet<$clase> \K\w+" Data/AppDbContext.cs)
  sed -i "s/new SelectList(_context\.$set, \"Id\", \"Id\"/new SelectList(_context.$set, \"Id\", \"$campo\"/" Controllers/*Controller.cs
  sed -i "s/item\.$nav\.Id)/item.$nav.$campo)/; s/model\.$nav\.Id)/model.$nav.$campo)/" Views/*/*.cshtml
done
# Listas desplegables con flecha (Bootstrap 5) y tablas con scroll horizontal en pantallas chicas
sed -i 's/<select \(.*\)class ="form-control"/<select \1class ="form-select"/' Views/*/*.cshtml
for v in $(grep -l '<table class="table">' Views/*/Index.cshtml); do
  sed -i 's|<table class="table">|<div class="table-responsive"><table class="table">|; s|</table>|</table></div>|' "$v"
done

echo "==> 6/6 Compilando"
dotnet build
echo
cd .. && ./guardar.sh "Nuevo proyecto $NOMBRE"
echo "Listo. Para ejecutar:  cd $NOMBRE && dotnet watch run"
