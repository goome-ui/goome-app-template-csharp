#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

stable='^[0-9]+\.[0-9]+\.[0-9]+$'
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

rewrite_pin() {
  local old=$1 new=$2
  git ls-files | grep -v '^\.github/' | xargs -d '\n' grep -lF -- "$old" | xargs -d '\n' sed -i "s/${old//./\\.}/$new/g"
}

latest_version() {
  curl -fsS https://api.nuget.org/v3-flatcontainer/goomesharp/index.json | python3 -c 'import json, sys; print("\n".join(json.load(sys.stdin)["versions"]))' | grep -E "$stable" | sort -V | tail -n1
}

current_version() {
  sed -n 's/.*Include="GoomeSharp" Version="\([^"]*\)".*/\1/p' MyGoomeApp.csproj | head -n1
}

pin_version() {
  rewrite_pin "$1" "$2"
}

smoke() {
  export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
  dotnet build -c Release -o "$work/out" -p:BaseIntermediateOutputPath="$work/obj/"
  mkdir "$work/shots"
  out=$(GOOME_BACKEND=test GOOME_TEST_SHOTS="$work/shots" GOOME_TEST_SCRIPT="screen 1280x720 1; 3 click 60,28; end 6" dotnet "$work/out/MyGoomeApp.dll")
  echo "$out"
  ls "$work/shots" | grep -q ppm
  if [ -d variants ]; then echo "$out" | grep -q "overlay mode"; fi
  for v in variants/*/; do
    [ -d "$v" ] || continue
    n=$(basename "$v")
    d="$work/v/$n"
    mkdir -p "$d" "$work/shots-$n"
    cp ./*.csproj "$d/"
    cp "$v/Program.cs" "$d/"
    dotnet build -c Release -o "$d/out" "$d"
    GOOME_BACKEND=test GOOME_TEST_SHOTS="$work/shots-$n" GOOME_TEST_SCRIPT="screen 1280x720 1; 2 hotkey alt+m; 3 click 100,140; end 6" dotnet "$d/out/MyGoomeApp.dll"
    ls "$work/shots-$n" | grep -q ppm
  done
}

is_upgrade() {
  [ "$1" != "$2" ] && [ "$(printf '%s\n%s\n' "$1" "$2" | sort -V | tail -n1)" = "$1" ]
}

push_branch() {
  if [ -n "${GOOME_DRY:-}" ]; then
    echo "dry run, not pushing $1"
  else
    git push -q origin "$1"
  fi
}

latest=$(latest_version)
if ! [[ $latest =~ $stable ]]; then
  echo "registry gave '$latest', not a stable version" >&2
  exit 1
fi
current=$(current_version)
if [ -z "$current" ]; then
  echo "could not find the goome pin" >&2
  exit 1
fi
echo "registry $latest, pinned $current"
if ! is_upgrade "$latest" "$current"; then
  echo "up to date"
  exit 0
fi
if [ -n "$(git status --porcelain)" ]; then
  echo "working tree is not clean" >&2
  exit 1
fi

branch=$(git rev-parse --abbrev-ref HEAD)
git config user.name "$(git log -1 --format=%an)"
git config user.email "$(git log -1 --format=%ae)"

pin_version "$current" "$latest"
smoke
git commit -aqm "Bump goome to $latest"
push_branch "$branch"

git checkout -q main
main_current=$(current_version)
if is_upgrade "$latest" "$main_current"; then
  pin_version "$main_current" "$latest"
  git commit -aqm "Update to goome $latest"
  push_branch main
else
  echo "main already on $main_current"
fi
git checkout -q "$branch"
