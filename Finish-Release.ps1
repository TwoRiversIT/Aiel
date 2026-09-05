git checkout main
git pull
.\build.ps1 -Release -Publish
git checkout develop
git merge --no-edit -s ours main
dotnet build
