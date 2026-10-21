#!/usr/bin/env bash
# Agrega paquetes NuGet extra a un proyecto, eligiéndolos de una lista.
# Uso: ./agregar-paquetes.sh [CarpetaProyecto] [números...]
#   ./agregar-paquetes.sh SistemaAlquilerVehiculos        (muestra el menú)
#   ./agregar-paquetes.sh MiApp 1 3 5                     (sin preguntar)
set -euo pipefail
cd "$(dirname "$0")"
EF_VER=10.0.13   # misma versión de EF Core en todos los proyectos

# número | paquete | versión ("" = la última) | para qué sirve
PAQUETES=(
  "1|Npgsql.EntityFrameworkCore.PostgreSQL||Base de datos PostgreSQL"
  "2|MySql.EntityFrameworkCore||Base de datos MySQL / MariaDB"
  "3|Microsoft.AspNetCore.Identity.EntityFrameworkCore|$EF_VER|Login: usuarios y roles guardados en la base"
  "4|Microsoft.AspNetCore.Identity.UI|$EF_VER|Login: pantallas listas (registro, ingreso, clave)"
  "5|Microsoft.EntityFrameworkCore.Proxies|$EF_VER|Carga perezosa (lazy loading) de relaciones"
  "6|Microsoft.EntityFrameworkCore.InMemory|$EF_VER|Base en memoria para pruebas"
  "7|X.PagedList.Mvc.Core||Paginación de tablas (Página 1, 2, 3…)"
  "8|ClosedXML||Exportar a Excel (.xlsx)"
  "9|QuestPDF||Generar reportes PDF (boletas, listados)"
  "10|Swashbuckle.AspNetCore||Swagger: documentar y probar una API"
  "11|Serilog.AspNetCore||Logs en archivo y consola"
)

PROY="${1:-}"
if [ -z "$PROY" ]; then
  mapfile -t LISTA < <(ls -d */ | tr -d / | while read -r d; do [ -f "$d/$d.csproj" ] && echo "$d"; done)
  if [ -n "${DISPLAY:-}" ] && command -v zenity >/dev/null; then
    PROY=$(zenity --list --title="Agregar paquetes" --text="¿A qué proyecto?" --column=Proyecto "${LISTA[@]}") || exit 0
  else
    echo "Proyectos: ${LISTA[*]}"; read -rp "¿A cuál? " PROY
  fi
fi
CSPROJ="$PROY/$PROY.csproj"
[ -f "$CSPROJ" ] || { echo "No encuentro $CSPROJ"; exit 1; }

shift || true
ELEGIDOS=("$@")
if [ ${#ELEGIDOS[@]} -eq 0 ]; then
  if [ -n "${DISPLAY:-}" ] && command -v zenity >/dev/null; then
    FILAS=(); for p in "${PAQUETES[@]}"; do IFS='|' read -r n nom _ desc <<<"$p"; FILAS+=(FALSE "$n" "$nom" "$desc"); done
    SEL=$(zenity --list --checklist --width=760 --height=480 --title="Paquetes para $PROY" \
      --text="Marca los que quieras instalar" --column="" --column="N°" --column=Paquete --column="Para qué sirve" \
      --print-column=2 --separator=" " "${FILAS[@]}") || exit 0
    read -ra ELEGIDOS <<<"$SEL"
  else
    for p in "${PAQUETES[@]}"; do IFS='|' read -r n nom _ desc <<<"$p"; printf '%3s) %-50s %s\n' "$n" "$nom" "$desc"; done
    read -rp "Números separados por espacio (Enter = ninguno): " -a ELEGIDOS
  fi
fi

for n in "${ELEGIDOS[@]}"; do
  for p in "${PAQUETES[@]}"; do
    IFS='|' read -r num nom ver desc <<<"$p"
    [ "$num" = "$n" ] || continue
    echo "==> $nom  ($desc)"
    dotnet add "$CSPROJ" package "$nom" ${ver:+-v "$ver"}
  done
done
echo "Paquetes de $PROY:"; dotnet list "$CSPROJ" package
