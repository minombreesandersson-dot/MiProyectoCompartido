# Versión Windows de crear-proyecto.sh (PowerShell). Doble clic en crear-proyecto.bat
# Uso: .\crear-proyecto.ps1 [-Nombre MiApp] [-Db MiAppWeb\mi_base_de_datos.db]
param([string]$Nombre, [string]$Db = "MiAppWeb/mi_base_de_datos.db")
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$env:PATH += [IO.Path]::PathSeparator + (Join-Path $HOME ".dotnet/tools")
$EfVer = "10.0.13"

function Run { & $args[0] $args[1..($args.Count-1)]; if ($LASTEXITCODE) { throw "Falló: $args" } }

if (-not $Nombre) { $Nombre = Read-Host "Nombre del proyecto (sin espacios)" }
if ($Nombre -notmatch '^[A-Za-z][A-Za-z0-9_]*$') { throw "Nombre inválido: '$Nombre'" }
if (Test-Path $Nombre) { throw "Ya existe la carpeta $Nombre" }
if (-not (Test-Path $Db)) { throw "No encuentro la base de datos $Db" }

Write-Host "==> 1/6 Herramientas globales"
$tools = dotnet tool list -g
if (-not ($tools -match '^dotnet-ef ')) { Run dotnet tool install -g dotnet-ef }
if (-not ($tools -match '^dotnet-aspnet-codegenerator ')) { Run dotnet tool install -g dotnet-aspnet-codegenerator }

Write-Host "==> 2/6 Proyecto MVC $Nombre"
Run dotnet new mvc -n $Nombre -f net10.0
Run dotnet sln MiProyectoCompartido.slnx add "$Nombre/$Nombre.csproj"
Copy-Item $Db "$Nombre/app.db"
Set-Location $Nombre

Write-Host "==> 3/6 Paquetes NuGet"
foreach ($p in "Microsoft.EntityFrameworkCore.Sqlite","Microsoft.EntityFrameworkCore.Design",
               "Microsoft.EntityFrameworkCore.Tools","Microsoft.EntityFrameworkCore.SqlServer") {
  Run dotnet add package $p -v $EfVer
}
Run dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
& ../agregar-paquetes.ps1 -Proyecto $Nombre -Numeros (1..13)   # los 13 paquetes extra (incluye ADO.NET y Dapper)
Set-Location "$PSScriptRoot/$Nombre"   # agregar-paquetes.ps1 cambia la carpeta actual

Write-Host "==> 4/6 Scaffolding Database First"
Run dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite `
  -o Models -c AppDbContext --context-dir Data --no-onconfiguring --force

$json = Get-Content appsettings.json -Raw
$json = $json -replace '"AllowedHosts": "\*"', "`"AllowedHosts`": `"*`",`n  `"ConnectionStrings`": {`n    `"DefaultConnection`": `"Data Source=app.db`"`n  }"
Set-Content appsettings.json $json
$prog = Get-Content Program.cs -Raw
$prog = $prog -replace 'builder\.Services\.AddControllersWithViews\(\);', "builder.Services.AddControllersWithViews();`nbuilder.Services.AddDbContext<AppDbContext>(o =>`n    o.UseSqlite(builder.Configuration.GetConnectionString(`"DefaultConnection`")));`nbuilder.Services.AddScoped<AdoService>();"
Set-Content Program.cs ("using Microsoft.EntityFrameworkCore;`nusing $Nombre.Data;`n" + $prog)

# ADO.NET: servicio, controlador y vista de ejemplo (/Ado), antes del CRUD porque el generador compila
Copy-Item -Recurse -Force "$PSScriptRoot/plantillas/ado/*" .
foreach ($f in "Data/AdoService.cs", "Controllers/AdoController.cs") {
  $ruta = (Resolve-Path $f).Path
  [IO.File]::WriteAllText($ruta, [IO.File]::ReadAllText($ruta).Replace("__NOMBRE__", $Nombre))
}

# Enlace en el menú superior de la app
function Nav($ctrl, $texto) {
  $ruta = (Resolve-Path "Views/Shared/_Layout.cshtml").Path
  $html = [IO.File]::ReadAllText($ruta)
  $i = $html.IndexOf("</ul>")
  $li = "    <li class=`"nav-item`"><a class=`"nav-link text-dark`" asp-controller=`"$ctrl`" asp-action=`"Index`">$texto</a></li>`n                    "
  [IO.File]::WriteAllText($ruta, $html.Insert($i, $li))
}

Write-Host "==> 5/6 CRUD por cada tabla"
# Efmigrations* = tablas internas de EF, sin CRUD
Get-ChildItem Models/*.cs | Where-Object { $_.BaseName -ne "ErrorViewModel" -and $_.BaseName -notlike "Efmigrations*" } | ForEach-Object {
  $m = $_.BaseName
  Run dotnet-aspnet-codegenerator controller -name "${m}Controller" -m $m -dc AppDbContext `
    --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries
  Nav $m $m
}
Nav "Ado" "ADO.NET"

Write-Host "==> 6/6 Compilando"
Run dotnet build
Set-Location ..
& ./guardar.ps1 "Nuevo proyecto $Nombre"
Write-Host "`nListo. Para ejecutar:  cd $Nombre ; dotnet watch run"
