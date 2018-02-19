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

$appcmd = $($env:SystemRoot + "\System32\inetsrv\appcmd.exe")
if (![System.IO.File]::Exists($appcmd))
{
	Write-Host "APPCMD.EXE not found at expected path, is IIS installed?"
}
Set-Alias appcmd $appcmd

$assemblyPath = $($(pwd).Path + "\X4D.WebMetrics.dll")
if (![System.IO.File]::Exists($assemblyPath))
{	
	Write-Error -Message "X4D.WebMetrics.dll not found, did the solution build successfully?"
}
$assemblyName = [Reflection.AssemblyName]::GetAssemblyName($assemblyPath)
$assemblyVersion = $assemblyName.Version

Write-Host "Installing X4D.WebMetrics.dll to GAC..";
[void] [System.Reflection.Assembly]::LoadWithPartialName("System.EnterpriseServices")
$publisher = New-Object System.EnterpriseServices.Internal.Publish
$publisher.GacInstall($assemblyPath)
# TODO: confirm gac install success: ie. C:\Windows\assembly\GAC_MSIL\X4D.WebMetrics\1.0.0.0__2b9f64f15f7de8a5\X4D.WebMetrics.dll

Write-Host "Installing WebMetricsHttpModule to IIS.."
appcmd delete module /MODULE.NAME:X4D_WebMetrics > $nul
appcmd add module /MODULE.NAME:X4D_WebMetrics /type:"X4D.WebMetrics.WebMetricsHttpModule, X4D.WebMetrics, Version=$assemblyVersion, Culture=neutral, PublicKeyToken=2b9f64f15f7de8a5" > $nul
