#!/usr/bin/env bash

set -e

apikey="${nupkg_api_key}"
rootFolder="$(pwd)"

mkdir -p artifacts

while read -r project
do
    [ -z "$project" ] && continue

    projectFile="$rootFolder/src/$project/$project.csproj"

    echo "Packing $project..."

    dotnet pack "$projectFile" \
        -c Release \
        --no-restore \
        -o "$rootFolder/artifacts"

done < publish-projects.txt

for package in "$rootFolder"/artifacts/*.nupkg
do
    echo "Publishing $(basename "$package")"

    dotnet nuget push "$package" \
        --api-key "$apikey" \
        --source https://api.nuget.org/v3/index.json \
        --skip-duplicate
done