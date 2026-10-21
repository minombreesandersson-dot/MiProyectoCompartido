# Versión Windows de crear-proyecto.sh (PowerShell). Doble clic en crear-proyecto.bat
# Uso: .\crear-proyecto.ps1 [-Nombre MiApp] [-Db MiAppWeb\mi_base_de_datos.db]
param([string]$Nombre, [string]$Db = "MiAppWeb\mi_base_de_datos.db")
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
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
Run dotnet sln MiProyectoCompartido.slnx add "$Nombre\$Nombre.csproj"
Copy-Item $Db "$Nombre\app.db"
Set-Location $Nombre

Write-Host "==> 3/6 Paquetes NuGet"
foreach ($p in "Microsoft.EntityFrameworkCore.Sqlite","Microsoft.EntityFrameworkCore.Design",
               "Microsoft.EntityFrameworkCore.Tools","Microsoft.EntityFrameworkCore.SqlServer") {
  Run dotnet add package $p -v $EfVer
}
Run dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
& ..\agregar-paquetes.ps1 -Proyecto $Nombre -Numeros (1..11)   # los 11 paquetes extra
Set-Location "$PSScriptRoot\$Nombre"   # agregar-paquetes.ps1 cambia la carpeta actual

Write-Host "==> 4/6 Scaffolding Database First"
Run dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite `
  -o Models -c AppDbContext --context-dir Data --no-onconfiguring --force

$json = Get-Content appsettings.json -Raw
$json = $json -replace '"AllowedHosts": "\*"', "`"AllowedHosts`": `"*`",`n  `"ConnectionStrings`": {`n    `"DefaultConnection`": `"Data Source=app.db`"`n  }"
Set-Content appsettings.json $json
$prog = Get-Content Program.cs -Raw
$prog = $prog -replace 'builder\.Services\.AddControllersWithViews\(\);', "builder.Services.AddControllersWithViews();`nbuilder.Services.AddDbContext<AppDbContext>(o =>`n    o.UseSqlite(builder.Configuration.GetConnectionString(`"DefaultConnection`")));"
Set-Content Program.cs ("using Microsoft.EntityFrameworkCore;`nusing $Nombre.Data;`n" + $prog)

Write-Host "==> 5/6 CRUD por cada tabla"
Get-ChildItem Models\*.cs | Where-Object BaseName -ne "ErrorViewModel" | ForEach-Object {
  $m = $_.BaseName
  Run dotnet-aspnet-codegenerator controller -name "${m}Controller" -m $m -dc AppDbContext `
    --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries
}

Write-Host "==> 6/6 Compilando"
Run dotnet build
Set-Location ..
& .\guardar.ps1 "Nuevo proyecto $Nombre"
Write-Host "`nListo. Para ejecutar:  cd $Nombre ; dotnet watch run"
