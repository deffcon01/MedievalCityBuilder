# run-prompts.ps1 - Semi-automated prompt library driver
#
# Executes ONE pending prompt from prompts/ through OpenCode on a fresh branch
# off main, runs the test suite, and pushes the branch for a PR.
#
# Usage:
#   .\tools\run-prompts.ps1                 # run the next pending prompt
#   .\tools\run-prompts.ps1 -SkipPush       # run but don't push (local only)
#
# Workflow per task (one prompt = one branch = one PR):
#   1. Verify clean tree, sync main from origin
#   2. Create branch task/<prompt-name> off main
#   3. Tell OpenCode to execute the prompt on that branch
#   4. Run dotnet test; on failure, bounded fix attempts
#   5. Require a clean tree (the agent committed its work)
#   6. Push the branch, print the PR link
#   7. STOP. Merge the PR, then run this script again for the next task.
#
# IMPORTANT: Do an attended run before leaving this unattended.

param(
    [switch]$SkipPush,
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

# --- Pre-flight ---

$dirty = git status --porcelain
if ($dirty) {
    Write-Log "ABORT: working tree is dirty. Commit or stash first."
    Write-Log $dirty
    exit 1
}

git checkout main
if ($LASTEXITCODE -ne 0) { Write-Log "ABORT: cannot check out main."; exit 1 }

git pull origin main
if ($LASTEXITCODE -ne 0) { Write-Log "ABORT: cannot pull main. Fix your remote/local state first."; exit 1 }

# --- Find the next pending prompt ---

$pending = Get-ChildItem -Path $promptsDir -Filter "[0-9][0-9][0-9]-*.md" |
    Sort-Object Name |
    Where-Object { -not (Test-Path (Join-Path $doneDir ($_.BaseName + ".done"))) }

if (-not $pending) {
    Write-Log "No pending prompts. All done."
    exit 0
}

$prompt = $pending[0]
$logFile = Join-Path $logDir ($prompt.BaseName + ".log")
$branchName = "task/" + $prompt.BaseName

# Refuse to reuse an existing task branch
$branchExists = git branch --list $branchName
if ($branchExists) {
    Write-Log "ABORT: branch $branchName already exists locally."
    Write-Log "Merge or delete it first: git branch -d $branchName (only after its PR is merged)."
    exit 1
}

git checkout -b $branchName
if ($LASTEXITCODE -ne 0) { Write-Log "ABORT: cannot create branch $branchName."; exit 1 }

Write-Log "=== Task $($prompt.Name) on branch $branchName ==="

# --- Run the task ---

$taskPrompt = "Read prompts/$($prompt.Name) and execute the task exactly. Follow AGENTS.md. You are on branch $branchName - commit all your work to this branch as the prompt instructs. Do not expand scope."
& $OpenCodeCmd run --agent build $taskPrompt 2>&1 | Tee-Object -FilePath $logFile

# --- Verify ---

$fixAttempts = 3
$attempt = 0
$passed = $false
while ($attempt -le $fixAttempts) {
    dotnet test (Join-Path $repoRoot "tests/MedievalCityBuilder.Tests") --nologo -v q 2>&1 |
        Tee-Object -FilePath $logFile -Append
    if ($LASTEXITCODE -eq 0) { $passed = $true; break }
    $attempt++
    if ($attempt -le $fixAttempts) {
        Write-Log "Tests failed (attempt $attempt/$fixAttempts). Asking agent to fix."
        & $OpenCodeCmd run --agent build "The test suite is failing after the previous task. Read the failures, fix them with the smallest possible change, re-run dotnet test until green, and commit the fix to the current branch. Do not expand scope." 2>&1 |
            Tee-Object -FilePath $logFile -Append
    }
}

if (-not $passed) {
    Write-Log "FAILED: $($prompt.Name) did not pass tests after $fixAttempts fix attempt(s). Stopping."
    Write-Log "Inspect: $logFile, 'git status', 'git log'. Branch $branchName left in place for review."
    exit 1
}

$dirty = git status --porcelain
if ($dirty) {
    Write-Log "FAILED: tests pass but the agent left uncommitted changes. Stopping."
    Write-Log $dirty
    Write-Log "Inspect and commit manually, or discard with git checkout -- ."
    exit 1
}

# --- Record and push ---

New-Item -ItemType File -Path (Join-Path $doneDir ($prompt.BaseName + ".done")) | Out-Null

if (-not $SkipPush) {
    git push -u origin $branchName
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Push failed. Branch $branchName is complete locally; push manually."
        exit 1
    }
    Write-Log "DONE: $($prompt.Name). Branch pushed: $branchName"
    Write-Log "NEXT: merge the PR, then run this script again for the next prompt."
} else {
    Write-Log "DONE (local only): $($prompt.Name) on branch $branchName"
}
