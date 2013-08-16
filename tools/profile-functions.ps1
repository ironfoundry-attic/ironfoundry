function Get-Profiles
{
  <#
    .Synopsis
    Gets a list of user profiles on a computer.

    .Description
    This command gets a list of user priles on a computer. The info is pipable and can be used to do other useful tasks.

    .Parameter computer
    The computer on which you wish to recieve profiles. (defaults to localhost)

    .Example
    Get-Profiles -comptuer comp1
    Gets all of the profiles from comp1

    .Example
    Get-Content .\computers.txt | Get-Profiles
    Returns all of the profiles for the computers listed in computers.txt

    .Link
          Remove-Profiles
          Author:    Scott Keiffer
    Date:      08/27/09
  #>
  [CmdletBinding()]
  param ([parameter(ValueFromPipeLine=$true)][String]$computer = "localhost")
  process {
    $ErrorActionPreference = "SilentlyContinue"
    # Check for pipe input
    if ($_.Name)
    {
      $computer = $_.Name
    }
    elseif ($_)
    {
      $computer = $_
    }

    $profiles=$null
    # Get the userprofile list and then filter out the built-in accounts
    if ($computer)
    {
      $profiles = Get-WmiObject win32_userprofile -computerName $computer | ?{ $_.SID -like "S-1-5-21-*" -or $_.SID -like "S-1-5-82-*" }
      if (!$?)
      {
        Write-Warning "Unable to communicate with - $computer"
      }
    }
    else {
      Write-Warning "Unable to communicate with specified host."
    }

    if ($profiles.count -gt 0 -or ($profiles -and ($profiles.GetType()).Name -eq "ManagementObject"))
    {
      # Loop through the list of profiles
      foreach ($profile in $profiles)
      {
        Write-Verbose ("Reading profile for SID " + $profile.SID + " on $computer")
        $user = $null
        $objUser = $null
        #Create output objects
        $Output = New-Object PSObject
        # create a new secuity identifier object
        $ObjSID = New-Object System.Security.Principal.SecurityIdentifier($profile.SID)
        # Try to link the user SID to an actual user object (can fail for local accounts on remote machines,
        #  or the user no long exists but the profile still remains)
        try
        {
          $objUser = $objSID.Translate([System.Security.Principal.NTAccount])
        }
        catch
        {
          $user = "ERROR: Not Readable"
        }

        if ($objUser.Value)
        {
          $user = $objUser.Value
        }

        $Output | Add-Member NoteProperty Computer $computer
        $Output | Add-Member NoteProperty Username $user
        $Output | Add-Member NoteProperty SID $profile.SID
        $Output | Add-Member NoteProperty Path $profile.LocalPath
        #$Output | Add-Member NoteProperty Profile $profile

        Write-Output $Output
      }
    }
  }
}

function Remove-Profiles
{
  <#
    .Synopsis
    Deletes a list of profiles from a computer

    .Description
    This command deletes all profiles from a given computer. Or all profiles that have been piped in from the Get-Profiles command.

    .Parameter computer
    The computer on which you wish to delete profiles from. (defaults to localhost)

    .parameter commit
    This switch is used to write the changes. If switch is missing, no profiles will be deleted.

    .Example
    Remove-Profiles -comptuer comp1 -commit
    Deletes all of the profiles from comp1.

    .Example
    Get-Content .\computers.txt | Get-Profiles | Remove-Profiles
    Does a dry run of deleteing all user profiles from all of the computers listed in computers.txt

    .example
    Get-Content .\computers.txt | Get-Profiles | ?{$_.Username -like "*billy"} | Remove-Profiles -commit
    Deletes billy's user profile from all comptuers listed in computers.txt

    .Link
          Get-Profiles
          AUTHOR:    Scott Keiffer
    Date:      08/28/09
  #>
  [CmdletBinding()]
  param ([parameter(ValueFromPipeLine=$true)][String]$computer = "localhost", [Switch]$commit)
  begin
  {
    if (!$commit)
    {
      Write-Host -ForegroundColor Cyan "** No Profiles will be Deleted **"
    }
  }
  process
  {
    $ErrorActionPreference = "SilentlyContinue"
    # if piped info is present, use it. if not get the profiles from the given computer
    if ($_)
    {
      if (!$_.SID -or !$_.Computer)
      {
        Write-Warning "Malformed input, SID or Computer name missing from input."
        continue
      }
      #get the profile for the current SID
      $profile = Get-WmiObject -query ("select * from win32_userprofile where SID='" + $_.SID + "'") -computer $_.Computer

      #if commit flag is present do the delete
      $success = $true
      if ($commit)
      {
        Write-Verbose "Attempting to delete profile for user: " + $_.Username
        # The delete process can take some time per profile, timeout is also very high, be patient.
        try
        {
          $profile.Delete()
        }
        catch
        {
          $success = $false
        }
      }

      if ($success)
      {
        Write-Host "Deleted Profile with Username:" $_.Username "From:" $_.Computer
      }
      else
      {
        Write-Warning ("Unable to Delete or Fully Delete Profile (possibly logged in, or file in use) with Username: " + $_.Username + " From: " + $_.Computer)
        #Write-Host $profile.SID
      }
    }
    else
    {
      # Get the userprofile list and then filter out the built-in accounts
      $profiles = Get-WmiObject win32_userprofile -computer $computer | ?{ $_.SID -like "S-1-5-21*" }
      if ($?)
      {
        foreach ($profile in $profiles)
        {
          $success = $true
          if ($commit)
          {
            Write-Verbose ("Attempting to delete profile for SID: " + $profile.SID)
            # The delete process can take some time per profile, timeout is also very high, be patient.
            try
            {
              $profile.Delete()
            }
            catch
            {
              $success = $false
            }
          }
          if($success)
          {
            Write-Host "Deleted Profile with SID:" $profile.SID "From:" $computer
          }
          else
          {
            Write-Warning "Unable to Delete or Fully Delete Profile (possibly logged in, or file in use) with SID: " + $profile.SID + " From: " + $computer
          }
        }
      }
      else
      {
        Write-Warning "Unable to communicate with - $computer"
      }
    }
  }
}
