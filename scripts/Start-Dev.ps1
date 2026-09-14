param([switch]$SkipBuild)
. "$PSScriptRoot\Dev-Helpers.ps1"
Push-Location $script:ProjectRoot
$started = @()
try {
    $tools = Get-FrontendTools
    foreach ($port in @(5080, 5173)) {
        if (Test-PortInUse $port) { throw "Port $port is occupied. Run scripts/Stop-Dev.ps1 or stop the existing terminal with Ctrl+C." }
    }
    $configPath = Join-Path $script:ProjectRoot 'backend\SO17.Api\appsettings.Local.json'
    if (!(Test-Path -LiteralPath $configPath) -or !(Test-Path 'frontend/node_modules')) { & "$PSScriptRoot\Setup-Dev.ps1" }
    New-Item -ItemType Directory -Path $script:RuntimeDirectory -Force | Out-Null
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = 'http://localhost:5080'
    if (!$SkipBuild) { Invoke-Checked 'dotnet' @('build', 'SO17.sln', '--no-restore', '--nologo') }
    Invoke-Checked 'dotnet' @('run', '--project', 'backend/SO17.Api', '--no-build', '--no-restore', '--', '--migrate')
    $api = Start-Process -FilePath 'dotnet' -ArgumentList @('run', '--project', 'backend/SO17.Api', '--no-build', '--no-restore') -WorkingDirectory $script:ProjectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $script:RuntimeDirectory 'api.log') -RedirectStandardError (Join-Path $script:RuntimeDirectory 'api.error.log')
    $started += $api
    Wait-Http 'http://localhost:5080/health/ready'
    $vite = Join-Path $script:ProjectRoot 'frontend\node_modules\vite\bin\vite.js'
    $web = Start-Process -FilePath $tools.Node -ArgumentList @(('"' + $vite + '"'), '--host', 'localhost', '--port', '5173') -WorkingDirectory (Join-Path $script:ProjectRoot 'frontend') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $script:RuntimeDirectory 'frontend.log') -RedirectStandardError (Join-Path $script:RuntimeDirectory 'frontend.error.log')
    $started += $web
    Wait-Http 'http://localhost:5173'
    $records = @($started | ForEach-Object { @{ Id = $_.Id; StartedAt = $_.StartTime.ToUniversalTime().ToString('o'); Executable = $_.Path } })
    ConvertTo-Json -InputObject $records | Set-Content -LiteralPath (Join-Path $script:RuntimeDirectory 'processes.json') -Encoding UTF8
    Write-Host ''
    Write-Host 'READY: http://localhost:5173'
    Write-Host 'API: http://localhost:5080/openapi/v1.json'
    Write-Host 'To register, copy Registration.InvitationCode from backend/SO17.Api/appsettings.Local.json.'
    Write-Host 'Stop both processes: .\scripts\Stop-Dev.ps1'
} catch {
    foreach ($process in $started) {
        if (!$process.HasExited) { & taskkill.exe /PID $process.Id /T /F | Out-Null }
    }
    throw
} finally { Pop-Location }
