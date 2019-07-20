#!/usr/bin/env sh

# abort on errors
set -e

cd Assets/UnityConsole

git init
git add -A
git commit -m 'publish'
git push -f git@github.com:Elringus/UnityConsole.git master:package

cd -