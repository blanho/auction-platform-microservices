<#
.SYNOPSIS
    Removes commented code from TypeScript/TSX files in the frontend.
.DESCRIPTION
    This script removes single-line comments (//) and multi-line comments (/* */)
    from TypeScript/TSX files. It preserves JSDoc comments (/**) and does NOT
    touch comments inside strings.
.PARAMETER Path
    The root path to search for TS/TSX files. Defaults to current directory.
.PARAMETER Preview
    If specified, shows what would be changed without modifying files.
.PARAMETER IncludeJSDoc
    If specified, also removes JSDoc documentation comments (/**).
.EXAMPLE
    .\Remove-FrontendComments.ps1 -Preview
    .\Remove-FrontendComments.ps1 -Path "src"
#>

param(
    [string]$Path = ".",
    [switch]$Preview,
    [switch]$IncludeJSDoc
)

$ErrorActionPreference = "Stop"
$script:RemoveJSDoc = $IncludeJSDoc

function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warn { param($msg) Write-Host $msg -ForegroundColor Yellow }

function Remove-TSComments {
    param([string]$Content)
    
    $result = New-Object System.Text.StringBuilder
    $i = 0
    $len = $Content.Length
    $inString = $false
    $inTemplateString = $false
    $stringChar = ''
    $removedComment = $false
    
    while ($i -lt $len) {
        $char = $Content[$i]
        $nextChar = if ($i + 1 -lt $len) { $Content[$i + 1] } else { $null }
        $prevChar = if ($i -gt 0) { $Content[$i - 1] } else { $null }
        $nextNextChar = if ($i + 2 -lt $len) { $Content[$i + 2] } else { $null }
        
        # Handle string literals
        if (-not $inString -and -not $inTemplateString) {
            # Template string
            if ($char -eq '`') {
                $inTemplateString = $true
                [void]$result.Append($char)
                $i++
                continue
            }
            # Regular string
            if (($char -eq '"' -or $char -eq "'") -and $prevChar -ne '\') {
                $inString = $true
                $stringChar = $char
                [void]$result.Append($char)
                $i++
                continue
            }
            
            # JSDoc comment /** */ - preserve unless IncludeJSDoc is set
            if ($char -eq '/' -and $nextChar -eq '*' -and $nextNextChar -eq '*') {
                if ($script:RemoveJSDoc) {
                    $removedComment = $true
                    $i += 2
                    while ($i -lt $len - 1) {
                        if ($Content[$i] -eq '*' -and $Content[$i + 1] -eq '/') {
                            $i += 2
                            break
                        }
                        $i++
                    }
                    continue
                } else {
                    # Keep JSDoc - copy until */
                    while ($i -lt $len - 1) {
                        [void]$result.Append($Content[$i])
                        if ($Content[$i] -eq '*' -and $Content[$i + 1] -eq '/') {
                            [void]$result.Append($Content[$i + 1])
                            $i += 2
                            break
                        }
                        $i++
                    }
                    continue
                }
            }
            
            # Multi-line comment /* */
            if ($char -eq '/' -and $nextChar -eq '*') {
                $removedComment = $true
                $i += 2
                while ($i -lt $len - 1) {
                    if ($Content[$i] -eq '*' -and $Content[$i + 1] -eq '/') {
                        $i += 2
                        break
                    }
                    $i++
                }
                continue
            }
            
            # Single-line comment //
            if ($char -eq '/' -and $nextChar -eq '/') {
                $removedComment = $true
                while ($i -lt $len -and $Content[$i] -ne "`n") {
                    $i++
                }
                continue
            }
        }
        else {
            # Inside a string
            if ($inTemplateString) {
                if ($char -eq '`' -and $prevChar -ne '\') {
                    $inTemplateString = $false
                }
            }
            elseif ($inString) {
                if ($char -eq $stringChar -and $prevChar -ne '\') {
                    $inString = $false
                }
            }
        }
        
        [void]$result.Append($char)
        $i++
    }
    
    return @{
        Content = $result.ToString()
        RemovedComment = $removedComment
    }
}

# Get all TS/TSX files excluding node_modules
$tsFiles = Get-ChildItem -Path $Path -Include "*.ts","*.tsx" -Recurse | 
    Where-Object { $_.FullName -notmatch '\\node_modules\\' }

Write-Info "Found $($tsFiles.Count) TypeScript/TSX files to scan..."

$totalFilesModified = 0

foreach ($file in $tsFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    if ([string]::IsNullOrEmpty($content)) {
        continue
    }
    
    $originalContent = $content
    
    # Remove comments
    $result = Remove-TSComments -Content $content
    
    if (-not $result.RemovedComment) {
        continue
    }
    
    $content = $result.Content
    
    # Clean up lines that are now empty
    $lines = $content -split "`n"
    $cleanedLines = @()
    $previousWasEmpty = $false
    
    foreach ($line in $lines) {
        $trimmedLine = $line.TrimEnd()
        $isEmptyOrWhitespace = [string]::IsNullOrWhiteSpace($trimmedLine)
        
        # Skip consecutive empty lines
        if ($isEmptyOrWhitespace -and $previousWasEmpty) {
            continue
        }
        
        $cleanedLines += $trimmedLine
        $previousWasEmpty = $isEmptyOrWhitespace
    }
    
    $content = $cleanedLines -join "`n"
    
    $totalFilesModified++
    $relativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart("\")
    
    if ($Preview) {
        Write-Warn "[PREVIEW] $relativePath"
    } else {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Success "Modified: $relativePath"
    }
}

Write-Host ""
if ($Preview) {
    Write-Info "=== PREVIEW COMPLETE ==="
    Write-Info "Would modify $totalFilesModified files"
    Write-Warn "Run without -Preview to apply changes"
} else {
    Write-Success "=== COMPLETE ==="
    Write-Success "Modified $totalFilesModified files"
}
