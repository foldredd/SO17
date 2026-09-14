param([string]$SqlServer = 'localhost\SQLEXPRESS')
. "$PSScriptRoot\Dev-Helpers.ps1"
Push-Location $script:ProjectRoot
try {
    $tools = Get-FrontendTools
    New-Item -ItemType Directory -Path $script:RuntimeDirectory -Force | Out-Null
    $configPath = Join-Path $script:ProjectRoot 'backend\SO17.Api\appsettings.Local.json'
    if (!(Test-Path -LiteralPath $configPath)) {
        $bytes = New-Object byte[] 32
        $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $generator.GetBytes($bytes) } finally { $generator.Dispose() }
        $invitation = [Convert]::ToBase64String($bytes)
        $config = @{
            ConnectionStrings = @{ DefaultConnection = "Server=$SqlServer;Database=SO17_Refactored;Integrated Security=true;Encrypt=true;TrustServerCertificate=true" }
            Registration = @{ InvitationCode = $invitation }
        }
        $config | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $configPath -Encoding UTF8
    }
    Invoke-Checked 'dotnet' @('restore', 'SO17.sln', '--configfile', 'NuGet.Config')
    Invoke-Checked 'dotnet' @('tool', 'restore', '--configfile', 'NuGet.Config')
    Invoke-Checked $tools.Pnpm @('--dir', 'frontend', 'install', '--frozen-lockfile')
    Write-Host 'Development dependencies and local configuration are ready.'
    Write-Host 'Your invitation code is in backend/SO17.Api/appsettings.Local.json (Registration.InvitationCode).'
} finally { Pop-Location }
