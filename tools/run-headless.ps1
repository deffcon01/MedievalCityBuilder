# Run headless script for MedievalCityBuilder
# Runs Godot in headless mode for 5 seconds then quits

$godotPath = "C:\Users\Admin\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe"
$projectPath = "."

Write-Host "Running MedievalCityBuilder in headless mode..." -ForegroundColor Cyan
Write-Host "Godot: $godotPath" -ForegroundColor Gray
Write-Host "Project: $projectPath" -ForegroundColor Gray

if (-not (Test-Path $godotPath)) {
    Write-Host "ERROR: Godot executable not found at: $godotPath" -ForegroundColor Red
    exit 1
}

$exitCode = 0

try {
    & $godotPath --headless --path $projectPath --quit-after 5
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host "Headless run completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Headless run exited with code $exitCode" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error running Godot: $_" -ForegroundColor Red
    $exitCode = 1
}

exit $exitCode