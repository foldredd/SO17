. "$PSScriptRoot\Dev-Helpers.ps1"
Push-Location $script:ProjectRoot
try {
    $tools = Get-FrontendTools
    $testArguments = @('test', 'SO17.sln', '--no-restore', '--logger', 'trx;LogFileName=api-tests.trx')
    if (Test-PortInUse 5080) {
        Write-Host 'The API is running. Tests will use the current compiled build.'
        $testArguments += '--no-build'
    }
    Invoke-Checked 'dotnet' $testArguments
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'lint')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'test')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'build')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'audit', '--prod')
    Write-Host 'All automated checks passed.'
} finally { Pop-Location }
