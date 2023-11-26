# Installing on macOS

## Installing the PicoGK Runtime

At this time, we have installers for macOS on Apple Silicon (M1 upwards). There is no Intel support yet (you can compile on an Intel Mac, but we do not have Intel machines.).

[Download the macOS installer disk image of the latest release.](https://github.com/leap71/PicoGK/releases)

<img src="images/image-20231125201341187.png" style="zoom:50%;" />

Double click the disk image and accept the license agreement. The disk image opens.

<img src="images/PicoGKDMG.png" style="zoom:33%;" />

Copy the PicoGK Example project from the disk image to your Documents folder (or wherever you want to have it).

<img src="images/MacOSPicoGKExample.png" style="zoom:33%;" />

Double click **on Install PicoGK.pkg**

You will get a warning that the installer package cannot be verified (our installers are not code-signed — yet).

<img src="images/MacInstallerWarning.png" style="zoom:50%;" />

Go to System Settings and under **Privacy and Security**

Make sure **App Store and identified developers** is enabled. Then click on **Open Anyway**.

<img src="images/MacSecuritySettings.png" style="zoom:50%;"/>

After entering your credentials, the following dialog pops up.

<img src="images/Mac_OpenInstaller.png" style="zoom:50%;" />

Choose **Open**

The PicoGK Installer will open.

<img src="images/MacInstaller1.png" style="zoom:50%;" />

Accept all defaults and install the PicoGKRuntime.

<img src="images/MacInstaller2.png" style="zoom:50%;" />


## Next: Installing Visual Studio

Next you have to decide, whether you want to use **Visual Studio 2022**, or **Visual Studio Code**. 

**Visual Studio 2022** has been end-of-lifed for macOS, unfortunately, as it was the easer-to-use product. It still works fine, though, for the time being.

**Visual Studio Code** is the cross platform integrated development environment for macOS, Windows and LINUX. It's a bit harder to use, but it is Microsoft's supported platform.

- [Install Visual Studio Code](VisualStudioCode_FirstTime.md)

- [Install Visual Studio 2022](VisualStudio_FirstTime.md)

