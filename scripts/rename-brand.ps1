$ErrorActionPreference = "Stop"
$root = "c:\Users\Alex\Documents\GitHub\evrotek-server"

function Rename-IfExists([string]$from, [string]$toName) {
    if (Test-Path $from) {
        $dest = Join-Path (Split-Path $from -Parent) $toName
        if ((Test-Path $dest) -and ($from -ne $dest)) {
            Remove-Item -Recurse -Force $dest -ErrorAction SilentlyContinue
        }
        if ($from -ne $dest) {
            Rename-Item -Path $from -NewName $toName
            Write-Host "Renamed: $from -> $toName"
        }
    }
}

Rename-IfExists "$root\Evrotek.ApplicationDataAccess" "JetFlight.ApplicationDataAccess"
Rename-IfExists "$root\Evrotek.IntegrationDataAccess" "JetFlight.IntegrationDataAccess"
Rename-IfExists "$root\Evrotek.Service" "JetFlight.Service"
Rename-IfExists "$root\Evrotek.Shared" "JetFlight.Shared"

$projRenames = @(
    @("$root\JetFlight.ApplicationDataAccess\Evrotek.ApplicationDataAccess.csproj", "JetFlight.ApplicationDataAccess.csproj"),
    @("$root\JetFlight.IntegrationDataAccess\Evrotek.IntegrationDataAccess.csproj", "JetFlight.IntegrationDataAccess.csproj"),
    @("$root\JetFlight.Service\Evrotek.Service.csproj", "JetFlight.Service.csproj"),
    @("$root\JetFlight.Shared\Evrotek.Shared.csproj", "JetFlight.Shared.csproj"),
    @("$root\src\Evrotek.WebApi.csproj", "JetFlight.WebApi.csproj"),
    @("$root\tests\Evrotek.WebApiTests.csproj", "JetFlight.WebApiTests.csproj")
)
foreach ($pair in $projRenames) {
    if (Test-Path $pair[0]) { Rename-Item $pair[0] $pair[1] }
}

Rename-IfExists "$root\WebApi.sln" "JetFlight.sln"

$avatarDir = "$root\src\Avatars"
if (Test-Path $avatarDir) {
    Get-ChildItem $avatarDir -File | Where-Object { $_.Name -like '*Arsen*' } | ForEach-Object {
        Rename-Item $_.FullName ($_.Name.Replace('Arsen', 'BirdJet'))
    }
    Get-ChildItem $avatarDir -File | Where-Object { $_.Name -like '*Kvartal*' } | ForEach-Object {
        Rename-Item $_.FullName ($_.Name.Replace('Kvartal', 'CatJet'))
    }
}

$extensions = @('.cs', '.csproj', '.sln', '.json', '.yml', '.yaml', '.sh', '.md', '.txt')
Get-ChildItem $root -Recurse -File | Where-Object {
    $path = $_.FullName
    ($extensions -contains $_.Extension -or $_.Name -in @('.gitignore', '.dockerignore', 'Dockerfile', 'docker-compose.yml')) -and
    $path -notmatch '\\(\.git|bin|obj|\.vs|benchmark)\\'
} | ForEach-Object {
    try {
        $content = [System.IO.File]::ReadAllText($_.FullName)
        $original = $content
        $content = $content.Replace('Evrotek', 'JetFlight')
        $content = $content.Replace('Arsen', 'BirdJet')
        $content = $content.Replace('Kvartal', 'CatJet')
        $content = $content.Replace('arsen', 'birdjet')
        $content = $content.Replace('kvartal', 'catjet')
        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($_.FullName, $content)
        }
    } catch {
        Write-Warning "Skip $($_.FullName): $_"
    }
}

Write-Host "Brand rename complete."
