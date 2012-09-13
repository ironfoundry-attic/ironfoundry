#
# Copyright [2011] [Tier 3 Inc.]
# http://www.apache.org/licenses/LICENSE-2.0
#
# When run, will delete all Application Pools and Web Sites on the local machine.
#

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 'Latest' -ErrorAction 'Stop' -Verbose

$iisVersion = Get-ItemProperty "HKLM:\software\microsoft\InetStp"
if ($iisVersion.MajorVersion -eq 7)
{
    if ($iisVersion.MinorVersion -ge 5)
    {
        Import-Module WebAdministration
    }
    else
    {
        if (-not (Get-PSSnapIn | ? { $_.Name -eq "WebAdministration" }))
        {
            Add-PSSnapIn WebAdministration
        }
    }
}

Get-ChildItem IIS:\AppPools | %{ Stop-WebAppPool -Name $_.Name; }
Get-ChildItem IIS:\Sites | %{ Stop-Website -Name $_.Name; Remove-Website -Name $_.Name }
Get-ChildItem IIS:\AppPools | %{ Remove-WebAppPool -Name $_.Name }
