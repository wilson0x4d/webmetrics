# X4D WebMetrics 
<img src="https://ci.appveyor.com/api/projects/status/ljbqrs82depunl04?svg=true" />

A thin `System.Web` based solution for gathering and observing metrics about content served in an ASP.&shy;NET Web Application.

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
<!-- code_chunk_output -->

* [Pre-Reqs, Compatibility](#pre-reqs-compatibility)
* [Build and Deploy](#build-and-deploy)
	* [Scripted Installation with `Install-WebMetrics.ps1`](#scripted-installation-with-install-webmetricsps1)
	* [Scripted Uninstallation with `Uninstall-WebMetrics.ps1`](#scripted-uninstallation-with-uninstall-webmetricsps1)
	* [Manual Steps](#manual-steps)
* [Customization / Restyling](#customization-restyling)
* [Configuration](#configuration)
* [Solution Overview](#solution-overview)
	* [Project: X4D.WebMetrics](#project-x4dwebmetrics)
	* [Project: X4D.WebMetrics.Tests](#project-x4dwebmetricstests)
	* [Project: X4D.WebMetrics.TestWeb](#project-x4dwebmetricstestweb)

<!-- /code_chunk_output -->


## Pre-Reqs, Compatibility

This solution has been verified using Windows 10 Pro 64-bit and a default installation if IIS with all "Application Development Features" enabled. Installation to a server environment should only require Web Server role and the ASP.&shy;NET Feature.

For maximum compatibility core projects target .NET Framework v2.0, this ensures that the resulting assembly can be loaded into both v2.0 and v4.0 runtime environments.


## Build and Deploy

Be aware that a scripted deployment requires Administrative privileges. If you do not have Administrative privileges (or cannot log into the target machine(s) to execute scripts) you will want to review the [Manual Installation Steps](#manual-installation).

Building does *not* require Administrative privileges, but you will require MSBuild v12.0 or later (ie. VS2013, VS2015 or VS2017 and associated build tools.) If you experience problems building from the command-line please submit an issue on Github.

Assuming you have the necessary tools version installed, building the solution should be easy:

1. Open a PowerShell Console
2. Change Directory to the WebMetrics Repository Root.
3. Execute the `Build-WebMetrics.ps1` PowerShell script.

The script can also be instructed to perform a deployment on successful build by supplying a `-Deploy` switch, for example:

```PowerShell
cd "Z:\Code\wilson0x4d\webmetrics"
.\Build-WebMetrics.ps1 -Deploy
```

There are separate scripts for Installation and Uninstallation, the `-Deploy` switch relies on `Install-WebMetrics.ps1` internally.


### Scripted Installation with `Install-WebMetrics.ps1`

Please be aware that this scripted install will add an assembly into the GAC. If this is undesired, or if you do not have sufficient privileges, you will want to review the [Manual Installation Steps](#manual-installation) instead.

1. Open a PowerShell Console
2. Change Directory to the WebMetrics Repository Root.
3. Copy the X4D.WebMetrics.dll assembly into the working folder.
3. Execute the `Install-WebMetrics.ps1` PowerShell script.

No special switches/parameters are required, for example:

```PowerShell
cd "Z:\Code\wilson0x4d\webmetrics"
cp .\X4D.WebMetrics\bin\Debug\X4D.WebMetrics.dll
.\Install-WebMetrics.ps1
```


### Scripted Uninstallation with `Uninstall-WebMetrics.ps1`

1. Open a PowerShell Console
2. Change Directory to the WebMetrics Repository Root.
3. Copy the X4D.WebMetrics.dll assembly into the working folder.
3. Execute the `Install-WebMetrics.ps1` PowerShell script.

By default the uninstallation script will remove the assembly previously added to the GAC. If you wish to keep the assembly in the GAC you must supply an additional `-ExcludeGac` switch, for example:

```PowerShell
cd "Z:\Code\wilson0x4d\webmetrics"
cp .\X4D.WebMetrics\bin\Debug\X4D.WebMetrics.dll
.\Uninstall-WebMetrics.ps1 -ExcludeGac
```

### Manual Steps

If for some reason you experience problems with a scripted installation/uninstallation, consider the following, these steps can be reviewed in order to perform a manual installation.


#### Manual Installation

The included Install/Uninstall scripts are designed to install an assembly in the GAC and then register the module globally for all IIS apps, which may not be desired. 

This section is an attempt to provide users with enough information to install the module to a single, specific application (useful in shared environments or, perhaps, developer workstations where this module might get in the way of other work.)

Installation is a matter of copying the `X4D.WebMetrics.dll` assembly to an appropriate location in your app (ie. the /bin/ folder) and then registering the `WebMetricsHttpModule` with the ASP.&shy;NET Runtime via the appropriate `web.config` file.

Instead of manually copying the assembly you may reference the assembly like any other from your development project, an assembly reference would then allow you to package and deploy the assembly with the rest of your web site.


##### Registering in an Integrated Mode Application/Pool

For an Integrated Mode application, add or update the follwoing config section in your `web.config`:

```xml
<configuration>
    <system.webServer>
        <modules>
            <add name="X4D_WebMetrics" type="X4D.WebMetrics.WebMetricsHttpModule,X4D.WebMetrics" />
        </modules>
    </system.webServer>
</configuration>
```


##### Registering in a Classic Mode Application/Pool

For a Classic Mode application, add or update the follwoing config section in your `web.config`:

```xml
<configuration>
    <system.web>
        <httpModules>
            <add name="X4D_WebMetrics" type="X4D.WebMetrics.WebMetricsHttpModule,X4D.WebMetrics" />
        </httpModules>
    </system.web>
</configuration>
```


#### Manual Uninstallation

To uninstall, simply remove the above registrations and then (optionally) either remove the assembly reference from your web project, or if manually copied, delete the assembly from the application /bin/ folder.

If after an uninstallation (including an re-installation) you experience problems unloading a prior version of the module please issue an `iisreset` and reverify.


## Customization / Restyling

By default, HTML content is injected into all pages/views with CSS styles to anchor it to the bottom-right of the window. The CSS styles are only applied to the outermost `<div/>`.

It is also injected with a few CSS Classes that you can use to restyle the injected content, classes are applied to all injected elements, as follows:

| CSS Class | Description |
|-|-|
| `x4d-webmetrics` | Applied to an outermost `<div/>` which encapsulates all other content.<br/>Apply `!important` overrides to relocate or resize the content. |
| `x4d-timemetrics` | Applied to the inner `<div/>` containing request timing metrics. |
| `x4d-sizemetrics` | Applied to the inner `<div/>` containing min, max and average size metrics. |
| `x4d-miscmetrics` | Applied to the inner `<div/>` containing request count and request rate metrics. |


## Configuration

There are no configuration files, registry keys, nor appsettings at this time. If you would like to see something made configurable please submit an issue on Github.


## Solution Overview
### Project: X4D.WebMetrics

Core product, contains all required components.

Requires .NET Framework 2.0 and a C# 7.x compatible compiler.


### Project: X4D.WebMetrics.Tests

Coded tests which can be used to verify various component behaviors.

Requires .NET Framework 4.7 or later and a C# 7.x compatible compiler. The .NET Framework can be retargeted if desired.


### Project: X4D.WebMetrics.TestWeb

A generic ASP.&shy;NET Web Forms scaffold which allows for manual testing of the HTTP Module. This is not a required resource, it is only provided for developer convenience/QOL.

Requires .NET Framework 4.7 or later and any C# compiler version. The .NET Framework can be retargeted if desired.
