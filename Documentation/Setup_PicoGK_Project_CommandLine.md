# Using the Command Line tools to quickly set up a new PicoGK project

## Overview

The command line is the quickest way to set up a new project and clone the relevant GitHub submodules.

You have to have the .NET framework installed, as well as Github command line tools.

## Create a new C# console app

Open terminal and go to the location of you Github root (for example `/user/myuser/Github`).

Create a new .NET Console Project by using the following commands:

```
dotnet new console -n YourProjectName
```

## Add your project to Github

Create an empty README.md file in the root of your folder so that Github can track your project (Github doesn't track empty folders). Then add the newly created folder to Github 

```
touch README.md
git init
git add .
```

> [!NOTE]
>
> Don't forget the dot after `git add`

## Add PicoGK and other submodules

Change into the newly-created directory (your project's root) and add PicoGK, ShapeKernel and other submodules you want.

```
git submodule add https://github.com/leap71/PicoGK PicoGK
git submodule add https://github.com/leap71/LEAP71_ShapeKernel ShapeKernel
git submodule add https://github.com/leap71/LEAP71_LatticeLibrary LatticeLibrary
```

Then initialize the submodules and update them.

```
git submodule init
git submodule update --init --recursive
```

## Summary

Here is it all in one block of script. Assuming you are at the root at your Github folder, such as Documents/Github.

```
dotnet new console -n YourProjectName
cd YourProjectName
touch README.md
git init
git add .
git submodule add https://github.com/leap71/PicoGK PicoGK
git submodule add https://github.com/leap71/LEAP71_ShapeKernel ShapeKernel
git submodule add https://github.com/leap71/LEAP71_LatticeLibrary LatticeLibrary
git submodule init
git submodule update --init --recursive
```

## .gitignore

The last step you should to before you start working is add a `.gitignore` file to your project, as otherwise you will track a lot of files that are temporary. 

A good starting point is the one we use in PicoGK. You can either copy it from the PicoGK subfolder in your project, [or download it from here](https://github.com/leap71/PicoGK/blob/main/.gitignore).