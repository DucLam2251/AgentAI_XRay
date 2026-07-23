param(
    [string]$Device = "0",
    [ValidateSet("all", "fracture-classifier", "fracture-detector", "abnormality-classifier")]
    [string]$Task = "all",
    [int]$Epochs = 0,
    [int]$ImageSize = 0,
    [int]$BatchSize = 0
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$PythonExe = Join-Path $ProjectRoot ".venv\Scripts\python.exe"
$LogDirectory = Join-Path $ProjectRoot "runs\research"
$StateFile = Join-Path $LogDirectory "pipeline_status.json"
$LogFile = Join-Path $LogDirectory "pipeline.log"
New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$OutputEncoding = New-Object System.Text.UTF8Encoding($false)

function Write-PipelineState {
    param([string]$Status, [string]$Task, [string]$Message)
    @{
        status = $Status
        task = $Task
        message = $Message
        pid = $PID
        updated_at = (Get-Date).ToString("s")
        log_file = "runs/research/pipeline.log"
    } | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath $StateFile
}

$AllTasks = @(
    "fracture-classifier",
    "fracture-detector",
    "abnormality-classifier"
)
$Tasks = if ($Task -eq "all") { $AllTasks } else { @($Task) }

try {
    Set-Content -Encoding UTF8 -LiteralPath $LogFile -Value "Research pipeline started: $(Get-Date -Format s)"
    Write-PipelineState "running" "preflight" "Validating prepared datasets"
    & $PythonExe (Join-Path $ProjectRoot "prepare_research_data.py") --dataset all 2>&1 |
        Out-File -Encoding UTF8 -Append -LiteralPath $LogFile
    if ($LASTEXITCODE -ne 0) { throw "Dataset preparation failed with exit code $LASTEXITCODE" }

    foreach ($CurrentTask in $Tasks) {
        Write-PipelineState "running" $CurrentTask "Training with selected configuration"
        $TrainArguments = @(
            (Join-Path $ProjectRoot "train_research.py"),
            $CurrentTask,
            "--device", $Device
        )
        if ($Epochs -gt 0) { $TrainArguments += @("--epochs", $Epochs) }
        if ($ImageSize -gt 0) { $TrainArguments += @("--imgsz", $ImageSize) }
        if ($BatchSize -gt 0) { $TrainArguments += @("--batch", $BatchSize) }
        & $PythonExe $TrainArguments 2>&1 |
            Out-File -Encoding UTF8 -Append -LiteralPath $LogFile
        if ($LASTEXITCODE -ne 0) { throw "$CurrentTask failed with exit code $LASTEXITCODE" }
    }
    Write-PipelineState "completed" $Task "Requested research training completed"
}
catch {
    Write-PipelineState "failed" $Task $_.Exception.Message
    $_ | Out-String | Add-Content -Encoding UTF8 -LiteralPath $LogFile
    exit 1
}
