Get-Childitem src -include bin,obj -recurse | ?{ $_.PsIsContainer -eq $true } | %{
    Write-Host -ForegroundColor 'yellow' "Removing: $_"
    Remove-Item $_ -Recurse -Force -ErrorAction Continue
}
if (Get-Item packages) {
  Write-Host -ForegroundColor 'yellow' "Removing: packages"
  Remove-Item packages -Recurse -Force -ErrorAction Continue
}
