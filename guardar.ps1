# Versión Windows de guardar.sh. Doble clic en guardar.bat
param([string]$Mensaje = "Guardado $(Get-Date -Format 'yyyy-MM-dd HH:mm')")
Set-Location $PSScriptRoot
git add -A
git diff --cached --quiet
if ($LASTEXITCODE -eq 0) { Write-Host "Nada nuevo que guardar."; exit 0 }
git commit -q -m $Mensaje
git push -q origin HEAD
Write-Host "Guardado en GitHub: $(git log -1 --format='%h %s')"
