# eh?
This folder is here, as the nuget package Inedo.ProGet.SDK, version 3.3.0 is not compatible with later version of ProGet.
ProGet support has said:

```
There are some missing backwards-compatibility shims in the 3.3 SDK, so it’s going to cause errors like this in some cases.

We will publish a new one soon, but in the meantime, remove your NuGet package reference, and then just copy the DLLs from the WebApp/bin folder into a /lib folder in your project, and reference those instead of the NuGet package:
- ICSharpCode.SharpZipLib.dll
- Inedo.NuGet.dll
- InedoLib.dll
- ProGetCore.dll
 
The SDK will contain exactly the same DLLS that you have already, and those are only used at compilation-time anyway.
```

Once a newer version of the SDK is available, we will need to go back to using the SDK and delete this folder.
