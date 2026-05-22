param(
    [string]$BaseUrl = "http://127.0.0.1:5093"
)

$ErrorActionPreference = "Stop"

$health = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing
if ($health.StatusCode -ne 200) {
    throw "Health endpoint returned HTTP $($health.StatusCode)."
}

$homeResponse = Invoke-WebRequest -Uri "$BaseUrl/" -UseBasicParsing
if ($homeResponse.StatusCode -ne 200) {
    throw "Home page returned HTTP $($homeResponse.StatusCode)."
}

Write-Output "Smoke test passed for $BaseUrl."
