$apikey = $env:nupkg_api_key

# Paths
$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# List of projects
$projects = (
    "Ddon.Common"
    #"Ddon.DependencyInjection",
    #"Ddon.Pipeline"
)

[xml]$commonPropsXml = Get-Content(Join-Path $rootFolder "version.props")
$version = $commonPropsXml.Project.PropertyGroup.Version

foreach($project in $projects) {
    $releasePath = Join-Path $rootFolder ("src" + "/" + $project + "/bin/Release/")
    $projectFolder = Join-Path $rootFolder ("src" + "/" + $project + "/" + $project + ".csproj")

    dotnet build $projectFolder -c Release 
    #dotnet publish $projectFolder -p:Configuration=Release -p:SourceLinkCreate=true -t:pack -f netstandard2.0
    #dotnet publish $projectFolder -p:Configuration=Release -p:SourceLinkCreate=true -t:pack
    dotnet nuget push ($releasePath + $project + "." + $version + ".nupkg") --api-key $apikey --source https://api.nuget.org/v3/index.json --skip-duplicate
}

#foreach($project in $projects) {
#    $projectFolder = Join-Path $rootFolder ("src/$project/$project.csproj")
#
#    $projXml = [xml](Get-Content $projectFolder)
#    $targetFrameworks = @()
#    if ($projXml.Project.PropertyGroup.TargetFramework) {
#        $targetFrameworks += $projXml.Project.PropertyGroup.TargetFramework
#    }
#    if ($projXml.Project.PropertyGroup.TargetFrameworks) {
#        $targetFrameworks += $projXml.Project.PropertyGroup.TargetFrameworks -split ";"
#    }
#
#    foreach($framework in $targetFrameworks) {
#        Write-Host "Building $project for framework $framework..."
#        dotnet build $projectFolder -c Release #-p:TargetFramework=$framework
#
#        Write-Host "Packing $project for framework $framework..."
#        $nupkgsFolder = [System.IO.Path]::Combine($rootFolder, "src", $project, "nupkgs")
#        #dotnet pack $projectFolder -c Release -p:TargetFramework=$framework -p:SourceLinkCreate=true -o $nupkgsFolder
#        dotnet pack $projectFolder -c Release -p:SourceLinkCreate=true -o nupkgs
#
#        # ¿ò¼Ü¶ÀÁ¢ÃüÃû£¬·ÀÖ¹¸²¸Ç
#        $nupkgPath = [System.IO.Path]::Combine($nupkgsFolder, "nupkgs/$project.$version.$framework.nupkg")
#        if (Test-Path $nupkgPath) {
#            #dotnet nuget push $nupkgPath --api-key $apikey --source https://api.nuget.org/v3/index.json --skip-duplicate
#        } else {
#            Write-Host "Warning: nupkg not found for $framework at $nupkgPath"
#        }
#    }
#}