# Crea el repositorio, el primer commit y una rama por cada módulo.
# Antes: crear en GitHub un repositorio VACÍO (sin README ni .gitignore).
# Uso, desde esta carpeta:
#   powershell -ExecutionPolicy Bypass -File .\crear-ramas.ps1 -Remoto "https://github.com/TU_USUARIO/SIGEBI.git"
param(
    [Parameter(Mandatory = $true)]
    [string]$Remoto
)

$ramas = @(
    "feature/usuarios",
    "feature/catalogo",
    "feature/prestamos",
    "feature/devoluciones",
    "feature/penalizaciones",
    "feature/notificaciones",
    "feature/reportes-auditoria"
)

git init -b main
git add .
git commit -m "Arquitectura en capas y entidades del dominio"
git remote add origin $Remoto

foreach ($rama in $ramas) { git branch $rama }

git push -u origin main
foreach ($rama in $ramas) { git push -u origin $rama }

Write-Host "Listo: main y $($ramas.Count) ramas publicadas."
