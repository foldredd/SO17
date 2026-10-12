. "$PSScriptRoot\Dev-Helpers.ps1"
Push-Location $script:ProjectRoot
try {
    $tools = Get-FrontendTools
    Invoke-Checked 'dotnet' @('restore', 'SO17.sln')
    Invoke-Checked 'dotnet' @('build', 'SO17.sln', '--configuration', 'Release', '--no-restore')
    Invoke-Checked 'dotnet' @('list', 'SO17.sln', 'package', '--vulnerable', '--include-transitive', '--no-restore')
    $testArguments = @('test', 'SO17.sln', '--configuration', 'Release', '--no-restore', '--no-build', '--logger', 'trx;LogFileName=api-tests.trx')
    if (Test-PortInUse 5080) {
        Write-Host 'The API is running. Tests will use the current compiled build.'
    }
    Invoke-Checked 'dotnet' $testArguments
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'lint')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'test')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'build')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'audit', '--prod')
    Write-Host 'All automated checks passed.'
} finally { Pop-Location }
