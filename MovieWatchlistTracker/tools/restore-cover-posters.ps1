$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$BackupCoverFolder = Join-Path $ProjectRoot "movie cover posters"
$RuntimeCoverFolder = Join-Path $ProjectRoot "src\MovieWatchlistTracker.Web\wwwroot\images\covers"
$SeedDataFile = Join-Path $ProjectRoot "src\MovieWatchlistTracker.Web\Data\SeedData.cs"

$CoverManifest = @{
    "0b8ac93196b845f09166c2be2b846b09.jpg" = "Total Recall (2012) (1).jpg"
    "1182258a65dc4a559f7eb4f31bbd2700.jpg" = "Dragon Ball Super Broly (1).jpg"
    "18ac11b9355c4a1bb816f0d304e05c0b.jpg" = "Blade Runner 2049 (1).jpg"
    "3276b518eb064e3eae5df6f5d4e34106.jpg" = "Iron Man (1).jpg"
    "37040de7afdb4012bc9473084b6befe7.jpg" = "Baaghi (1).jpg"
    "4156b8d106074ee3ac4d03bc807dfb21.jpg" = "Spirited Away (1).jpg"
    "45315a1a82b24b9bb15c59c2df5188b6.jpg" = "Elysium (2).jpg"
    "45ed96d4972e41e29a5b1053d84eaea7.jpg" = "Alita Battle Angel (2).jpg"
    "52014a641df842e793c554b37836b055.jpg" = "Rush Hour (1).jpg"
    "69ccbccab5474be3a10c803d98964ec4.jpg" = "Humko Deewana Kar Gaye (1).jpg"
    "6a2a4dae0c8e4860803ab1563a76bd4c.jpg" = "Singh is Kinng (1).jpg"
    "715146a2f57a443b8313b213fb4e530b.jpg" = "Avengers Age of Ultron (1).jpg"
    "720444eadccf40869362c2d64b01ad85.jpg" = "Chhaava (1).jpg"
    "765d5f098a984328be0953bccb0b0405.jpg" = "Man of Steel (1).jpg"
    "89ac8976cba144158e7efb46cb499fff.jpg" = "WallE (1).jpg"
    "ac94c220515340a48d5f2a82586dd920.jpg" = "Alita Battle Angel (3).jpg"
    "b027724e5da548a09fb1b8ab2beabf5e.jpg" = "Elysium (1).jpg"
    "bf411dd0f8f345ed906b9480662c808d.jpg" = "Ghost in the Shell (1).jpg"
    "cdc43db099414b1eaa33b5e1a1aebc4d.jpg" = "Eternal Sunshine of the Spotless Mind (1).jpg"
    "d27bef653a7244f3a5835f9187c7e256.jpg" = "Wolf Children (1).jpg"
    "da22ad2c2bd54002a6dd1893fef96daf.jpg" = "Stree (1).jpg"
    "df4fef8e16c7473e967315e118e4b6bc.jpg" = "The Incredibles (1).jpg"
    "f31b369227a041609d62ad9043fd0678.jpg" = "Ip Man (1).jpg"
    "f62471bc23fc46d994d43ab42141a233.jpg" = "A Silent Voice (1).jpg"
}

function Restore-CoverPosters {
    if (-not (Test-Path $BackupCoverFolder)) {
        Write-Host "Cover backup folder was not found: $BackupCoverFolder" -ForegroundColor Yellow
        return
    }

    if (-not (Test-Path $RuntimeCoverFolder)) {
        New-Item -ItemType Directory -Path $RuntimeCoverFolder | Out-Null
    }

    $restoredCount = 0
    $missingBackupFiles = @()

    foreach ($targetFileName in $CoverManifest.Keys) {
        $sourcePath = Join-Path $BackupCoverFolder $CoverManifest[$targetFileName]
        $runtimePath = Join-Path $RuntimeCoverFolder $targetFileName

        if (-not (Test-Path $sourcePath)) {
            $missingBackupFiles += $CoverManifest[$targetFileName]
            continue
        }

        if (-not (Test-Path $runtimePath) -or ((Get-Item $runtimePath).Length -eq 0)) {
            Copy-Item -LiteralPath $sourcePath -Destination $runtimePath -Force
            $restoredCount++
        }
    }

    if ($restoredCount -gt 0) {
        Write-Host "Restored $restoredCount cover poster file(s)." -ForegroundColor Green
    }
    else {
        Write-Host "Cover poster files are already in place."
    }

    if ($missingBackupFiles.Count -gt 0) {
        Write-Host "Some backup poster files are missing:" -ForegroundColor Yellow
        $missingBackupFiles | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Yellow }
    }
}

function Test-SeedDataCoverReferences {
    if (-not (Test-Path $SeedDataFile)) {
        return
    }

    $seedData = Get-Content -Raw $SeedDataFile
    $matches = [regex]::Matches($seedData, '"/images/covers/([^"]+)"')
    $missing = @()

    foreach ($match in $matches) {
        $fileName = $match.Groups[1].Value
        $runtimePath = Join-Path $RuntimeCoverFolder $fileName

        if (-not (Test-Path $runtimePath)) {
            $missing += $fileName
        }
    }

    if ($missing.Count -gt 0) {
        Write-Host "Some seeded movies reference cover files that are still missing:" -ForegroundColor Yellow
        $missing | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Yellow }
    }
}

Restore-CoverPosters
Test-SeedDataCoverReferences
