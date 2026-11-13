# run-prompts.ps1 - Semi-automated prompt library driver
#
# Executes prompts/*.md in numeric order through OpenCode, runs the test suite
# after each task, allows a bounded number of fix attempts, and records progress.
#
# Usage:
#   .\tools\run-prompts.ps1                 # run all pending prompts
#   .\tools\run-prompts.ps1 -MaxTasks 1     # run only the next prompt (recommended first try)
#   .\tools\run-prompts.ps1 -FixAttempts 0  # never retry failures, just stop
#
# IMPORTANT:
# - Do an attended run (-MaxTasks 1) before leaving this unattended.
# - Each prompt is expected to commit its own work (the prompt says so).
# - Stop conditions in a prompt mean the agent stops and reports; this script
#   will treat an unverified task as a failure and retry only up to FixAttempts.

param(
    [int]$MaxTasks = 100,
    [int]$FixAttempts = 3,
    [string]$OpenCodeCmd = "opencode"   # change to "opencode2" if that is your binary name
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$promptsDir = Join-Path $repoRoot "prompts"
$doneDir = Join-Path $promptsDir ".done"
$logDir = Join-Path $doneDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Write-Log([string]$msg) {
    $stamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$stamp] $msg"
}

# Pending prompts: numeric order, not yet marked done
$pending = Get-ChildItem -Path $promptsDir -Filter "[0-9][0-9][0-9]-*.md" |
    Sort-Object Name |
    Where-Object { -not (Test-Path (Join-Path $doneDir ($_.BaseName + ".done"))) }

if (-not $pending) {
    Write-Log "No pending prompts. All done."
    exit 0
}

$runCount = [Math]::Min($MaxTasks, $pending.Count)
Write-Log "$($pending.Count) pending prompt(s); running up to $runCount."

for ($i = 0; $i -lt $runCount; $i++) {
    $prompt = $pending[$i]
    $logFile = Join-Path $logDir ($prompt.BaseName + ".log")

    # Clean working tree required before each task
    $dirty = git status --porcelain
    if ($dirty) {
        Write-Log "ABORT: working tree is dirty. Commit or stash first."
        Write-Log $dirty
        exit 1
    }

    Write-Log "=== Task $($prompt.Name) ==="

    $taskPrompt = "Read prompts/$($prompt.Name) and execute the task exactly. Follow AGENTS.md. Commit your work as the prompt instructs. Do not expand scope."
    & $OpenCodeCmd run --agent build $taskPrompt 2>&1 | Tee-Object -FilePath $logFile

    # Verify: tests must pass
    $attempt = 0
    $passed = $false
    while ($attempt -le $FixAttempts) {
        dotnet test (Join-Path $repoRoot "tests/MedievalCityBuilder.Tests") --nologo -v q 2>&1 |
            Tee-Object -FilePath $logFile -Append
        if ($LASTEXITCODE -eq 0) { $passed = $true; break }
        $attempt++
        if ($attempt -le $FixAttempts) {
            Write-Log "Tests failed (attempt $attempt/$FixAttempts). Asking agent to fix."
            & $OpenCodeCmd run --agent build "The test suite is failing after the previous task. Read the failures, fix them with the smallest possible change, re-run dotnet test until green, and commit the fix. Do not expand scope." 2>&1 |
                Tee-Object -FilePath $logFile -Append
        }
    }

    if (-not $passed) {
        Write-Log "FAILED: $($prompt.Name) did not pass tests after $FixAttempts fix attempt(s). Stopping."
        Write-Log "Inspect: $logFile and 'git status' / 'git log'."
        exit 1
    }

    # Also require a clean tree = the agent committed its work
    $dirty = git status --porcelain
    if ($dirty) {
        Write-Log "FAILED: tests pass but the agent left uncommitted changes. Stopping."
        Write-Log $dirty
        exit 1
    }

    New-Item -ItemType File -Path (Join-Path $doneDir ($prompt.BaseName + ".done")) | Out-Null
    Write-Log "DONE: $($prompt.Name)"
}

Write-Log "Run complete."
