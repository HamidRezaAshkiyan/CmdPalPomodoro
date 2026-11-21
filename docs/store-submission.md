# Microsoft Store submission

Partner Center access is required to reserve the name and submit the package. This repo cannot complete that step until the account is available.

## Prepare the identity

1. Open [Partner Center](https://partner.microsoft.com/dashboard/home).
2. Create an MSIX product and reserve the name **Pomodoro**.
3. Copy **Package identity name**, **Publisher**, and **Publisher display name**.
4. Replace the matching values in `CmdPalPomodoro/Package.appxmanifest` and these project properties:
   - `AppxPackageIdentityName`
   - `AppxPackagePublisher`
5. Replace the development certificate with the Store signing flow. Store submission uses Partner Center signing, so remove `PackageCertificateKeyFile` for the Store build or let the Store re-sign the upload.
6. Build both architectures:

```powershell
dotnet build CmdPalPomodoro.sln -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\x64\
dotnet build CmdPalPomodoro.sln -c Release -p:Platform=ARM64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ARM64\
```

7. Bundle them with `makeappx bundle` and upload the `.msixbundle`.

## Listing text

Description:

> Pomodoro integrates with the Windows Command Palette to show a live focus countdown on the Dock. You can set focus and break lengths. When a session ends, the timer stops and reminds you to start the break or the next focus yourself.

Certification notes:

> This extension requires Microsoft PowerToys with Command Palette enabled.
> 1. Install PowerToys and enable Command Palette.
> 2. Install this extension.
> 3. Open Command Palette (Win+Alt+Space by default).
> 4. Search for Pomodoro.
> 5. Enable the Dock and add the Pomodoro band.
> 6. Start a focus session and confirm the Dock countdown updates.

## Extension Gallery

After the package is on WinGet or the Store, list it in [microsoft/CmdPal-Extensions](https://github.com/microsoft/CmdPal-Extensions) using `gallery/extension.json`. The gallery id is `hamidrezaashkiyan.cmdpal-pomodoro`.
