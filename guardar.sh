#!/usr/bin/env bash
# Guarda todo en GitHub: add + commit con fecha + push.
# Uso: ./guardar.sh ["mensaje"]   |   ./guardar.sh --auto  (guarda solo cada 15 min vía cron)
#      ./guardar.sh --no-auto     (quita el guardado automático)
set -euo pipefail
cd "$(dirname "$0")"
DIR=$(pwd)

case "${1:-}" in
  --auto)
    (crontab -l 2>/dev/null | grep -v "$DIR/guardar.sh"; echo "*/15 * * * * $DIR/guardar.sh >> $DIR/.guardar.log 2>&1") | crontab -
    echo "Guardado automático activado (cada 15 min). Log: $DIR/.guardar.log"; exit 0 ;;
  --no-auto)
    crontab -l 2>/dev/null | grep -v "$DIR/guardar.sh" | crontab - || true
    echo "Guardado automático desactivado."; exit 0 ;;
esac

git add -A
if git diff --cached --quiet; then echo "Nada nuevo que guardar."; exit 0; fi
git commit -q -m "${1:-Guardado $(date '+%Y-%m-%d %H:%M')}"
if ! git pull -q --rebase origin "$(git branch --show-current)"; then
  git rebase --abort 2>/dev/null || true
  echo "CONFLICTO con cambios de un compañero: tu guardado quedó local. Resuélvelo con: git pull --rebase"; exit 1
fi
git push -q origin HEAD
echo "Guardado en GitHub: $(git log -1 --format='%h %s')"
