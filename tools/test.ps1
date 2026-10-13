# Test script for MedievalCityBuilder
# Runs dotnet test on the test project and exits with its exit code

Write-Host "Running tests for MedievalCityBuilder..." -ForegroundColor Cyan

$testProjectPath = "tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj"
$exitCode = 0

try {
    $result = dotnet test $testProjectPath --configuration Debug --no-build
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "Tests failed with exit code $exitCode" -ForegroundColor Red
    }
} catch {
    Write-Host "Error running dotnet test: $_" -ForegroundColor Red
    $exitCode = 1
}

exit $exitCode