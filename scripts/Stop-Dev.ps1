. "$PSScriptRoot\Dev-Helpers.ps1"
$statePath = Join-Path $script:RuntimeDirectory 'processes.json'
if (!(Test-Path -LiteralPath $statePath)) { Write-Host 'No managed processes. For a manually started server, press Ctrl+C in its terminal.'; return }
$records = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
foreach ($record in $records) {
    $process = Get-Process -Id $record.Id -ErrorAction SilentlyContinue
    if ($process -and $process.Path -eq $record.Executable -and $process.StartTime.ToUniversalTime().ToString('o') -eq $record.StartedAt) {
        & taskkill.exe /PID $record.Id /T /F
    }
}
Remove-Item -LiteralPath $statePath
Write-Host 'Development processes stopped. SQL Server service remains available.'
