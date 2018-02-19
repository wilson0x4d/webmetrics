param(
	[string] $configuration,
	[switch] $ignoreErrors,
	[switch] $excludeGac
)

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

Write-Host "Uninstalling WebMetricsHttpModule from IIS (Global).."
appcmd delete module /MODULE.NAME:X4D_WebMetrics > $nul

if (!$excludeGac)
{
	Write-Host "Uninstalling X4D.WebMetrics.dll from GAC..";
	[void] [System.Reflection.Assembly]::LoadWithPartialName("System.EnterpriseServices")
	$publisher = New-Object System.EnterpriseServices.Internal.Publish
	# TODO: load assembly get assembly name proper
	$publisher.GacRemove($assemblyPath)
}

