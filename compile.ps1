# Use the folder where the script is run as the root
$root = Join-Path (Get-Location) "UNDF"

# Create output folder
$outFolder = Join-Path $root "out"
if (-not (Test-Path $outFolder)) {
    New-Item -ItemType Directory -Path $outFolder | Out-Null
}

# Get all first-level subfolders (projects)
$projects = Get-ChildItem -Path $root -Directory

foreach ($proj in $projects) {
    Write-Host "Processing $($proj.Name)..."

    # Get all .cs files in numeric order based on filename prefix
    $files = Get-ChildItem -Path $proj.FullName -Filter "*.cs" |
        Where-Object { $_.Name -match "^\d{2}-" } |
        Sort-Object { [int]($_.BaseName.Split('-')[0]) }

    if ($files.Count -eq 0) {
        Write-Host "No .cs files found in $($proj.Name), skipping."
        continue
    }

    # Output file path in the out folder
    $outFile = Join-Path $outFolder "$($proj.Name).cs"

    # Remove existing output file if it exists
    if (Test-Path $outFile) { Remove-Item $outFile }

    # Concatenate files into the output file
    foreach ($file in $files) {
        Write-Host "Adding $($file.Name)"
        Get-Content $file.FullName | Add-Content $outFile
        Add-Content $outFile "`r`n"  # extra newline between files
    }

    Write-Host "$($proj.Name).cs created successfully in $outFolder!"
}
