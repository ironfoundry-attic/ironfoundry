get-childitem src -include bin,obj -recurse | where { $_.psIsContainer -eq $true } | foreach ($_) {
    write-host -foregroundcolor 'yellow' "Removing: $_"
    remove-item $_ -recurse -force
}
get-childitem test -include bin,obj -recurse | where { $_.psIsContainer -eq $true } | foreach ($_) {
    write-host -foregroundcolor 'yellow' "Removing: $_"
    remove-item $_ -recurse -force
}
