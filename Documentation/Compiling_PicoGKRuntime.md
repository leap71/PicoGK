# Compiling the PicoGK Runtime

We compiled and tested PicoGKRuntime on Windows 64 bit and MacOS 14 Sonoma on Mac computers running Apple Silicon. Since macOS is our primary work environment, we have tested it on Mac significantly more than on Windows. There is, however, nothing fundamentally platform-specific about PicoGK. The main platform dependencies are well-established libraries like OpenVDB. [CorrieVS has forked PicoGK and made a docker environment for LINUX, check it out here.](https://github.com/CorrieVS/PicoGK)

Our code is very straightforward and mostly header-only C++ code.

## What you need

On Windows, you need **Visual Studio 2022 Community Edition** (or higher) with C++ support installed (bare bones C++ is enough).

On Mac, you need the latest version of **XCode** with C++ support, and the XCode command line tools.

In addition you need a current version of **CMake** (Download at https://cmake.org/) and **Git** (Download at https://git-scm.com/downloads) — install both with the default settings. 

**On Windows you need to restart so that Git can be found when using the command line.**

## Preparing the project

First clone the **[PicoGKRuntime](https://github.com/leap71/PicoGKRuntime)** repository to your machine. The Git path is:

```
https://github.com/leap71/PicoGKRuntime
```

Make sure to initialize the submodules, so that the OpenVDB submodule is properly initialized.

```
git submodule update --init --recursive -remote
```

PicoGKRuntime has no dependencies besides **OpenVDB** and **GLFW** (which is fetched automatically), but those libraries have plenty of dependencies (boost, blosc, etc).

To facilitate the installation of these dependencies, we have provided you with two scripts that download and install everything needed.

## Installing OpenVDB dependencies

On Mac, please run **PicoGKRuntime/Install_Dependencies/Mac.sh**

On Windows, please run **PicoGKRuntime/Install_Dependencies/Win.bat** 

The installation of the dependencies may take a while, especially on Windows.

After you have done this, you can move onto compiling the PicoGK Runtime.

## Preparing the PicoGKRuntime build environment

Start the **CMake GUI** client and specify the path to the PicoGKRuntime repository in **"Where is the source code"**.

Specify the Build subfolder under **"Where to build the libraries"**. It should like this

<img src="images/image-20231017134154856.png" style="zoom:50%;" />

Hit **Configure** and accept all defaults. After Configure has run without errors, click **Generate**.

Note: On Mac, we advise to use "Unix Makefiles" as target. If you target XCode, you will get an error in the OpenVDB CMake setup. We have reached out to the OpenVDB team why this happens. You can safely comment out the offending lines in the OpenVDB CMake files, but this should not happen and it seems like an issue on their side.

Now you can compile PicoGKRuntime on your system.

## Compiling

On Mac, go to the **Build** subdirectory in **Terminal** and type **make** [enter] to run the make tool. The build process should start and you will get the compiled picogk.*version*.dylib in the Dist subfolder of PicoGKRuntime.

On Windows, open the resulting project in Visual Studio and compile (use the release version).

## Using the compiled runtime

You either copy the resulting library to /usr/local/lib on Mac or the System32 folder on Windows (or any other folder that is in your system path).

If you don't want to copy to your system folders, you can adjust the path that PicoGK loads it from in the PicoGK/PicoGK__Config.cs file in your PicoGK submodule:

```C#
public const string strPicoGKLib = "picogk.1.1"; // dll or dylib
```

You can change this constant to load it from a specific location. However make sure **to include the .dylib or .dll extension**, as otherwise it will not work:

```C#
public const string strPicoGKLib = "/Users/myuser/GitHub/PicoGKRuntime/Dist/picogk.1.1.0.dylib";
```

On Windows, make sure all the DLLs from the OpenVDB dependencies are also in the same folder as the picogk.*version*.dll (or in Windows/System32). At this time these are **blosc.dll** and **lz4.dll**. You will find them in **Install_Dependencies\vcpkg\installed\x64-windows\bin**. These files were created when you ran Win.bat before you built PicoGKRuntime.

## Code signing on the Mac

One last thing — on Mac, you may have to sign the library using the **codesign** tool. Otherwise the library may not load. 

The necessary command line is **codesign -s LEAP71 picogk.*version*.dylib** — you need to have a valid code signing certificate for this (we used one we named LEAP71 — you can self issue this certificate, but it's a few steps).

Here is a relevant Apple article how to create self-signed certificates: https://support.apple.com/en-ae/guide/keychain-access/kyca8916/mac

[You may have to adjust your security settings as described here.](MacSecurity.md)
