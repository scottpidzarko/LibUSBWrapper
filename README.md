# LibUSBWrapper
Wrapper to use LibUSBDotNet in PowerShell 7 to work with Crestron devices using their USB OTG port. Supports targeting any framework LibUSBDotNet does.

Example usage in powershell:

```Powershell
#Wrapper class to create concrete instance of LibUsbDotNet.UsbDevice
#and also do the heavy lifting
#Since powershell can't deal with creating an abstract class
Add-Type -Path (Join-Path $PSScriptRoot "USBSession.dll")

$usb = New-Object "USBSession.USBSession"
if($null -ne $usb){
    $usb.Open()
    WriteHost "Session opened. Sending CRLF"
    $usb.Invoke("") | Out-Null
    Write-Host "Clearing Read Buffer"
    $usb.ClearReadBuffer()
    
    # You can skip sending the username if the device has already been logged in since it was connected, or if 
    # you have auth off
    Write-Host "Buffer cleared. Logging in, sending username"
    $usb.Invoke("admin")
    Write-Host "Sending password"
    $usb.Invoke("p@ssw0rd!")
    Write-Host "Turning Echo off"
    $usb.Invoke("ECHO OFF") | Out-Null
    Write-Host "Session ready to go."
    
    $response = $usb.Invoke("VER -v")
    #Do something with the response
    Write-Verbose $response
    Write-Host ($response -like "PUF: 1.601.0050")
    
    Write-Host "Closing session ..."
    $usb.Close();   
}

Read-Host -Prompt "Press any key to continue ..."
```
