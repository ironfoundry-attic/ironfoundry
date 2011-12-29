# Example of a powershell installation file that will retrieve a local IP address to set
# the LOCALROUTE property correctly

$localIP = (Get-WmiObject -Class Win32_NetworkAdapterConfiguration -Filter IPEnabled=True -ComputerName . | ? { $_.IPAddress -ne $null }).IPAddress[0]

Write-Host "Command: msiexec -ArgumentList '/q', '/i', 'IronFoundry.Dea.Service.x64.msi', '/l*v', 'install.log', 'NATSHOST=10.91.2.12', 'NATSPORT=4222', `"LOCALROUTE=$localIP`", 'INSTALLDIR=D:\IronFoundry\DEA', 'APPDIR=D:\IronFoundry\apps', 'DROPLETDIR=D:\IronFoundry\droplets'"

Start-Process -Wait -FilePath msiexec -ArgumentList '/q', '/i', 'IronFoundry.Dea.Service.x64.msi', '/l*v', 'install.log', 'NATSHOST=10.91.2.12', 'NATSPORT=4222', "LOCALROUTE=$localIP", 'INSTALLDIR=D:\IronFoundry\DEA', 'APPDIR=D:\IronFoundry\apps', 'DROPLETDIR=D:\IronFoundry\droplets'
