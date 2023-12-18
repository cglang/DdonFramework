$apikey = $env:nupkg_api_key

# Paths
$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# List of projects
$projects = (
"Ddon.Application",
"Ddon.AspNetCore",
"Ddon.Cache",
"Ddon.Cache.Memory",
"Ddon.Core",
"Ddon.Domain",
"Ddon.EventBus",
"Ddon.EventBus.Memory",
"Ddon.FileStorage",
"Ddon.Identity",
"Ddon.Jwt",
"Ddon.KeyValueStorage",
"Ddon.Localizer",
"Ddon.Mail",
"Ddon.Repository",
"Ddon.Schedule",
"Ddon.Socket"
)

[xml]$commonPropsXml = Get-Content(Join-Path $rootFolder "common.props")
$version = $commonPropsXml.Project.PropertyGroup.Version

foreach($project in $projects) {
    $releasePath = Join-Path $rootFolder ("src" + "/" + $project + "/bin/Release/")
    $projectFolder = Join-Path $rootFolder ("src" + "/" + $project + "/" + $project + ".csproj")

    dotnet publish $projectFolder -p:Configuration=Release -p:SourceLinkCreate=true -t:pack
    dotnet nuget push ($releasePath + $project + "." + $version + ".nupkg") --api-key $apikey --source https://api.nuget.org/v3/index.json --skip-duplicate
}