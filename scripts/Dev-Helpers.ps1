Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script:ProjectRoot = Split-Path -Parent $PSScriptRoot
$script:RuntimeDirectory = Join-Path $script:ProjectRoot '.local'

function Invoke-Checked {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed with exit code $LASTEXITCODE" }
}

function Get-FrontendTools {
    $runtime = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies'
    $node = Get-Command node -ErrorAction SilentlyContinue
    $pnpm = Get-Command pnpm.cmd -ErrorAction SilentlyContinue
    $nodePath = if ($node) { $node.Source } else { Join-Path $runtime 'node\bin\node.exe' }
    $pnpmPath = if ($pnpm) { $pnpm.Source } else { Join-Path $runtime 'bin\fallback\pnpm.cmd' }
    if (!(Test-Path -LiteralPath $nodePath) -or !(Test-Path -LiteralPath $pnpmPath)) {
        throw 'Install Node.js 24 LTS and pnpm 11.19.0, then reopen PowerShell. See README.md.'
    }
    $env:PATH = (Split-Path $nodePath) + ';' + $env:PATH
    return @{ Node = $nodePath; Pnpm = $pnpmPath }
}

function Test-PortInUse([int]$Port) {
    $client = New-Object System.Net.Sockets.TcpClient
    try { $client.Connect('127.0.0.1', $Port); return $true } catch { return $false } finally { $client.Dispose() }
}

function Wait-Http([string]$Url, [int]$Seconds = 40) {
    $deadline = [DateTime]::UtcNow.AddSeconds($Seconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        try { $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2; if ($response.StatusCode -eq 200) { return } } catch { }
        Start-Sleep -Milliseconds 500
    }
    throw "Startup failed: $Url. Read logs in .local."
}
