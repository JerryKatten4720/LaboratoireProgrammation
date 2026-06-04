$path = "C:\Users\antoi\Documents\GitHub\LaboratoireProgrammation\Project\ModernOverseerWars"
$rootNsRegex = "LaboratoireProgrammation\.Project\.ModernOverseerWars"
$rootNs = "LaboratoireProgrammation.Project.ModernOverseerWars"

function Replace-In-Files($pathGlob, $regexPattern, $replacement) {
    Get-ChildItem -Path $pathGlob -Recurse -File | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content -match $regexPattern) {
            $newContent = $content -replace $regexPattern, $replacement
            Set-Content -Path $_.FullName -Value $newContent -NoNewline
            Write-Host "Updated $($_.Name)"
        }
    }
}

# 1. Update Domain -> Models globally for usings and namespaces
Replace-In-Files "$path\*" "namespace ${rootNsRegex}\.Domain" "namespace ${rootNs}.Models"
Replace-In-Files "$path\*" "using ${rootNsRegex}\.Domain" "using ${rootNs}.Models"

# 2. Update Data -> Models.Data globally for usings and namespaces
Replace-In-Files "$path\*" "namespace ${rootNsRegex}\.Data" "namespace ${rootNs}.Models.Data"
Replace-In-Files "$path\*" "using ${rootNsRegex}\.Data" "using ${rootNs}.Models.Data"

# 3. Update Services -> Controllers globally for usings and namespaces
Replace-In-Files "$path\*" "namespace ${rootNsRegex}\.Services" "namespace ${rootNs}.Controllers"
Replace-In-Files "$path\*" "using ${rootNsRegex}\.Services" "using ${rootNs}.Controllers"

# 4. Update UI -> Views globally for usings and namespaces
Replace-In-Files "$path\*" "namespace ${rootNsRegex}\.UI" "namespace ${rootNs}.Views"
Replace-In-Files "$path\*" "using ${rootNsRegex}\.UI" "using ${rootNs}.Views"

# 5. Fix remaining classes that were in root but moved to Views
$viewsFolder = "$path\Views"
$rootViews = @("BuildRoomDialog", "ControlCard", "ControlCardFactory", "DeckPicker", "HexTileControl", "OverseerWarsWindow", "StartMenuWindow", "VaultCardHolder", "OutlinedTextBlock")

foreach ($v in $rootViews) {
    # .cs files (they might have file-scoped namespaces `namespace ...;` or block-scoped `namespace ... {`)
    Replace-In-Files "$viewsFolder\${v}.*" "namespace ${rootNsRegex}\s*;" "namespace ${rootNs}.Views;"
    Replace-In-Files "$viewsFolder\${v}.*" "namespace ${rootNsRegex}\s*\{" "namespace ${rootNs}.Views {"
    
    # xaml files
    Replace-In-Files "$viewsFolder\${v}.xaml" "x:Class=`"${rootNsRegex}\.${v}`"" "x:Class=`"${rootNs}.Views.${v}`""
    Replace-In-Files "$viewsFolder\${v}.xaml" "xmlns:local=`"clr-namespace:${rootNsRegex}`"" "xmlns:local=`"clr-namespace:${rootNs}.Views`""
}

# PlayerProfile was moved to Models
Replace-In-Files "$path\Models\PlayerProfile.cs" "namespace ${rootNsRegex}\s*;" "namespace ${rootNs}.Models;"
Replace-In-Files "$path\Models\PlayerProfile.cs" "namespace ${rootNsRegex}\s*\{" "namespace ${rootNs}.Models {"

Write-Host "Done replacements."
