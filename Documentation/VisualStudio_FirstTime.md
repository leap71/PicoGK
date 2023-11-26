# Installing and running Visual Studio 2022

**Note** we show Windows screenshots. Mac is similar. 

If you are on Mac, you should know that Visual Studio has been end-of-lifed for Mac. [You might want to use Visual Studio Code instead](VisualStudioCode_FirstTime.md).

## Download and Install Visual Studio 2022

Download Visual Studio 2022 Community Edition (or higher) from: https://visualstudio.microsoft.com/vs/community/ and run the installer.

Agree to the licensing terms and wait for the download of the installation package.

<img src="images/VisualStudio2022GettingInstallerReady.png" style="zoom:50%;" />

Select .NET desktop development and accept the defaults by clicking on Install.

<img src="images/VisualStudio2022Options.png" style="zoom:50%;" />

Wait for the installation to finish.

<img src="images/VisualStudio2022Download.png" style="zoom:50%;" />

After VisualStudio 2022 is installed, click the Launch button.

*Also, close the installer after you launched Visual Studio. Sometimes Visual Studio needs to install additional packages, and this process fails, if the installer is still running in the background.*

<img src="images/VisualStudio2022Launch.png" style="zoom:50%;" />

Congratulations, you have Visual Studio up and running.

## Running the PicoGK Example Project

After you launched VisualStudio, you the welcome screen is shown. **Choose Open a project or solution**

<img src="images/VisualStudio2022Welcome.png" style="zoom:50%;" />

Browse to where you installed the PicoGK demo project (default is **Documents/PicoGK Example**)

Choose **PicoGK Example.vcproj**

<img src="images/VisualStudio2022OpenVCPRJ.png" style="zoom:50%;" />

The example project will open in Visual Studio. On the right side, you see your project files. **Program.cs** contains the main source code of the example.

Click on Program.cs to show the source code.

<img src="images/VisualStudioProgramCS.png" style="zoom:50%;" />

Click on the run PicoGK Example button at the top center of the screen.

<img src="images/VisualStudioRun.png" style="zoom:50%;" />

Visual Studio will now compile the example project and launch it.

Your screen should look like this after you clicked run. 

<img src="images/VisualStudioSuccess.png" style="zoom:50%;" />

**Congratulations. You are up and running with Visual Studio and PicoGK.**

Return to the [Main PicoGK Documentation](README.md)