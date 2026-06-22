#Requires -Version 7
<#
  Starts Weather.Api and Weather.Mcp, waits until both are ready,
  then runs Weather.Console. Stops both services when the console exits.
#>

$root = $PSScriptRoot

function Wait-ForHttp([string]$url, [int]$timeoutSec = 30) {
    $deadline = [DateTime]::Now.AddSeconds($timeoutSec)
    while ([DateTime]::Now -lt $deadline) {
        try { Invoke-RestMethod $url -TimeoutSec 2 -ErrorAction Stop; return } catch {}
        Start-Sleep -Milliseconds 500
    }
    throw "Timed out waiting for $url"
}

function Wait-ForPort([int]$port, [int]$timeoutSec = 30) {
    $deadline = [DateTime]::Now.AddSeconds($timeoutSec)
    while ([DateTime]::Now -lt $deadline) {
        try {
            $tcp = [System.Net.Sockets.TcpClient]::new()
            $tcp.Connect("localhost", $port)
            $tcp.Dispose()
            return
        } catch {}
        Start-Sleep -Milliseconds 500
    }
    throw "Timed out waiting for port $port"
}

Write-Host "Starting Weather.Api on http://localhost:5150..."
$api = Start-Process dotnet -ArgumentList "run","--project","src/Weather.Api","--urls","http://localhost:5150" `
    -WorkingDirectory $root -PassThru -WindowStyle Hidden
Wait-ForHttp "http://localhost:5150/weatherforecast?city=Amsterdam&days=1"
Write-Host "Weather.Api ready."

Write-Host "Starting Weather.Mcp on http://localhost:5151..."
$env:WeatherApi__BaseUrl = "http://localhost:5150"
$mcp = Start-Process dotnet -ArgumentList "run","--project","src/Weather.Mcp","--urls","http://localhost:5151" `
    -WorkingDirectory $root -PassThru -WindowStyle Hidden
Wait-ForPort 5151
Write-Host "Weather.Mcp ready."

Write-Host ""
try {
    dotnet run --project "$root/src/Weather.Console"
} finally {
    Write-Host "`nStopping services..."
    $api, $mcp | Where-Object { -not $_.HasExited } | ForEach-Object { $_.Kill() }
}
