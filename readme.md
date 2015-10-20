# ProGet Azure FileShare Package Store Extension
---

This repo contains the source code for a [ProGet](http://inedo.com/proget) extension that allows the use of Azure File Shares to store packages.

## How to use:
1. clone the repo
2. compile the code
3. grab the output dll from the bin folder (`ProGetAzureFileShareExtension.dll`) and zip it up
4. rename the zip file to `ProGetAzureFileShareExtension.progetx`
5. create a folder on the ProGet server(s) `C:\ProgramData\ProGet\Extensions`
6. copy the file to the ProGet server(s) to thew new folder
7. in ProGet -> Administration -> Advanced Settings, change `Core.ExtensionsPath` to `C:\ProgramData\ProGet\Extensions`
8. restart IIS (or the service, if it is self hosted)
9. in ProGet -> Administration -> Manage Feeds -> <feed> -> Package Store -> Change, paste the following
```
<ProGetAzureFileShareExtension.AzureFileShareNuGetPackageStore Assembly="ProGetAzureFileShareExtension">
  <Properties />
</ProGetAzureFileShareExtension.AzureFileShareNuGetPackageStore>
```
