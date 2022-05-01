echo "build: Build started"

Push-Location $PSScriptRoot

if (Test-Path ./artifacts) {
	echo "build: Cleaning ./artifacts"
	Remove-Item ./artifacts -Force -Recurse
}

echo "build: Restoring"
& dotnet restore --no-cache
if($LASTEXITCODE -ne 0) { exit 1 }

$projectName = "Serilog.Sinks.ApplicationInsights"
$ref = $env:GITHUB_REF ?? ""
$run = $env:GITHUB_RUN_NUMBER ?? "0"
$branch = @{ $true = $ref.Substring($ref.LastIndexOf("/") + 1); $false = $(git symbolic-ref --short -q HEAD) }[$ref -ne ""];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $run, 10); $false = "local" }[$run -ne "0"];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "main" -and $revision -ne "local"]

echo "build: Version suffix is $suffix"

& dotnet test -c Release "./test/$projectName.Tests/$projectName.Tests.csproj"
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }

$src = "./src/$projectName"

& dotnet build -c Release --version-suffix=$suffix "$src/$projectName.csproj"
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

if ($suffix) {
    & dotnet pack -c Release -o ./artifacts --no-build --version-suffix=$suffix "$src/$projectName.csproj"
} else {
    & dotnet pack -c Release -o ./artifacts --no-build "$src/$projectName.csproj"
}
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }

Pop-Location
