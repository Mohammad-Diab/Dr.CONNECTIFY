# Dr.CONNECTIFY

A small Windows app that turns your PC into a Wi-Fi hotspot and shares its
internet connection with other devices. It is a front end for Windows' built-in
hosted network (`netsh wlan set hostednetwork`), written in C# with WPF on
.NET Framework 4.0.

## Features

- Create a named Wi-Fi hotspot with your own SSID and password.
- Show or hide the password while typing it.
- Built-in password generator (from version 2).
- Options window for the hotspot name and key.
- Keyboard shortcuts: Left and Right arrows, F1, F2, Escape and Tab.

## Requirements

Windows with .NET Framework 4.0 or later, and a Wi-Fi adapter whose driver
supports the Microsoft Hosted Network. Creating a hosted network needs
administrator rights, so run the app elevated. The hosted-network feature is a
Windows 7/8-era capability and is not available on every adapter or on the
newer Windows 10/11 builds where Microsoft removed it.

## Usage

Download the archive from [Releases](https://github.com/Mohammad-Diab/Dr.CONNECTIFY/releases),
extract it, and run `Dr.CONNECTIFY.exe` as administrator. There is no installer.

The exe is not signed, so Windows SmartScreen may warn the first time you run
it: click **More info**, then **Run anyway**.

## Building

```
git clone https://github.com/Mohammad-Diab/Dr.CONNECTIFY.git
```

Open `Dr.CONNECTIFY.sln` in Visual Studio and build, or run:

```
MSBuild Dr.CONNECTIFY.sln /p:Configuration=Release
```

The exe is written to `Dr.CONNECTIFY\bin\Release`.

The project targets .NET Framework 4.0. Recent Visual Studio versions don't ship
its reference assemblies; if the build stops with `MSB3644`, extract the
[Microsoft.NETFramework.ReferenceAssemblies.net40](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net40)
NuGet package and point MSBuild at its `build` folder:

```
MSBuild Dr.CONNECTIFY.sln /p:Configuration=Release /p:TargetFrameworkRootPath=<package>\build
```

## Author

Mohammad Diab

## License

[Apache 2.0](LICENSE)
