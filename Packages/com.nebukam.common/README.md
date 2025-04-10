![preview](https://img.shields.io/badge/-preview-orange.svg)
![version](https://img.shields.io/badge/dynamic/json?color=blue&label=version&query=version&url=https%3A%2F%2Fraw.githubusercontent.com%2FNebukam%2Fcom.nebukam.common%2Fmaster%2Fpackage.json)
![in development](https://img.shields.io/badge/license-MIT-black.svg)
[![doc](https://img.shields.io/badge/documentation-darkgreen.svg)](https://nebukam.github.io/docs/unity/com.nebukam.common/)

# N:Common
#### Shared code across com.nebukam.* packages

N:Common centralize very low-level code, utils, extensions & boilerplates used across the com.nebukam namespace.

---
## Hows

### Installation
> To be used with [Unity's Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html).  
> ⚠ [Git Dependency Resolver For Unity](https://github.com/mob-sakai/GitDependencyResolverForUnity) must be installed *before* in order to fetch nested git dependencies. (See the [Installation troubleshooting](#installation-troubleshooting) if you encounter issues).  

See [Unity's Package Manager : Getting Started](https://docs.unity3d.com/Manual/upm-parts.html)


### Quick Start

```CSharp

    TBD
    

```

---
---
## N:Packages for Unity

| Package | Infos |
| :---| :---|
|**_Standalone_**|
|[com.nebukam.easing](https://github.com/Nebukam/com.nebukam.easing.git)|**N:Easing** provide barebone, garbage-free easing & tweening utilities.|
|[com.nebukam.signals](https://github.com/Nebukam/com.nebukam.signals.git)|**N:Signals** is a lightweight, event-like signal/slot lib.|
|**_General purpose_**|
|[com.nebukam.common](https://github.com/Nebukam/com.nebukam.common.git)|**N:Common** are shared resources for non-standalone N:Packages|
|[com.nebukam.job-assist](https://github.com/Nebukam/com.nebukam.job-assist.git)|**N:JobAssist** is a lightweight lib to manage Jobs & their resources.|
|[com.nebukam.slate](https://github.com/Nebukam/com.nebukam.slate.git)|**N:Slate** is a barebone set of utilities to manipulate graphs (node & their connections).|
|[com.nebukam.v-field](https://github.com/Nebukam/com.nebukam.v-field.git)|**N:V-Field** is a barebone lib to work vector fields|
|[com.nebukam.geom](https://github.com/Nebukam/com.nebukam.geom.git)|**N:Geom** is a procedural geometry toolkit.|
|[com.nebukam.splines](https://github.com/Nebukam/com.nebukam.splines.git)|**N:Splines** is a procedural geometry toolkit focused on splines & paths.|
|[com.nebukam.ffd](https://github.com/Nebukam/com.nebukam.ffd.git)|**N:FFD** is a lightweight set of utilities to create free-form deformation envelopes.|
|**_Procgen_**|
|[com.nebukam.wfc](https://github.com/Nebukam/com.nebukam.wfc.git)|**N:WFC** is a spinoff on the popular Wave Function Collapse algorithm.|
|**_Navigation_**|
|[com.nebukam.orca](https://github.com/Nebukam/com.nebukam.orca.git)|**N:ORCA** is a feature-rich Optimal Reciprocal Collision Avoidance lib|
|[com.nebukam.beacon](https://github.com/Nebukam/com.nebukam.beacon.git)|**N:Beacon** is a modular navigation solution|
|[com.nebukam.beacon-orca](https://github.com/Nebukam/com.nebukam.beacon-orca.git)|**N:Beacon** module providing a user-friendly **N:ORCA** implementation.|
|[com.nebukam.beacon-v-field](https://github.com/Nebukam/com.nebukam.beacon-v-field.git)|**N:Beacon** module providing a user-friendly **N:V-Field** implementation.|

---
## Installation Troubleshooting

After installing this package, Unity may complain about missing namespace references error (effectively located in dependencies). What [Git Dependency Resolver For Unity](https://github.com/mob-sakai/GitDependencyResolverForUnity) does, instead of editing your project's package.json, is create local copies of the git repo *effectively acting as custom local packages*.
Hence, if you encounter issues, try the following:
- In the project explorer, do a ```Reimport All``` on the **Packages** folder (located at the same level as **Assets** & **Favorites**). This should do the trick.
- Delete Library/ScriptAssemblies from you project, then ```Reimport All```.
- Check the [Resolver usage for users](https://github.com/mob-sakai/GitDependencyResolverForUnity#usage)
