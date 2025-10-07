# Define the path to the compiled cmdlet DLL
$cmdletPath = "D:\BACKUP\IPP Book\Chap3\Code\powershell\Cmdlet\PsGpaRequest.dll" 

# Load the cmdlet
Import-Module $cmdletPath

# Use the cmdlet
Get-PrinterAttributes -PrinterName "HPM528" -AttributesRequired "printer-description"
