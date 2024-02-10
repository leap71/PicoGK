# PicoGK Runtime files on your system

The [PicoGK C# module](https://github.com/leap71/PicoGK) relies on the [PicoGK Runtime](https://github.com/leap71/PicoGKRuntime) to do the heavy lifting. 

PicoGK Runtime is written in C++ and is dependent on the [OpenVDB](https://www.openvdb.org/) and the [GLFW](https://www.glfw.org/) open source software projects.

In order for PicoGK to work, we need to put the compiled PicoGK Runtime and its dependent modules in a place on your system that can be found when you launch your Computational Engineering app.

Here are the files that the PicoGK installer puts on your system.

## macOS

The PicoGK macOS installer copies the following files to `/usr/local/lib`

```
picogk.v.x.dylib
picogk.v.x.liblzma.5.dylib
picogk.v.x.libzstd.1.dylib
```

(with `v.x.` being the PicoGK version, for example `picogk.1.0.dylib`)

`liblzma` and `libzstd` are dependencies of OpenVDB.

To remove the PicoGK Runtime from your system, simply delete these files.

## Windows

The PicoGK Windows installer copies the following files to your `System32` directory.

```
picogk.*.dll
blosc.dll
lz4.dll
tbb12.dll
zlib1.dll
zstd.dll
```

To remove PicoGK from your system, run the uninstall procedure in the Windows settings. All of the files, except for `picogk*.dll` are dependencies of OpenVDB.

# More information

If you have custom needs for your installation behavior, you can always [compile PicoGK Runtime on your own](Compiling_PicoGKRuntime.md) and/or [create custom installers](https://github.com/leap71/PicoGKInstaller).