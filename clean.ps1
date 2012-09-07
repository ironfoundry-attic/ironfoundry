Get-Childitem src -include bin,obj -recurse | ?{ $_.PsIsContainer -eq $true } | %{
    Write-Host -foregroundcolor 'yellow' "Removing: $_"
    Remove-Item $_ -recurse -force
}
Write-Host -foregroundcolor 'yellow' "Removing: packages"
Remove-Item packages -recurse -force
