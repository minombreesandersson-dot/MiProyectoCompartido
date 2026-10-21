# Versión Windows de guardar.sh. Doble clic en guardar.bat
param([string]$Mensaje = "Guardado $(Get-Date -Format 'yyyy-MM-dd HH:mm')")
Set-Location $PSScriptRoot
git add -A
git diff --cached --quiet
if ($LASTEXITCODE -eq 0) { Write-Host "Nada nuevo que guardar."; exit 0 }
git commit -q -m $Mensaje
git pull -q --rebase origin (git branch --show-current)
if ($LASTEXITCODE) {
  git rebase --abort 2>$null
  Write-Host "CONFLICTO con cambios de un compañero: tu guardado quedó local. Resuélvelo con: git pull --rebase"; exit 1
}
git push -q origin HEAD
Write-Host "Guardado en GitHub: $(git log -1 --format='%h %s')"
