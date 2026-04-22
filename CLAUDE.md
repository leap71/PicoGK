---
title: "PicoGK — Compact Geometry Kernel"
type: library
scope: computational-engineering
status: active
version: "1.7.7.5"
last_updated: "2026-04-21"
technology: [C#, .NET10, OpenVDB, P/Invoke, NuGet]
domain: [Geometrie, Tragwerkslabor, BIM, Computational-Engineering, 3D-Druck]
license: Apache-2.0
institution: LEAP 71 (extern) — Integration Tragwerkslabor Hochschule Mainz
git_remote: https://github.com/leap71/PicoGK
related:
  - path: /Users/philippschafer/Projekte/JaxFEM/CLAUDE.md
    label: JaxFEM — FEM-Berechnungen (Synergie: Geometrie → FEM-Mesh)
  - path: /Users/philippschafer/Library/CloudStorage/OneDrive-HochschuleMainz-UniversityofAppliedSciences/03_Tragwerkslabor/06_Projekte/2026_03_11_MIB/CLAUDE.md
    label: MIB-Vault — Forschungsdokumentation Tragwerkslabor
  - path: /Users/philippschafer/Projekte/brush-app-aarch64-apple-darwin
    label: Brush — 3D Gaussian Splatting (Scan → Voxel/Mesh Pipeline)
---
# PicoGK — Compact Geometry Kernel

Quelloffener 3D-Geometrie-Kernel von **LEAP 71** (Dubai) für **Computational Engineering Models (CEM)**.
Basiert auf OpenVDB. Vertrieb als NuGet-Paket. Unterstützt macOS ARM64 und Windows x64.

> "Pico" (winzig) + "GK" (Geometry Kernel) — kleiner Befehlssatz, maximale Wirkung.

---

## Was ist PicoGK?

PicoGK ist kein CAD-System, sondern ein **programmatischer Geometrie-Kern**: Geometrie wird als
Code definiert, nicht gezeichnet. Kern-Paradigma: Implizite Funktionen und Voxel-Felder statt
explizite Polygone. Das ermöglicht:

- **Boolesche Operationen** ohne topologische Probleme
- **Komplexe organische Formen** (Gyroids, Bio-Strukturen, Wärmetauscher)
- **3D-Druck-Workflow** (SDF → Voxels → Mesh → STL/CLI-Schichten)
- **Strukturanalyse-Vorbereitung** (Geometrie → FEM-Mesh, Synergie mit JaxFEM)

---

## Schlüsselpfade

```
picogk/
├── PicoGK.csproj              # Build: dotnet build / dotnet pack
├── PicoGK_Library.cs          # Einstiegspunkt: Library.Go(...)
├── PicoGK_Voxels.cs           # Haupt-Datenstruktur (OpenVDB Level-Set)
├── PicoGK_Mesh.cs             # Dreiecksnetze, STL-Import/-Export
├── PicoGK_ScalarField.cs      # Skalarfelder (Dichte, Temperatur, ...)
├── PicoGK_VectorField.cs      # Vektorfelder (Kraft, Strömung, ...)
├── PicoGK__Interop.cs         # P/Invoke → native C++ Bibliothek
├── PicoGK_Viewer.cs           # 3D-Echtzeit-Viewer (OpenGL)
├── PicoGK_Lattice.cs          # Gitterstrukturen
├── native/
│   ├── osx-arm64/             # macOS ARM64: picogk.1.7.dylib (12.3 MB)
│   └── win-x64/               # Windows x64: picogk.1.7.dll (4.9 MB)
└── ViewerEnvironment/         # HDRI-Umgebungen für 3D-Viewer (Barcelona, DarkStudio, ...)
```

---

## Kernkonzepte

### 1. Voxels — Zentrale Datenstruktur

OpenVDB Level-Set: Signiertes Distanzfeld (SDF). Negative Werte = Inneres, Positive = Äußeres.

```csharp
// Von Mesh
Voxels vox = new Voxels(mesh);

// Von impliziter Funktion
Voxels vox = new Voxels(myImplicitFunc);

// Boolesche Operationen
vox1.BoolAddAll(vox2);         // Union
vox1.BoolSubtract(vox2);       // Subtraktion
vox1.BoolIntersect(vox2);      // Schnittmenge
```

### 2. IImplicit / IBoundedImplicit — Kern-Interface

Jede Geometrie implementiert `float fSignedDistance(in Vector3 vec)`.
Erlaubt beliebige mathematische Formen ohne Polygonisierung.

### 3. Mesh ↔ Voxels Konvertierung

- Mesh → Voxels: `new Voxels(mesh)` (Rasterisierung via Marching Cubes)
- Voxels → Mesh: `mesh = Voxels.mshAsMesh()` (Oberflächen-Extraktion)

### 4. Library.Go() — Initialisierungsmuster

```csharp
Library.Go(
    fVoxelSizeMM: 0.1f,    // Voxelgröße in mm
    fnTask: () => {
        // Geometrie-Code hier
        Library.oTheViewer.ObjectList.Add(voxels);
    }
);
```

---

## Felder-Typen

| Klasse          | Inhalt            | Anwendung                          |
| --------------- | ----------------- | ---------------------------------- |
| `Voxels`      | Level-Set SDF     | Geometrie, boolesche Ops, 3D-Druck |
| `ScalarField` | Float pro Voxel   | Dichte, Temperatur, FEM-Ergebnisse |
| `VectorField` | Vector3 pro Voxel | Kräfte, Strömungen, Gradienten   |

---

## I/O-Formate

| Format   | Klasse                 | Richtung | Verwendung                      |
| -------- | ---------------------- | -------- | ------------------------------- |
| `.stl` | `PicoGK_MeshIo`      | ↔       | 3D-Druck, CAD                   |
| `.vdb` | `PicoGK_OpenVdbFile` | ↔       | OpenVDB-Datei (inkl. Metadaten) |
| `.cli` | `PicoGK_Cli`         | →       | Schichtungsdaten für 3D-Druck  |
| `.csv` | `PicoGK_Csv`         | ↔       | Datenaustausch                  |
| Bild     | `PicoGK_ImageIo`     | ↔       | Slices, Visualisierung          |

---

## Build & Abhängigkeiten

```bash
dotnet build    # Bibliothek bauen
dotnet pack     # NuGet-Paket erstellen (.nupkg)
```

## Beispiele starten

Beispiele liegen unter `examples/`. Jedes Beispiel ist ein eigenes C#-Projekt.

```bash
cd examples/01_Quader
dotnet run
```

**Wichtig:** Die `.csproj`-Dateien der Beispiele verwenden noch `net9.0` — vor dem ersten `dotnet run` auf `net10.0` ändern (einmalig pro Beispiel), da nur .NET 10 installiert ist.

Der PicoGK-Viewer öffnet sich als eigenes Fenster und zeigt die 3D-Geometrie. Das Programm beendet sich automatisch, wenn das Viewer-Fenster geschlossen wird. Ausgabe-STL-Dateien landen im Projektverzeichnis des Beispiels.

**Target Framework**: .NET 10.0 (`.csproj` auf `net10.0` setzen — .NET 9 ist nicht mehr installiert)
**NuGet-Paket**: `PicoGK` v1.7.7.5

### Native Abhängigkeiten (in Binary eingebettet)

- **OpenVDB** — Sparse-Voxel-Struktur (Mozilla Public License 2.0)
- **Intel TBB** — Parallelisierung
- **zstd / lzma / lz4 / zlib / blosc** — Kompression

---

# PicoGK — Datei-Index (C# Namespace `PicoGK`)

### Kern-Infrastruktur

| Datei                                                                                                       | Klasse(n)                                                                                                                                      | Was es tut                                                                                                                                                            |
| ----------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK__Config.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK__Config.cs)   | `Config`                                                                                                                                     | Einzige Konfiguration: Name der nativen `.dylib/.dll` (`picogk.1.7`)                                                                                              |
| [PicoGK__Interop.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK__Interop.cs) | `Library`, `Mesh`, `Lattice`, `Voxels`, `PolyLine`, `Viewer`, `OpenVdbFile`, `ScalarField`, `VectorField`, `FieldMetadata` | **Alle P/Invoke-Deklarationen** — dünne C#-Brücke zur nativen C++-Runtime. Enthält auch `IDisposable`-Implementierungen und Destruktoren für alle Typen. |
| [PicoGK_Library.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Library.cs)   | `Library`                                                                                                                                    | Einstiegspunkt:`Library.Go()` (mit Viewer) oder `new Library(fVoxelSizeMM)` (headless). Verwaltet Voxelgröße global, Logging, Viewer-Thread, Licht-Setup.       |
| [PicoGK_Log.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Log.cs)           | `LogFile`                                                                                                                                    | Thread-sicheres Logging in Datei                                                                                                                                      |

---

### Geometrie-Primitive

| Datei                                                                                                         | Klasse(n)                                                                                      | Was es tut                                                                                                                               |
| ------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK_Types.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Types.cs)         | `Coord`, `Triangle`                                                                        | Basisstrukturen:`Coord` (3×int Voxel-Koordinate), `Triangle` (3×int Vertex-Index). Memory-Layout exakt kompatibel mit C++.         |
| [PicoGK_BBox.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_BBox.cs)           | `BBox2`, `BBox3`                                                                           | 2D/3D Bounding Boxes:`Include()`, `Grow()`, `bContains()`, `vecCenter()`, `oFitInto()` (skaliert+verschiebt BBox in Ziel-BBox) |
| [PicoGK_Color.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Color.cs)         | `ColorFloat`, `ColorHLS`, `ColorBgr24`, `ColorBgra32`, `ColorRgb24`, `ColorRgba32` | Farbtypen mit Konvertierungen, Hex-String-Parsing (`"FF0000"`), Interpolation                                                          |
| [PicoGK_VectorExt.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_VectorExt.cs) | `Vector3Ext`                                                                                 | Extension-Methods für `Vector3`: `vecNormalized()`, `vecMirrored()`, `vecTransformed()`                                         |

---

### Geometrie-Objekte

| Datei                                                                                                                               | Klasse(n)                                       | Was es tut                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| ----------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK_Voxels.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Voxels.cs)                             | `Voxels`, `IImplicit`, `IBoundedImplicit` | **Kernklasse.** Signed-Distance-Field auf OpenVDB-Basis. Konstruktoren aus Mesh, Lattice, Implicit-Funktion. Boolean-Operationen (`BoolAdd`/`BoolSubtract`/`BoolIntersect`, Operatoren `+`/`-`/`&`). Morphologie: `Offset`, `DoubleOffset`, `TripleOffset`/`Smoothen`, `Fillet`, `voxShell`. Abfragen: `CalculateProperties` (Volumen+BBox), Ray-Cast, Surface-Normal, Closest-Point. Slicing: `GetVoxelSlice` (SDF/BW/Antialiased). |
| [PicoGK_VoxelsIo.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_VoxelsIo.cs)                         | `Voxels` (partial)                            | I/O:`voxFromVdbFile()` (lädt ersten kompatiblen Level-Set aus `.vdb`), `SaveToVdbFile()`                                                                                                                                                                                                                                                                                                                                                                        |
| [PicoGK_Mesh.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Mesh.cs)                                 | `Mesh`                                        | Dreiecks-Mesh: Vertices + Triangles hinzufügen, Quads, Transformieren (Scale+Offset, Matrix4x4), Spiegeln,`Append()`, `oBoundingBox()`. Wird aus `Voxels` konstruiert (Marching Cubes via nativer Lib).                                                                                                                                                                                                                                                         |
| [PicoGK_MeshIo.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_MeshIo.cs)                             | `Mesh` (partial)                              | STL-I/O (binär): Lesen mit automatischer Einheitenerkennung (UNITS=mm/cm/m/ft/in im Header), Schreiben mit optionalem Offset+Scale. ASCII-STL nicht implementiert.                                                                                                                                                                                                                                                                                                    |
| [PicoGK_MeshMath.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_MeshMath.cs)                         | `Mesh` (partial)                              | `bFindTriangleFromSurfacePoint()`, `bPointLiesOnTriangle()` (Barycentric-Test via Kreuzprodukte)                                                                                                                                                                                                                                                                                                                                                                   |
| [PicoGK_Lattice.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Lattice.cs)                           | `Lattice`                                     | Primitiv für Voxelisierung:`AddSphere(center, r)`, `AddBeam(A, rA, B, rB, roundCap)`. Wird an `Voxels(lat)` übergeben.                                                                                                                                                                                                                                                                                                                                         |
| [PicoGK_PolyLine.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_PolyLine.cs)                         | `PolyLine`                                    | Farbige 3D-Linie für Viewer: Vertices hinzufügen,`AddArrow()`, `AddCross()`, BBox                                                                                                                                                                                                                                                                                                                                                                                |
| [PicoGK_TriangleVoxelization.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_TriangleVoxelization.cs) | `Mesh` (partial), `ImplicitMesh`            | `voxVoxelizeHollow(thickness)` — voxelisiert ein Mesh als hohle Schale via Implicit-Funktion (Distanz zu Dreiecken)                                                                                                                                                                                                                                                                                                                                                 |

---

### Skalar- und Vektorfelder

| Datei                                                                                                                 | Klasse(n)                                                                                                                          | Was es tut                                                                                                                                                                                                                                                                                                        |
| --------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK_ScalarField.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ScalarField.cs)     | `ScalarField`, `ITraverseScalarField`                                                                                          | Sparse 3D-Skalarfeld (float pro Voxel). Konstruktoren aus Voxels (SDF extrahieren oder mit konstantem Wert füllen).`SetValue`/`bGetValue`/`RemoveValue`, Slice-Abfrage, `TraverseActive()` (Callback für alle aktiven Voxel). Implementiert `IImplicit` (kann als Implicit in Voxels rendert werden). |
| [PicoGK_VectorField.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_VectorField.cs)     | `VectorField`, `ITraverseVectorField`                                                                                          | Sparse 3D-Vektorfeld (Vector3 pro Voxel). Aus Voxels: Gradient-Feld oder konstantem Vektor.`SetValue`/`bGetValue`/`RemoveValue`, `TraverseActive()`.                                                                                                                                                      |
| [PicoGK_FieldMetadata.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_FieldMetadata.cs) | `FieldMetadata`, `IFieldWithMetadata`                                                                                          | Key-Value-Tabelle (string/float/Vector3) die in `.vdb`-Dateien gespeichert wird. Interne PicoGK-Felder (`PicoGK.*`, `class`, `name`, `file_*`) sind schreibgeschützt.                                                                                                                                  |
| [PicoGK_FieldUtils.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_FieldUtils.cs)       | `SdfVisualizer`, `ActiveVoxelCounterScalar`, `SurfaceNormalFieldExtractor`, `VectorFieldMerge`, `AddVectorFieldToViewer` | Utilities: SDF-Slices als farbkodierte TGA-Bilder exportieren, aktive Voxel zählen, Surface-Normalen aus Voxels extrahieren (mit Richtungsfilter), Vektorfelder zusammenführen, Vektorfeld als Pfeil-Polylines im Viewer anzeigen                                                                               |

---

### I/O & Dateiformate

| Datei                                                                                                             | Klasse(n)                                                                                                                                               | Was es tut                                                                                                                                                                                                      |
| ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK_OpenVdbFile.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_OpenVdbFile.cs) | `OpenVdbFile`                                                                                                                                         | Container für mehrere OpenVDB-Felder (Voxels, ScalarField, VectorField) in einer `.vdb`-Datei. Laden/Speichern, Zugriff per Index oder Name, Typ-Abfrage, PicoGK-Voxelgröße wird in Metadaten gespeichert. |
| [PicoGK_Image.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Image.cs)             | `Image`, `ImageBWAbstract`, `ImageGrayscaleAbstract`, `ImageColorAbstract`, `ImageGrayScale`, `ImageRgba32`, `ImageRgb24`, `ImageColor` | Bild-Klassenbaum (BW/Graustufen/Farbe). Float-basiert intern, Konvertierung in/aus byte-Formaten. Bilineare Interpolation bei normalisierten Koordinaten. Bresenham-Linien zeichnen.                            |
| [PicoGK_ImageIo.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ImageIo.cs)         | `TgaIo`                                                                                                                                               | TGA-Lesen und -Schreiben (8-Bit Graustufen, 24-Bit Farbe). Kein RLE-Support.                                                                                                                                    |
| [PicoGK_Cli.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Cli.cs)                 | `CliIo`                                                                                                                                               | ASCII/Binär CLI-Format (Common Layer Interface) I/O — Schicht-Konturformat für Additive Manufacturing                                                                                                        |
| [PicoGK_Csv.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Csv.cs)                 | `IDataTable`, CSV-Klassen                                                                                                                             | Generisches Datentabellen-Interface + CSV-Reader/-Writer                                                                                                                                                        |
| [PicoGK_Slice.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Slice.cs)             | `PolyContour`, `PolySlice`, `PolySliceStack`                                                                                                      | Polygon-Konturen pro Schicht (für Slicing-Workflows): Winding-Erkennung, Schichtstapel                                                                                                                         |

---

### Viewer

| Datei                                                                                                                     | Klasse(n)                                        | Was es tut                                                                                                                                                                                                                                                                                         |
| ------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PicoGK_Viewer.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Viewer.cs)                   | `Viewer`                                       | OpenGL-3D-Viewer (läuft im Haupt-Thread).`Add()`/`Remove()` für Meshes, Voxels, PolyLines. Gruppen (`SetGroupVisible`, `SetGroupMaterial`, `SetGroupMatrix`). Orbit+Zoom per Maus. Perspective/Ortho. Screenshots. Hintergrundfarbe. Licht-Setup aus ZIP (Diffuse.dds + Specular.dds). |
| [PicoGK_ViewerActions.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ViewerActions.cs)     | `IViewerAction`, diverse `*Action`-Klassen   | Thread-sichere Action-Queue für Viewer-Operationen (Producer-Consumer-Pattern)                                                                                                                                                                                                                    |
| [PicoGK_ViewerAnimation.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ViewerAnimation.cs) | `AnimationQueue`                               | Verwaltet mehrere `Animation`-Objekte gleichzeitig im Viewer                                                                                                                                                                                                                                     |
| [PicoGK_ViewerKeyboard.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ViewerKeyboard.cs)   | `EKeys`, `IKeyHandler`, `KeyActionHandler` | Keyboard-Event-System für Viewer: Tastenzuordnung, Handler-Kette                                                                                                                                                                                                                                  |
| [PicoGK_ViewerTimelapse.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_ViewerTimelapse.cs) | `TimeLapse`                                    | Automatische Screenshot-Sequenz in einstellbaren Zeitabständen                                                                                                                                                                                                                                    |

---

### Utilities & Mathematik

| Datei                                                                                                         | Klasse(n)     | Was es tut                                                                                                                           |
| ------------------------------------------------------------------------------------------------------------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| [PicoGK_Utils.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Utils.cs)         | `Utils`     | Hilfsfunktionen: Pfad-Handling (Quotes entfernen, Doku-Ordner, Executable-Ordner), Datei-Warten, Cube-Mesh erstellen,`matLookAt()` |
| [PicoGK_Animation.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Animation.cs) | `Animation` | Zeit-basierte Animation (0…1):`Once`, `Repeat`, `Wiggle`-Modus mit konfigurierbarer Dauer und `IAction`-Callback            |
| [PicoGK_Easing.cs](vscode-webview://1psb3rbcsknsrr7qfrtanm64ei3bnop5qr1kmikakabh80jlrcqt/PicoGK_Easing.cs)       | `Easing`    | Easing-Kurven (EaseIn, EaseOut, EaseInOut) für Animationen — Float 0..1 → eased 0..1                                              |

---

### Architektur auf einen Blick

```
Library.Go() / new Library(voxelSize)
    └─ Native picogk.1.7.dylib (OpenVDB, OpenGL)
           ↕  P/Invoke  (PicoGK__Interop.cs)
    ┌──────────────────────────────────────┐
    │  Voxels  ←→  Mesh  ←→  Lattice      │  Geometrie
    │  ScalarField  VectorField            │  Felder
    │  OpenVdbFile  (.vdb I/O)             │  Persistenz
    │  Viewer  (OpenGL, Haupt-Thread)      │  Darstellung
    │  Image / TgaIo / CliIo / CsvIo      │  2D/Slicing I/O
    └──────────────────────────────────────┘
```

Der zentrale Workflow: `Lattice`/`IImplicit`/`Mesh` → `Voxels` (OpenVDB Level-Set) → Boolean-Ops → `Mesh` → `SaveToStlFile` oder direkt im `Viewer` anzeigen.

---

## Relevanz für Philipps Forschung

### Direkte Anknüpfungspunkte

| Thema                             | Synergie                                                                                      |
| --------------------------------- | --------------------------------------------------------------------------------------------- |
| **JaxFEM**                  | PicoGK erzeugt Geometrie → STL-Export → GMSH Mesh → JaxFEM FEM-Analyse                     |
| **Scan-to-BIM** (Brush App) | 3D-Scan (Gaussian Splatting) → Mesh-Rekonstruktion → PicoGK Voxelisierung → BIM-Geometrie  |
| **Strukturforschung**       | Implizite Topologieoptimierung: PicoGK kann Gitter/Lattice-Strukturen für Leichtbau erzeugen |
| **3D-Druck Labor**          | Direkte STL/CLI-Pipeline für additiv gefertigte Prüfkörper                                 |
| **Digitaler Zwilling**      | VDB-Dateien als Geometrie-Grundlage für Simulations-Workflows                                |

### Potenzielle Forschungsanwendungen

1. **Bioinspirated Strukturen** — Gyroid-basierte Leichtbau-Querschnitte für Tragwerke
2. **Topologieoptimierung** — PicoGK als Geometrie-Backend für iterative FEM + Formoptimierung
3. **Generative Geometrie** — Parametrische Tragwerksgeometrie via C#-Code
4. **Prüfkörper-Generierung** — Standardisierte Geometrien für Materialversuche im Labor

---

## Einschränkungen

- **Kein Linux-Support** (nur macOS ARM64 + Windows x64 als native Binaries)
- **Kein direkter IFC-Export** — Bridge-Code erforderlich für BIM-Integration
- **Keine GUI** außer dem integrierten 3D-Viewer — rein programmatisch
- **C# only** — kein Python-Binding (anders als JaxFEM/JAX)
