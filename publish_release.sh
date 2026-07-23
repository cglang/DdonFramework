#!/usr/bin/env bash

set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
VERSION_FILE="$ROOT_DIR/version.props"

cd "$ROOT_DIR"

# 检查工作区是否干净
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "❌ Git 工作区存在未提交的修改，请先提交或暂存。"
    git status --short
    exit 1
fi

# 获取当前版本
CURRENT_VERSION=$(grep -oP '(?<=<Version>).*?(?=</Version>)' "$VERSION_FILE")

if [ -z "$CURRENT_VERSION" ]; then
    echo "❌ 无法读取 version.props 中的版本号。"
    exit 1
fi

echo
echo "========================================="
echo "Current Version : $CURRENT_VERSION"
echo "========================================="
echo

read -rp "New Version: " NEW_VERSION

if [ -z "$NEW_VERSION" ]; then
    echo "❌ Version cannot be empty."
    exit 1
fi

if [ "$NEW_VERSION" = "$CURRENT_VERSION" ]; then
    echo "❌ New version must be different."
    exit 1
fi

# 简单校验版本号
if ! [[ "$NEW_VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9.-]+)?$ ]]; then
    echo "❌ Invalid version."
    echo "Example:"
    echo "  1.2.3"
    echo "  1.2.3-preview.1"
    exit 1
fi

TAG="v$NEW_VERSION"

# 检查本地 Tag
if git rev-parse "$TAG" >/dev/null 2>&1; then
    echo "❌ Local tag $TAG already exists."
    exit 1
fi

# 检查远程 Tag
if git ls-remote --tags origin | grep -q "refs/tags/$TAG$"; then
    echo "❌ Remote tag $TAG already exists."
    exit 1
fi

# 更新 version.props
sed -i.bak "s#<Version>$CURRENT_VERSION</Version>#<Version>$NEW_VERSION</Version>#g" "$VERSION_FILE"
rm -f "$VERSION_FILE.bak"

echo
echo "Version:"
echo "  $CURRENT_VERSION -> $NEW_VERSION"
echo

git diff -- "$VERSION_FILE"

echo
read -rp "Release v$NEW_VERSION ? (y/N): " CONFIRM

if [[ ! "$CONFIRM" =~ ^[Yy]$ ]]; then
    git checkout -- "$VERSION_FILE"
    echo "Cancelled."
    exit 0
fi

git add "$VERSION_FILE"

git commit -m "chore(release): v$NEW_VERSION"

git tag -a "$TAG" -m "Release $TAG"

echo
echo "Pushing..."

git push origin HEAD
git push origin "$TAG"

echo
echo "========================================="
echo "Release completed."
echo
echo "Version : $NEW_VERSION"
echo "Tag     : $TAG"
echo
echo "GitHub Actions will publish NuGet packages."
echo "========================================="