#!/usr/bin/env bash

set -e

apikey="${nupkg_api_key}"

# Paths
rootFolder="$(pwd)"

# List of projects
projects=(
    "Ddon.Common"
    "Ddon.DependencyInjection"
    "Ddon.Pipeline"
    "Ddon.Workflow"
    "Ddon.Cache"
    "Ddon.Cache.Redis"
    "Ddon.Cache.Memory"
    "Ddon.Serial"
    "Ddon.Socket"
    "Ddon.EventBus"
    "Ddon.EventBus.Memory"
    "Ddon.VitrinPLC"
    "Ddon.Hosting"
    "Ddon.Desktop.Core"
    "Ddon.Desktop.Avalonia"
)

# Read version from version.props
version=$(grep -oP '(?<=<Version>).*?(?=</Version>)' "$rootFolder/version.props")

if [ -z "$version" ]; then
    echo "version.props 文件中找不到版本：Version"
    exit 1
fi

for project in "${projects[@]}"; do

    releasePath="$rootFolder/src/$project/bin/Release"
    projectFile="$rootFolder/src/$project/$project.csproj"

    echo "构建 $project..."

    dotnet build "$projectFile" -c Release

    package="$releasePath/$project.$version.nupkg"

    if [ -f "$package" ]; then
         echo "上传 $package 中..."

         dotnet nuget push "$package" \
             --api-key "$apikey" \
             --source "https://api.nuget.org/v3/index.json" \
             --skip-duplicate
    else
        echo "警告: 找不到: $package"
    fi

done
