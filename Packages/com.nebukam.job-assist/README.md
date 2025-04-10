![stable](https://img.shields.io/badge/stable-darkgreen.svg)
![version](https://img.shields.io/badge/dynamic/json?color=blue&label=version&query=version&url=https%3A%2F%2Fraw.githubusercontent.com%2FNebukam%2Fcom.nebukam.job-assist%2Fmaster%2Fpackage.json)
![in development](https://img.shields.io/badge/status-in%20development-blue.svg)
[![doc](https://img.shields.io/badge/documentation-darkgreen.svg)](https://nebukam.github.io/docs/unity/com.nebukam.job-assist/)

# N:JobAssist
#### Helper library for Unity's Job System

N:JobAssist is a mini-library that help streamlining interactions with Unity's Job System through wrappers, grouping and chaining automation.
It was initially designed for [**N:ORCA**](https://github.com/Nebukam/com.nebukam.orca) & [**N:Geom**](https://github.com/Nebukam/com.nebukam.geom), but ended in its own package for practical purposes.

---
## Hows

### Installation
> To be used with [Unity's Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html).  
> ⚠ [Git Dependency Resolver For Unity](https://github.com/mob-sakai/GitDependencyResolverForUnity) must be installed *before* in order to fetch nested git dependencies. (See the [Installation troubleshooting](#installation-troubleshooting) if you encounter issues).  

See [Unity's Package Manager : Getting Started](https://docs.unity3d.com/Manual/upm-parts.html)

---
## Dependencies
- **Unity.Collections 1.3.1** [com.unity.collections]()
- **Unity.Jobs 0.51.0** [com.unity.jobs]()

---
## Installation Troubleshooting

After installing this package, Unity may complain about missing namespace references error (effectively located in dependencies). What [Git Dependency Resolver For Unity](https://github.com/mob-sakai/GitDependencyResolverForUnity) does, instead of editing your project's package.json, is create local copies of the git repo *effectively acting as custom local packages*.
Hence, if you encounter issues, try the following:
- In the project explorer, do a ```Reimport All``` on the **Packages** folder (located at the same level as **Assets** & **Favorites**). This should do the trick.
- Delete Library/ScriptAssemblies from you project, then ```Reimport All```.
- Check the [Resolver usage for users](https://github.com/mob-sakai/GitDependencyResolverForUnity#usage)


