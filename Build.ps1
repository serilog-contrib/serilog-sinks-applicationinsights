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

$csproj = "./src/$projectName/$projectName.csproj"

if ($suffix) {
    & dotnet pack "$csproj" -c Release -o ./artifacts --version-suffix=$suffix
} else {
    & dotnet pack "$csproj" -c Release -o ./artifacts
}
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }

Pop-Location
