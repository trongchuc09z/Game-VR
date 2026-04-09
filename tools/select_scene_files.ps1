$ErrorActionPreference = "Stop"

function Get-PendingPaths {
    $set = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($cmd in @("git diff --name-only --cached -z", "git diff --name-only -z", "git ls-files --others --exclude-standard -z")) {
        $raw = Invoke-Expression $cmd
        if ($raw) {
            foreach ($p in ($raw -split "`0")) {
                if ($p) { [void]$set.Add($p) }
            }
        }
    }
    return $set
}

function Get-Guids([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return @() }
    $content = Get-Content -LiteralPath $path -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { return @() }
    $m = [regex]::Matches($content, "guid:\s*([0-9a-fA-F]{32})")
    $set = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($x in $m) { [void]$set.Add($x.Groups[1].Value.ToLowerInvariant()) }
    return @($set)
}

$pending = Get-PendingPaths
$sceneFiles = @("Assets/Scenes/Man1_Tutorial.unity", "Assets/Scenes/Man4_BongRo.unity")
foreach ($sf in $sceneFiles) {
    if (-not (Test-Path -LiteralPath $sf)) { throw "Missing scene $sf" }
}

$guidToAsset = @{}
foreach ($p in $pending) {
    if ($p.EndsWith(".meta", [System.StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $p -PathType Leaf)) {
        $line = Select-String -LiteralPath $p -Pattern "^guid:\s*([0-9a-fA-F]{32})" -AllMatches -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($line -and $line.Matches.Count -gt 0) {
            $g = $line.Matches[0].Groups[1].Value.ToLowerInvariant()
            $guidToAsset[$g] = $p.Substring(0, $p.Length - 5)
        }
    }
}

$textExt = @(
    ".unity", ".prefab", ".mat", ".asset", ".controller", ".anim", ".overrideController", ".playable",
    ".lighting", ".physicMaterial", ".shader", ".shadergraph", ".vfx", ".asmdef", ".asmref", ".inputactions",
    ".renderTexture", ".terrainlayer", ".spriteatlas", ".compute", ".cginc", ".hlsl", ".uss", ".uxml"
)
$textSet = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
$textExt | ForEach-Object { [void]$textSet.Add($_) }

$selected = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
$visited = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
$q = New-Object System.Collections.Queue

foreach ($sf in $sceneFiles) {
    foreach ($p in @($sf, "$sf.meta")) {
        if ($pending.Contains($p)) { [void]$selected.Add($p) }
    }
    [void]$visited.Add($sf)
    $q.Enqueue($sf)
}

while ($q.Count -gt 0) {
    $cur = [string]$q.Dequeue()
    $ext = [System.IO.Path]::GetExtension($cur)
    if (-not $textSet.Contains($ext)) { continue }

    foreach ($g in (Get-Guids $cur)) {
        if ($guidToAsset.ContainsKey($g)) {
            $asset = $guidToAsset[$g]
            $meta = "$asset.meta"

            if ($pending.Contains($asset)) { [void]$selected.Add($asset) }
            if ($pending.Contains($meta)) { [void]$selected.Add($meta) }

            if ($visited.Add($asset) -and (Test-Path -LiteralPath $asset -PathType Leaf)) {
                $q.Enqueue($asset)
            }
        }
    }
}

if ($selected.Count -eq 0) { throw "No related files found." }

$pathspec = ".git/selected_scene_paths.txt"
$fs = [System.IO.File]::Open($pathspec, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
try {
    foreach ($p in $selected) {
        $b = [System.Text.Encoding]::UTF8.GetBytes($p)
        $fs.Write($b, 0, $b.Length)
        $fs.WriteByte(0)
    }
} finally { $fs.Dispose() }

"Selected files count: $($selected.Count)"
$selected | Sort-Object | Set-Content -LiteralPath ".git/selected_scene_paths_preview.txt" -Encoding UTF8
"Preview saved: .git/selected_scene_paths_preview.txt"
