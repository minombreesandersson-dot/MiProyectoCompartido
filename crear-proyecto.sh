#!/usr/bin/env bash
# Crea un proyecto ASP.NET Core MVC + EF Core (Database First, SQLite) con CRUD generado.
# Uso: ./crear-proyecto.sh [NombreProyecto] [ruta/a/base.db]
#   Sin argumentos pregunta el nombre (ventana si hay zenity) y usa MiAppWeb/mi_base_de_datos.db.
set -euo pipefail
cd "$(dirname "$0")"
export PATH="$PATH:$HOME/.dotnet/tools"

NOMBRE="${1:-}"
DB_ORIGEN="${2:-MiAppWeb/mi_base_de_datos.db}"
EF_VER=10.0.13   # mismo que MiAppWeb

if [ -z "$NOMBRE" ]; then
  if [ -n "${DISPLAY:-}" ] && command -v zenity >/dev/null; then
    NOMBRE=$(zenity --entry --title="Nuevo proyecto MVC" --text="Nombre del proyecto (sin espacios):" --entry-text="MiAppCrud") || exit 0
  else
    read -rp "Nombre del proyecto (sin espacios): " NOMBRE
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
dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite \
  -o Models -c AppDbContext --context-dir Data --no-onconfiguring --force

# Cadena de conexión en appsettings.json y registro en Program.cs
sed -i 's|"AllowedHosts": "\*"|"AllowedHosts": "*",\n  "ConnectionStrings": {\n    "DefaultConnection": "Data Source=app.db"\n  }|' appsettings.json
sed -i "1i using Microsoft.EntityFrameworkCore;\nusing $NOMBRE.Data;" Program.cs
sed -i 's|builder.Services.AddControllersWithViews();|&\nbuilder.Services.AddDbContext<AppDbContext>(o =>\n    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));\nbuilder.Services.AddScoped<AdoService>();|' Program.cs

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

echo "==> 6/6 Compilando"
dotnet build
echo
cd .. && ./guardar.sh "Nuevo proyecto $NOMBRE"
echo "Listo. Para ejecutar:  cd $NOMBRE && dotnet watch run"
