# Build script for MedievalCityBuilder
# Runs dotnet build

Write-Host "Building MedievalCityBuilder..." -ForegroundColor Cyan

$projectPath = "MedievalCityBuilder.csproj"
$exitCode = 0

try {
    $result = dotnet build $projectPath --configuration Debug
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "Build succeeded!" -ForegroundColor Green
    } else {
        Write-Host "Build failed with exit code $exitCode" -ForegroundColor Red
    }
} catch {
    Write-Host "Error running dotnet build: $_" -ForegroundColor Red
    $exitCode = 1
}

exit $exitCode