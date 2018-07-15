# X4D WebMetrics 

[![Build](https://ci.appveyor.com/api/projects/status/ljbqrs82depunl04/branch/master?svg=true)](https://ci.appveyor.com/project/wilson0x4d/webmetrics/branch/master) ![netframework](https://img.shields.io/badge/.net-framework-blue.svg) [![NuGet](https://img.shields.io/nuget/v/X4D.WebMetrics.svg)](https://www.nuget.org/packages/X4D.WebMetrics) [![Downloads](https://img.shields.io/nuget/dt/X4D.WebMetrics.svg)](https://www.nuget.org/api/v2/package/X4D.WebMetrics/)


A thin `System.Web` based solution for gathering and observing metrics about content served in an ASP.&shy;NET Web Application.

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
<!-- code_chunk_output -->

* [Pre-Reqs, Compatibility](#pre-reqs-compatibility)
* [Building Locally](#building-locally)
* [Manual Registration Steps](#manual-registration-steps)
	* [Registering in an Integrated Mode Application](#registering-in-an-integrated-mode-application)
* [Customization / Restyling](#customization-restyling)
* [Configuration](#configuration)
* [Solution Overview](#solution-overview)
	* [Project: X4D.WebMetrics](#project-x4dwebmetrics)
	* [Project: X4D.WebMetrics.Tests](#project-x4dwebmetricstests)

<!-- /code_chunk_output -->


## Pre-Reqs, Compatibility

This solution has been verified using Windows 10 Pro 64-bit and a default installation if IIS with all "Application Development Features" enabled. Installation to a server environment should only require Web Server role and the ASP.&shy;NET Feature.

> NOTE: Support for .NET Framework v2.0 was dropped in the interest of integrating with newer systems and libraries where a `net20` package could not be referenced. If you still require a `net20`-compatible package, do not despair, you can still reference [X4D.WebMetrics-0.1.0.40](https://www.nuget.org/packages/X4D.WebMetrics/0.1.0.40) from your legacy projects :)


## Building Locally

Building does *not* require Administrative privileges, but you will require MSBuild v12.0 or later (ie. VS2013, VS2015 or VS2017 and associated build tools.) If you experience problems building from the command-line please submit an issue on Github.

Assuming you have the necessary tools version installed, building the solution should be easy:

1. Open a PowerShell Console
2. Change Directory to the WebMetrics Repository Root.
3. Execute the `Build-WebMetrics.ps1` PowerShell script.

```PowerShell
cd "Z:\Code\wilson0x4d\webmetrics"
.\Build-WebMetrics.ps1
```

> NOTE: Support for installation into the GAC has been dropped in favor of a deployment model which is less complex, and which will allow us to adopt newer libraries and packages that do not support installation into the GAC. This also means the primary method of integration is not to Deploy/Install the module, but to instead install/reference [the appropriate nuget package](https://www.nuget.org/packages/X4D.WebMetrics).


## Manual Registration Steps

Reference the assembly like any other from your development project, an assembly reference will allow you to package and deploy the assembly with the rest of your web site.


### Registering in an Integrated Mode Application

For an Integrated Mode application, add or update the following config section in your `web.config`:

```xml
<configuration>
    <system.webServer>

        <modules>
            <add name="X4D_WebMetrics" 
                 type="X4D.WebMetrics.WebMetricsHttpModule,X4D.WebMetrics" />
        </modules>

    </system.webServer>
</configuration>
```


#### Registering in a Classic Mode Application

For a Classic Mode application, add or update the following config section in your `web.config`:

```xml
<configuration>
    <system.web>
    
        <httpModules>
            <add name="X4D_WebMetrics" 
                 type="X4D.WebMetrics.WebMetricsHttpModule,X4D.WebMetrics" />
        </httpModules>

    </system.web>
</configuration>
```


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

Requires .NET Framework 4.6.1 or later and a C# 7.x compatible compiler, the .NET Framework can be retargeted if desired but you may experience problems with certain NuGet packages in future builds.


### Project: X4D.WebMetrics.Tests

Coded tests which can be used to verify various component behaviors.

Requires .NET Framework 4.6.1 or later and a C# 7.x compatible compiler. The .NET Framework can be retargeted if desired.

