<#
.SYNOPSIS
    Removes commented code from C# files in the project.
.DESCRIPTION
    This script removes single-line comments (//) and multi-line comments (/* */)
    from C# files. It preserves XML documentation comments (///) and does NOT
    touch comments inside strings (like URLs with http://).
.PARAMETER Path
    The root path to search for C# files. Defaults to current directory.
.PARAMETER Preview
    If specified, shows what would be changed without modifying files.
.PARAMETER IncludeXmlDocs
    If specified, also removes XML documentation comments (///).
.EXAMPLE
    .\Remove-CommentedCode.ps1 -Preview
    .\Remove-CommentedCode.ps1 -Path "src/Services"
#>

param(
    [string]$Path = ".",
    [switch]$Preview,
    [switch]$IncludeXmlDocs
)

$ErrorActionPreference = "Stop"
$script:RemoveXmlDocs = $IncludeXmlDocs

# Colors for output
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warn { param($msg) Write-Host $msg -ForegroundColor Yellow }

function Remove-Comments {
    param([string]$Content)
    
    $result = New-Object System.Text.StringBuilder
    $i = 0
    $len = $Content.Length
    $inString = $false
    $inVerbatimString = $false
    $inRawString = $false
    $stringChar = ''
    $removedComment = $false
    
    while ($i -lt $len) {
        $char = $Content[$i]
        $nextChar = if ($i + 1 -lt $len) { $Content[$i + 1] } else { $null }
        $prevChar = if ($i -gt 0) { $Content[$i - 1] } else { $null }
        
        # Handle string literals - don't remove // inside strings
        if (-not $inString -and -not $inVerbatimString -and -not $inRawString) {
            # Check for verbatim string @"
            if ($char -eq '@' -and $nextChar -eq '"') {
                $inVerbatimString = $true
                [void]$result.Append($char)
                $i++
                [void]$result.Append($Content[$i])
                $i++
                continue
            }
            # Check for raw string """
            if ($char -eq '"' -and $nextChar -eq '"' -and ($i + 2 -lt $len) -and $Content[$i + 2] -eq '"') {
                $inRawString = $true
                [void]$result.Append($Content[$i])
                [void]$result.Append($Content[$i + 1])
                [void]$result.Append($Content[$i + 2])
                $i += 3
                continue
            }
            # Check for regular string
            if ($char -eq '"' -and $prevChar -ne '\') {
                $inString = $true
                $stringChar = '"'
                [void]$result.Append($char)
                $i++
                continue
            }
            # Check for char literal
            if ($char -eq "'" -and $prevChar -ne '\') {
                $inString = $true
                $stringChar = "'"
                [void]$result.Append($char)
                $i++
                continue
            }
            
            # Multi-line comment /* */
            if ($char -eq '/' -and $nextChar -eq '*') {
                $removedComment = $true
                # Skip until */
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
            
            # XML doc comment /// - remove if IncludeXmlDocs is set
            if ($char -eq '/' -and $nextChar -eq '/' -and ($i + 2 -lt $len) -and $Content[$i + 2] -eq '/') {
                if ($script:RemoveXmlDocs) {
                    $removedComment = $true
                    # Skip until end of line
                    while ($i -lt $len -and $Content[$i] -ne "`n") {
                        $i++
                    }
                    continue
                } else {
                    # Keep XML docs - copy until end of line
                    while ($i -lt $len -and $Content[$i] -ne "`n") {
                        [void]$result.Append($Content[$i])
                        $i++
                    }
                    continue
                }
            }
            
            # Single-line comment //
            if ($char -eq '/' -and $nextChar -eq '/') {
                $removedComment = $true
                # Skip until end of line
                while ($i -lt $len -and $Content[$i] -ne "`n") {
                    $i++
                }
                continue
            }
        }
        else {
            # Inside a string
            if ($inVerbatimString) {
                # Verbatim string ends with " (but "" is escaped quote)
                if ($char -eq '"') {
                    if ($nextChar -eq '"') {
                        # Escaped quote in verbatim string
                        [void]$result.Append($char)
                        [void]$result.Append($nextChar)
                        $i += 2
                        continue
                    } else {
                        $inVerbatimString = $false
                    }
                }
            }
            elseif ($inRawString) {
                # Raw string ends with """
                if ($char -eq '"' -and $nextChar -eq '"' -and ($i + 2 -lt $len) -and $Content[$i + 2] -eq '"') {
                    [void]$result.Append($Content[$i])
                    [void]$result.Append($Content[$i + 1])
                    [void]$result.Append($Content[$i + 2])
                    $i += 3
                    $inRawString = $false
                    continue
                }
            }
            elseif ($inString) {
                # Regular string ends with unescaped quote
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

# Get all C# files excluding bin/obj folders
$csFiles = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | 
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

Write-Info "Found $($csFiles.Count) C# files to scan..."

$totalFilesModified = 0
$totalCommentsRemoved = 0

foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    
    # Remove comments using the string-aware parser
    $result = Remove-Comments -Content $content
    
    # Only process if we actually removed a comment
    if (-not $result.RemovedComment) {
        continue
    }
    
    $content = $result.Content
    
    # Clean up lines that are now empty (were comment-only lines)
    $lines = $content -split "`n"
    $cleanedLines = @()
    $previousWasEmpty = $false
    
    foreach ($line in $lines) {
        $trimmedLine = $line.TrimEnd()
        $isEmptyOrWhitespace = [string]::IsNullOrWhiteSpace($trimmedLine)
        
        # Skip consecutive empty lines (keep max 1)
        if ($isEmptyOrWhitespace -and $previousWasEmpty) {
            continue
        }
        
        $cleanedLines += $trimmedLine
        $previousWasEmpty = $isEmptyOrWhitespace
    }
    
    $content = $cleanedLines -join "`n"
    
    # Count how many lines were different
    $originalLines = ($originalContent -split "`n").Count
    $newLines = ($content -split "`n").Count
    $linesChanged = $originalLines - $newLines
    
    $totalFilesModified++
    $totalCommentsRemoved += [Math]::Max(1, $linesChanged)

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
