Installing .NET Warden Service
---------------------------------
* Operating Systems:
  * Tested on Windows 8 and Server 2012
* Prerequisites:
  * .NET Framework 4.5
  * Edit group policy to remove password complexity requirements
    * `secpol.msc` -> "Account Policies" -> "Password Policy"
  * Install IIS with Hostable Web Core
  * Ensure `Administrators` group owns `C:\IronFoundry` and has `Full Control`. The Ruby DEA runs as `Local Service` so ensure this user has full control as well.
  * Create dedicated user in `Administrators` group to run Warden service as. Admin user is required due to the fact that the Warden service creates unprivileged user accounts for containers.
    `NT AUTHORITY\Local Service` does not have these permissions. `Local System` can not be used due to the fact that the service uses the `CreateProcessWithLogonW` API call to run subprocesses.
  * Set `powershell` execution policy to `RemoteSigned`. Don't forget to also set 32-bit powershell here `C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe` (may not be required)
 
Be sure to update your Cloud Controller's `config/stacks.yml` to recognize the `mswin-clr` stack:

```
vagrant@precise64:/vagrant$ cat cloud_controller_ng/config/stacks.yml 
default: lucid64

stacks:
  - name: lucid64
    description: Ubuntu Lucid 64-bit
  - name: mswin-clr
    description: Microsoft .NET / Windows 64-bit
```
