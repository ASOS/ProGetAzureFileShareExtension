# ProGet Azure FileShare Package Store Extension
---

This repo contains the source code for a [ProGet](http://inedo.com/proget) 3.x extension that allows the use of Azure File Shares to store packages.

## How to use:
1. clone the repo
2. compile the code
3. grab the `ProGetAzureFileShareExtension.progetx` file from the output directory
4. create a folder on the ProGet server(s) `C:\ProgramData\ProGet\Extensions`
5. copy the file to the ProGet server(s) to thew new folder
6. in ProGet -> Administration -> Advanced Settings, change `Core.ExtensionsPath` to `C:\ProgramData\ProGet\Extensions`
7. restart IIS and the ProGet service
8. ensure that the account that proget is running under has modify rights to %TEMP%
9. in ProGet -> Administration -> Manage Feeds -> <feed> -> Package Store -> Change, paste the following
```
<ProGetAzureFileShareExtension.AzureFileShareNuGetPackageStore Assembly="ProGetAzureFileShareExtension">
  <Properties RootPath="P:\PackageFolder"
              DriveLetter="P:"
              FileShareName="AzureFileShareName"
              UserName="StorageAccountName"
              AccessKey="StorageAccountAccessKey"
              LogFileName="c:\temp\somelogfilepath.log" />
</ProGetAzureFileShareExtension.AzureFileShareNuGetPackageStore>
```
*Note* `LogFileName` is optional. If not provided, logging is disabled.


## Why does this extension exist?
Azure File Shares are the closest thing to a SAN available on Azure. Unfortunately, they do not support domain account security - they have their own username and password (access key). This means that there is no transparent network access to the share.

Microsoft recommends that you use mapped network drives, but as [various](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/05/27/persisting-connections-to-microsoft-azure-files.aspx) [articles](http://fabriccontroller.net/blog/posts/using-the-azure-file-service-in-your-cloud-services-web-roles-and-worker-role/) show, this doesn't work very well when running under a service account or IIS AppPool.

Therefore, this extension attempts to reconnect the network share before use. Other than that, this extension is (in theory) identical to the default nuget package store built into ProGet.

## Useful information
* [Extending ProGet Package Store Tutorial](http://inedo.com/support/tutorials/extending-proget-package-store)

## Note
This code is [based](https://github.com/asos/ProGetAzureFileShareExtension/commit/4c0549193c7010b5a1c8567a780a1f13ee9ca530) on the DefaultNuGetPackageStore class graciously provided by Inedo, and change has deliberately been limited to only modifications.

This has been tested against ProGet 3.8.6. It is known not to work against 4.x.

## todo
- [ ] Refactor
- [ ] Determine if its possible to inherit from DefaultNugetPackageStore, and call base methods to insulate against changes
- [ ] Change back to using SDK (instead of dll's in the lib folder) once new version released by Inedo

## Licence

Copyright 2016 ASOS.com Limited

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

[Portions](https://github.com/asos/ProGetAzureFileShareExtension/commit/4c0549193c7010b5a1c8567a780a1f13ee9ca530) of this code 
based upon code written and provided by [Inedo](https://inedo.com).
