$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
$TestProjectFile = Join-Path $ProjectRoot "tests\MovieWatchlistTracker.Tests\MovieWatchlistTracker.Tests.csproj"
$StartBat = Join-Path $ProjectRoot "START_HERE.bat"
$AppHost = "127.0.0.1"
$PreferredPort = 5093
$FallbackPortLimit = 5100
$DotnetSdkPackage = "Microsoft.DotNet.SDK.10"

function Write-Heading {
    param([string]$Text)

    Write-Host ""
    Write-Host "== $Text ==" -ForegroundColor Cyan
}

function Read-YesNo {
    param(
        [string]$Prompt,
        [bool]$DefaultYes = $false
    )

    $suffix = if ($DefaultYes) { "[Y/n]" } else { "[y/N]" }

    while ($true) {
        $answer = Read-Host "$Prompt $suffix"

        if ([string]::IsNullOrWhiteSpace($answer)) {
            return $DefaultYes
        }

        switch ($answer.Trim().ToLowerInvariant()) {
            "y" { return $true }
            "yes" { return $true }
            "n" { return $false }
            "no" { return $false }
            default { Write-Host "Please answer y or n." -ForegroundColor Yellow }
        }
    }
}

function Get-DotnetCommand {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $programFilesDotnet = Join-Path $env:ProgramFiles "dotnet\dotnet.exe"
    if (Test-Path $programFilesDotnet) {
        return $programFilesDotnet
    }

    return $null
}

function Test-DotnetSdk10 {
    $dotnet = Get-DotnetCommand
    if (-not $dotnet) {
        return $false
    }

    try {
        $sdks = & $dotnet --list-sdks 2>$null
        return [bool]($sdks | Where-Object { $_ -match "^10\." })
    }
    catch {
        return $false
    }
}

function Install-DotnetSdk10 {
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) {
        Write-Host "winget was not found on this machine." -ForegroundColor Red
        Write-Host "Install the .NET 10 SDK manually, then run START_HERE.bat again:"
        Write-Host "https://dotnet.microsoft.com/download"
        return $false
    }

    Write-Host "Installing .NET 10 SDK with winget. Windows may show prompts during installation."
    & winget install --id $DotnetSdkPackage --exact --source winget --accept-package-agreements --accept-source-agreements

    if ($LASTEXITCODE -ne 0) {
        Write-Host "winget did not complete successfully. Exit code: $LASTEXITCODE" -ForegroundColor Red
        return $false
    }

    if (Test-DotnetSdk10) {
        Write-Host ".NET 10 SDK is now available." -ForegroundColor Green
        return $true
    }

    Write-Host "The installer completed, but this terminal still cannot see the .NET 10 SDK." -ForegroundColor Yellow
    Write-Host "Close this window and run START_HERE.bat again. If it still fails, restart Windows."
    return $false
}

function New-DesktopShortcut {
    if (-not (Test-Path $StartBat)) {
        Write-Host "Could not find START_HERE.bat at $StartBat" -ForegroundColor Red
        return
    }

    $desktop = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = Join-Path $desktop "MovieWatchlistTracker.lnk"

    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $StartBat
    $shortcut.WorkingDirectory = $ProjectRoot
    $shortcut.Description = "Start MovieWatchlistTracker setup and local server"
    $shortcut.Save()

    Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green
}

function Test-PortAvailable {
    param([int]$Port)

    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Parse($AppHost), $Port)
    try {
        $listener.Start()
        return $true
    }
    catch {
        return $false
    }
    finally {
        $listener.Stop()
    }
}

function Get-AvailableAppUrl {
    foreach ($port in $PreferredPort..$FallbackPortLimit) {
        if (Test-PortAvailable $port) {
            return "http://$AppHost`:$port"
        }
    }

    return $null
}

function Open-AppInDefaultBrowserWhenReady {
    param([string]$Url)

    Start-Job -ScriptBlock {
        param([string]$AppUrl)

        $deadline = (Get-Date).AddSeconds(60)

        while ((Get-Date) -lt $deadline) {
            try {
                $request = [System.Net.WebRequest]::Create($AppUrl)
                $request.Timeout = 1000

                $response = $request.GetResponse()
                $response.Close()

                Start-Process $AppUrl
                return
            }
            catch {
                Start-Sleep -Milliseconds 750
            }
        }
    } -ArgumentList $Url | Out-Null
}

function Show-ManualCommands {
    Write-Heading "Manual Commands"
    Write-Host "Run these from the project folder:"
    Write-Host "cd `"$ProjectRoot`""
    Write-Host "dotnet clean .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet restore .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet restore .\tests\MovieWatchlistTracker.Tests\MovieWatchlistTracker.Tests.csproj"
    Write-Host "dotnet build .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet run --project .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj --urls http://$AppHost`:$PreferredPort"
    Write-Host ""
    Write-Host "Then open http://$AppHost`:$PreferredPort in your browser."
    Write-Host "If that port is busy, change 5093 to another free port, such as 5094 through $FallbackPortLimit."
}

function Start-WebApp {
    $dotnet = Get-DotnetCommand
    if (-not $dotnet) {
        Write-Host "dotnet was not found, so the server cannot be started." -ForegroundColor Red
        Show-ManualCommands
        return
    }

    Write-Heading "Preparing Project"
    Push-Location $ProjectRoot
    try {
        Write-Host "Cleaning stale web build outputs..."
        & $dotnet clean $ProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet clean failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Restoring web project dependencies..."
        & $dotnet restore $ProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore for the web project failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Restoring test project dependencies..."
        & $dotnet restore $TestProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore for the test project failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Building web project..."
        & $dotnet build $ProjectFile --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }

        $appUrl = Get-AvailableAppUrl
        if (-not $appUrl) {
            Write-Host ""
            Write-Host "Could not find a free port from $PreferredPort to $FallbackPortLimit." -ForegroundColor Red
            Write-Host "Close other running copies of the app, then run START_HERE.bat again."
            Show-ManualCommands
            return
        }

        Write-Host ""
        if ($appUrl -ne "http://$AppHost`:$PreferredPort") {
            Write-Host "Port $PreferredPort is busy, so the launcher will use $appUrl instead." -ForegroundColor Yellow
        }

        Write-Host "Starting MovieWatchlistTracker at $appUrl"
        Write-Host "Your default browser will open automatically when the app is ready."
        Write-Host "Leave this window open while using the app. Press Ctrl+C here to stop the server."
        Write-Host ""

        $env:ASPNETCORE_ENVIRONMENT = "Development"
        Open-AppInDefaultBrowserWhenReady $appUrl
        & $dotnet run --no-build --project $ProjectFile --urls $appUrl
    }
    finally {
        Pop-Location
    }
}

Write-Heading "MovieWatchlistTracker Setup"
Write-Host "This script can create a desktop shortcut, check for the .NET 10 SDK, and start the local web server."

if (Read-YesNo "Create desktop shortcut?") {
    New-DesktopShortcut
}
else {
    Write-Host "Skipping desktop shortcut."
}

Write-Heading ".NET SDK Check"
$hasDotnet10 = Test-DotnetSdk10

if ($hasDotnet10) {
    Write-Host ".NET 10 SDK found." -ForegroundColor Green
}
else {
    Write-Host ".NET 10 SDK was not found." -ForegroundColor Yellow
    if (Read-YesNo "Install .NET 10 SDK now with winget?") {
        $hasDotnet10 = Install-DotnetSdk10
    }
}

Write-Heading "Server"
if (Read-YesNo "Start server now?") {
    if (-not $hasDotnet10) {
        Write-Host "The .NET 10 SDK is still missing, so starting the server will probably fail." -ForegroundColor Yellow

        if (Read-YesNo "Install .NET 10 SDK now with winget?") {
            $hasDotnet10 = Install-DotnetSdk10
        }
    }

    if ($hasDotnet10) {
        Start-WebApp
    }
    else {
        Write-Host "Server was not started because the .NET 10 SDK is not available." -ForegroundColor Red
        Show-ManualCommands
    }
}
else {
    Show-ManualCommands
}$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
$TestProjectFile = Join-Path $ProjectRoot "tests\MovieWatchlistTracker.Tests\MovieWatchlistTracker.Tests.csproj"
$StartBat = Join-Path $ProjectRoot "START_HERE.bat"
$AppHost = "127.0.0.1"
$PreferredPort = 5093
$FallbackPortLimit = 5100
$DotnetSdkPackage = "Microsoft.DotNet.SDK.10"

function Write-Heading {
    param([string]$Text)

    Write-Host ""
    Write-Host "== $Text ==" -ForegroundColor Cyan
}

function Read-YesNo {
    param(
        [string]$Prompt,
        [bool]$DefaultYes = $false
    )

    $suffix = if ($DefaultYes) { "[Y/n]" } else { "[y/N]" }

    while ($true) {
        $answer = Read-Host "$Prompt $suffix"

        if ([string]::IsNullOrWhiteSpace($answer)) {
            return $DefaultYes
        }

        switch ($answer.Trim().ToLowerInvariant()) {
            "y" { return $true }
            "yes" { return $true }
            "n" { return $false }
            "no" { return $false }
            default { Write-Host "Please answer y or n." -ForegroundColor Yellow }
        }
    }
}

function Get-DotnetCommand {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $programFilesDotnet = Join-Path $env:ProgramFiles "dotnet\dotnet.exe"
    if (Test-Path $programFilesDotnet) {
        return $programFilesDotnet
    }

    return $null
}

function Test-DotnetSdk10 {
    $dotnet = Get-DotnetCommand
    if (-not $dotnet) {
        return $false
    }

    try {
        $sdks = & $dotnet --list-sdks 2>$null
        return [bool]($sdks | Where-Object { $_ -match "^10\." })
    }
    catch {
        return $false
    }
}

function Install-DotnetSdk10 {
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) {
        Write-Host "winget was not found on this machine." -ForegroundColor Red
        Write-Host "Install the .NET 10 SDK manually, then run START_HERE.bat again:"
        Write-Host "https://dotnet.microsoft.com/download"
        return $false
    }

    Write-Host "Installing .NET 10 SDK with winget. Windows may show prompts during installation."
    & winget install --id $DotnetSdkPackage --exact --source winget --accept-package-agreements --accept-source-agreements

    if ($LASTEXITCODE -ne 0) {
        Write-Host "winget did not complete successfully. Exit code: $LASTEXITCODE" -ForegroundColor Red
        return $false
    }

    if (Test-DotnetSdk10) {
        Write-Host ".NET 10 SDK is now available." -ForegroundColor Green
        return $true
    }

    Write-Host "The installer completed, but this terminal still cannot see the .NET 10 SDK." -ForegroundColor Yellow
    Write-Host "Close this window and run START_HERE.bat again. If it still fails, restart Windows."
    return $false
}

function New-DesktopShortcut {
    if (-not (Test-Path $StartBat)) {
        Write-Host "Could not find START_HERE.bat at $StartBat" -ForegroundColor Red
        return
    }

    $desktop = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = Join-Path $desktop "MovieWatchlistTracker.lnk"

    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $StartBat
    $shortcut.WorkingDirectory = $ProjectRoot
    $shortcut.Description = "Start MovieWatchlistTracker setup and local server"
    $shortcut.Save()

    Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green
}

function Test-PortAvailable {
    param([int]$Port)

    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Parse($AppHost), $Port)
    try {
        $listener.Start()
        return $true
    }
    catch {
        return $false
    }
    finally {
        $listener.Stop()
    }
}

function Get-AvailableAppUrl {
    foreach ($port in $PreferredPort..$FallbackPortLimit) {
        if (Test-PortAvailable $port) {
            return "http://$AppHost`:$port"
        }
    }

    return $null
}

function Show-ManualCommands {
    Write-Heading "Manual Commands"
    Write-Host "Run these from the project folder:"
    Write-Host "cd `"$ProjectRoot`""
    Write-Host "dotnet clean .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet restore .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet restore .\tests\MovieWatchlistTracker.Tests\MovieWatchlistTracker.Tests.csproj"
    Write-Host "dotnet build .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj"
    Write-Host "dotnet run --project .\src\MovieWatchlistTracker.Web\MovieWatchlistTracker.Web.csproj --urls http://$AppHost`:$PreferredPort"
    Write-Host ""
    Write-Host "Then open http://$AppHost`:$PreferredPort in your browser."
    Write-Host "If that port is busy, change 5093 to another free port, such as 5094 through $FallbackPortLimit."
}

function Start-WebApp {
    $dotnet = Get-DotnetCommand
    if (-not $dotnet) {
        Write-Host "dotnet was not found, so the server cannot be started." -ForegroundColor Red
        Show-ManualCommands
        return
    }

    Write-Heading "Preparing Project"
    Push-Location $ProjectRoot
    try {
        Write-Host "Cleaning stale web build outputs..."
        & $dotnet clean $ProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet clean failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Restoring web project dependencies..."
        & $dotnet restore $ProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore for the web project failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Restoring test project dependencies..."
        & $dotnet restore $TestProjectFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore for the test project failed with exit code $LASTEXITCODE."
        }

        Write-Host ""
        Write-Host "Building web project..."
        & $dotnet build $ProjectFile --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }

        $appUrl = Get-AvailableAppUrl
        if (-not $appUrl) {
            Write-Host ""
            Write-Host "Could not find a free port from $PreferredPort to $FallbackPortLimit." -ForegroundColor Red
            Write-Host "Close other running copies of the app, then run START_HERE.bat again."
            Show-ManualCommands
            return
        }

        Write-Host ""
        if ($appUrl -ne "http://$AppHost`:$PreferredPort") {
            Write-Host "Port $PreferredPort is busy, so the launcher will use $appUrl instead." -ForegroundColor Yellow
        }

        Write-Host "Starting MovieWatchlistTracker at $appUrl"
        Write-Host "Leave this window open while using the app. Press Ctrl+C here to stop the server."
        Write-Host ""

        $env:ASPNETCORE_ENVIRONMENT = "Development"
        & $dotnet run --no-build --project $ProjectFile --urls $appUrl
    }
    finally {
        Pop-Location
    }
}

Write-Heading "MovieWatchlistTracker Setup"
Write-Host "This script can create a desktop shortcut, check for the .NET 10 SDK, and start the local web server."

if (Read-YesNo "Create desktop shortcut?") {
    New-DesktopShortcut
}
else {
    Write-Host "Skipping desktop shortcut."
}

Write-Heading ".NET SDK Check"
$hasDotnet10 = Test-DotnetSdk10

if ($hasDotnet10) {
    Write-Host ".NET 10 SDK found." -ForegroundColor Green
}
else {
    Write-Host ".NET 10 SDK was not found." -ForegroundColor Yellow
    if (Read-YesNo "Install .NET 10 SDK now with winget?") {
        $hasDotnet10 = Install-DotnetSdk10
    }
}

Write-Heading "Server"
if (Read-YesNo "Start server now?") {
    if (-not $hasDotnet10) {
        Write-Host "The .NET 10 SDK is still missing, so starting the server will probably fail." -ForegroundColor Yellow

        if (Read-YesNo "Install .NET 10 SDK now with winget?") {
            $hasDotnet10 = Install-DotnetSdk10
        }
    }

    if ($hasDotnet10) {
        Start-WebApp
    }
    else {
        Write-Host "Server was not started because the .NET 10 SDK is not available." -ForegroundColor Red
        Show-ManualCommands
    }
}
else {
    Show-ManualCommands
}
