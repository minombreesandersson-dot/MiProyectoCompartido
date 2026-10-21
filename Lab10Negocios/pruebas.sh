#!/usr/bin/env bash
# Casos de prueba mínimos del Laboratorio 10 contra la app en ejecución y la base Negocios.
# Uso: ./pruebas.sh [url]   (la app debe estar corriendo: dotnet run --urls http://localhost:5210)
# ponytail: la base se consulta con sqlcmd dentro del contenedor docker "mssql-lab09"; ajustar si tu SQL Server es otro.
set -uo pipefail
U="${1:-http://localhost:5210}"
CK=$(mktemp); trap 'rm -f "$CK"' EXIT
sq() { docker exec mssql-lab09 bash -c "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"\$MSSQL_SA_PASSWORD\" -C -d Negocios -h -1 -W -Q \"SET NOCOUNT ON; $1\"" | tr -d '\r'; }
token() { curl -s -c "$CK" "$U/Pedidos/Create" | grep -oP 'name="__RequestVerificationToken" type="hidden" value="\K[^"]+'; }
# pedir "campos extra..." -> imprime "código_http ubicación"
pedir() { curl -s -b "$CK" -o /tmp/lab10-resp.html -w '%{http_code} %{redirect_url}' --data-urlencode "__RequestVerificationToken=$(token)" "$@" "$U/Pedidos/Create"; }
errores() { grep -oP '<li>\K[^<]+' /tmp/lab10-resp.html | sed 's/&quot;/"/g; s/&#xF3;/ó/g; s/&#xE1;/á/g; s/&#xFA;/ú/g; s/&#xED;/í/g; s/&#xE9;/é/g; s/&#xBA;/º/g' | paste -sd'|'; }
ok=0; mal=0
caso() { if [ "$2" = si ]; then ok=$((ok+1)); echo "✔ Caso $1"; else mal=$((mal+1)); echo "✘ Caso $1"; fi; }

ped0=$(sq "SELECT COUNT(*) FROM PEDIDO"); det0=$(sq "SELECT COUNT(*) FROM DETALLEPEDIDO")
st1=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=1")
echo "Inicio: $ped0 pedidos, $det0 detalles, stock producto 1 = $st1"

echo; echo "== 1. Pedido con un producto y stock suficiente (2 x producto 1)"
r=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=1' -d 'Lineas[0].Cantidad=2'); echo "   respuesta: $r"
id1=${r##*/}; st=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=1")
echo "   pedido $id1: $(sq "SELECT COUNT(*) FROM DETALLEPEDIDO WHERE IdPedido=$id1") detalle(s), stock producto 1: $st1 -> $st"
caso 1 $([[ $r == 302* && $st -eq $((st1-2)) ]] && echo si || echo no)

echo; echo "== 2. Cantidad mayor que el stock (2 x producto 1 válido + 9999 x producto 2)"
st1=$st; st2=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=2"); p=$(sq "SELECT COUNT(*) FROM PEDIDO")
r=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=1' -d 'Lineas[0].Cantidad=2' -d 'Lineas[1].IdProducto=2' -d 'Lineas[1].Cantidad=9999')
echo "   respuesta: $r  mensaje: $(errores)"
d=$(sq "SELECT COUNT(*) FROM PEDIDO"); a=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=1"); b=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=2")
echo "   pedidos $p -> $d, stock producto 1 $st1 -> $a, stock producto 2 $st2 -> $b (sin registros parciales)"
caso 2 $([[ $r == 200* && $d -eq $p && $a -eq $st1 && $b -eq $st2 ]] && echo si || echo no)

echo; echo "== 3. Cantidad cero y negativa"
r0=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=1' -d 'Lineas[0].Cantidad=0'); m0=$(errores)
rn=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=1' -d 'Lineas[0].Cantidad=-5'); mn=$(errores)
echo "   cero: $r0 -> $m0"; echo "   negativa: $rn -> $mn"
caso 3 $([[ $r0 == 200* && $rn == 200* && $(sq "SELECT COUNT(*) FROM PEDIDO") -eq $p ]] && echo si || echo no)

echo; echo "== 4. Pedido sin detalles"
r=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1); echo "   respuesta: $r  mensaje: $(errores)"
caso 4 $([[ $r == 200* && $(sq "SELECT COUNT(*) FROM PEDIDO") -eq $p ]] && echo si || echo no)

echo; echo "== 5. Precio modificado en el navegador (envía S/ 1, el real es otro)"
real=$(sq "SELECT CAST(PrecioUnidad AS int) FROM PRODUCTO WHERE IdProducto=1")
r=$(pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=1' -d 'Lineas[0].Cantidad=1' -d 'Lineas[0].PrecioUnidad=1')
id5=${r##*/}; guardado=$(sq "SELECT CAST(PrecioUnidad AS int) FROM DETALLEPEDIDO WHERE IdPedido=$id5")
echo "   respuesta: $r  precio enviado: 1, guardado: $guardado, precio en PRODUCTO: $real"
caso 5 $([[ $r == 302* && $guardado -eq $real ]] && echo si || echo no)

echo; echo "== 6. Consultar el pedido registrado en el caso 1"
html=$(curl -s "$U/Pedidos/Details/$id1")
echo "   $(echo "$html" | grep -oP '<h1>\K[^<]+') | total: $(echo "$html" | grep -oP 'fw-bold.*?Total</td><td class="text-end">\K[^<]+')"
caso 6 $(echo "$html" | grep -q "Pedido N.º $id1\|Pedido N.&#xBA; $id1" && echo si || echo no)

echo; echo "== Extra: dos usuarios piden a la vez todo el stock de un mismo producto"
orig3=$(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=3")
sq "UPDATE PRODUCTO SET UnidadesEnExistencia = 5 WHERE IdProducto = 3" >/dev/null
p=$(sq "SELECT COUNT(*) FROM PEDIDO")
for i in 1 2; do ( pedir -d IdCliente=ALFKI -d IdEmpleado=1 -d 'Lineas[0].IdProducto=3' -d 'Lineas[0].Cantidad=5' > /tmp/lab10-c$i ) & done; wait
echo "   usuario 1: $(cut -c1-3 /tmp/lab10-c1)  usuario 2: $(cut -c1-3 /tmp/lab10-c2)  pedidos nuevos: $(( $(sq "SELECT COUNT(*) FROM PEDIDO") - p ))  stock final: $(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=3")"
caso extra $([[ $(( $(sq "SELECT COUNT(*) FROM PEDIDO") - p )) -eq 1 && $(sq "SELECT UnidadesEnExistencia FROM PRODUCTO WHERE IdProducto=3") -eq 0 ]] && echo si || echo no)
sq "UPDATE PRODUCTO SET UnidadesEnExistencia = $orig3 WHERE IdProducto = 3" >/dev/null   # deja el stock como estaba

echo; echo "Resultado: $ok correctos, $mal fallidos"
[ $mal -eq 0 ]
