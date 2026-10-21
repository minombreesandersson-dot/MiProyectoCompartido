# Versión Windows de agregar-paquetes.sh. Doble clic en agregar-paquetes.bat
# Uso: .\agregar-paquetes.ps1 [-Proyecto MiApp] [-Numeros 1,3,5]
param([string]$Proyecto, [int[]]$Numeros)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$EfVer = "10.0.13"

$Paquetes = @(
  @(1, "Npgsql.EntityFrameworkCore.PostgreSQL", "", "Base de datos PostgreSQL"),
  @(2, "MySql.EntityFrameworkCore", "", "Base de datos MySQL / MariaDB"),
  @(3, "Microsoft.AspNetCore.Identity.EntityFrameworkCore", $EfVer, "Login: usuarios y roles guardados en la base"),
  @(4, "Microsoft.AspNetCore.Identity.UI", $EfVer, "Login: pantallas listas (registro, ingreso, clave)"),
  @(5, "Microsoft.EntityFrameworkCore.Proxies", $EfVer, "Carga perezosa (lazy loading) de relaciones"),
  @(6, "Microsoft.EntityFrameworkCore.InMemory", $EfVer, "Base en memoria para pruebas"),
  @(7, "X.PagedList.Mvc.Core", "", "Paginación de tablas"),
  @(8, "ClosedXML", "", "Exportar a Excel (.xlsx)"),
  @(9, "QuestPDF", "", "Generar reportes PDF"),
  @(10, "Swashbuckle.AspNetCore", "", "Swagger: documentar y probar una API"),
  @(11, "Serilog.AspNetCore", "", "Logs en archivo y consola")
)

if (-not $Proyecto) {
  $lista = Get-ChildItem -Directory | Where-Object { Test-Path "$($_.Name)\$($_.Name).csproj" } | ForEach-Object Name
  Write-Host "Proyectos: $($lista -join ', ')"
  $Proyecto = Read-Host "¿A cuál?"
}
$csproj = "$Proyecto\$Proyecto.csproj"
if (-not (Test-Path $csproj)) { throw "No encuentro $csproj" }

if (-not $Numeros) {
  foreach ($p in $Paquetes) { "{0,3}) {1,-50} {2}" -f $p[0], $p[1], $p[3] | Write-Host }
  $resp = Read-Host "Números separados por espacio (Enter = ninguno)"
  $Numeros = $resp -split '\s+' | Where-Object { $_ } | ForEach-Object { [int]$_ }
}

foreach ($p in $Paquetes | Where-Object { $Numeros -contains $_[0] }) {
  Write-Host "==> $($p[1])  ($($p[3]))"
  if ($p[2]) { dotnet add $csproj package $p[1] -v $p[2] } else { dotnet add $csproj package $p[1] }
  if ($LASTEXITCODE) { throw "Falló $($p[1])" }
}
dotnet list $csproj package
