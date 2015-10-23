# ProGet Azure FileShare Package Store Extension
---

This repo contains the source code for a [ProGet](http://inedo.com/proget) extension that allows the use of Azure File Shares to store packages.

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

## todo
- [ ] Refactor
- [x] Add tests
- [x] Add post build task to zip into progetx file
- [x] Add validation to InitPackageStore()
- [x] Add proper logging framework
- [ ] Determine if its possible to inherit from DefaultNugetPackageStore, and call base methods (to insulate against changes)
- [x] Add missing xmldoc comments
- [x] Add log statements to useful spots
- [x] Move sln file to root
- [x] Log timings around actions
- [ ] BUG: Feed cleanup crashes for some unknown reason. Probably needs ProGet support involvement.