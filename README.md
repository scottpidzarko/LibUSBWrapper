# LibUSBWrapper
Wrapper to use LibUSBDotNet in PowerShell 7 to work with Crestron devices using their USB OTG port. Supports targeting any framework LibUSBDotNet does.

Example usage in powershell:

~~~~
#Wrapper class to create concrete instance of LibUsbDotNet.UsbDevice
#and also do the heavy lifting
#Since powershell can't deal with creating an abstract class
Add-Type -Path (Join-Path $PSScriptRoot "USBSession.dll")
#$VerbosePreference = "Continue"

$usb = New-Object "USBSession.USBSession"
if($null -ne $usb){
    $usb.Open()
    $usb.Invoke("") | Out-Null
    $usb.Invoke("ECHO OFF") | Out-Null
    Write-Host($usb.Invoke("VER -v"))
    $usb.Close();   
}

Read-Host -Prompt "Press any key to continue ..."
~~~~~