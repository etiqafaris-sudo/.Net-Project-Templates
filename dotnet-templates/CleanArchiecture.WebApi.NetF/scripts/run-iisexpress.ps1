param(
    [string]$ProjectPath = "$PSScriptRoot\..\src\CompanyName.Api",
    [int]$Port = 5080
)

$projectPath = (Resolve-Path $ProjectPath).Path
$iisExpress = "${env:ProgramFiles}\IIS Express\iisexpress.exe"

if (-not (Test-Path $iisExpress)) {
    Write-Error "IIS Express not found. Install Visual Studio or the IIS Express standalone package."
    exit 1
}

Write-Host "Starting IIS Express at http://localhost:$Port ($projectPath)"
& $iisExpress /path:$projectPath /port:$Port