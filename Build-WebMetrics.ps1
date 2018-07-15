param(
	[string] $configuration,
	[switch] $ignoreErrors)

if (!$ignoreErrors)
{
	$ErrorActionPreference = "Stop"
}

if ([System.String]::IsNullOrEmpty($configuration))
{
	$configuration = "Debug"
}

# TODO: consider using something like `vswhere` or `vssetup.powershell` to resolve MSBuild tools location.
# NOTE: we check a few well-known paths which should be fine for most environments, but will probably/eventually break for someone:
$msbuildPaths = @(
	$(${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"),
	$(${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"),
	$(${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"),
	$(${env:ProgramFiles(x86)} + "\MSBuild\14.0\Bin\MSBuild.exe"),
	$(${env:ProgramFiles(x86)} + "\MSBuild\12.0\Bin\MSBuild.exe")
)
$msbuild = $null
$msbuildPaths | %{
	if ([System.String]::IsNullOrEmpty($msbuildLocated) -and [System.IO.File]::Exists($_))
	{
		$msbuild = $_
	}
}
if ([System.String]::IsNullOrEmpty($msbuild))
{
	Write-Error "Could not resolve an appropriate version of MSBuild, is Visual Studio and/or Compiler Tools installed?"
}
Set-Alias msbuild $msbuild

Write-Host "Building Solution..";
msbuild /target:Clean,Rebuild /verbosity:normal /p:Configuration="$configuration" /p:Platform="Any CPU"

$assemblyPath = $($(pwd).Path + "\X4D.WebMetrics\bin\$configuration\X4D.WebMetrics.dll")
if (![System.IO.File]::Exists($assemblyPath))
{
	Write-Error "X4D.WebMetrics.dll not found at expected location, did the build succeed?"
}
