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

## Why does this extension exist?
Azure File Shares are the closest thing to a SAN available on Azure. Unfortunately, they do not support domain account security - they have their own username and password (access key). This means that there is no transparent network access to the share.

Microsoft recommends that you use mapped network drives, but as [various](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/05/27/persisting-connections-to-microsoft-azure-files.aspx) [articles)](http://fabriccontroller.net/blog/posts/using-the-azure-file-service-in-your-cloud-services-web-roles-and-worker-role/) show, this doesn't work very well when running under a service account or IIS AppPool.

Therefore, this extension attempts to reconnect the network share before use.

## Useful information
* [Extending ProGet Package Store Tutorial](http://inedo.com/support/tutorials/extending-proget-package-store)
