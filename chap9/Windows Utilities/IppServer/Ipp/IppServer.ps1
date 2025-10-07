while ($true) 
{
    Write-Output "Starting ippServer..."
    Start-Process "ippserver.exe -p 631 VPrinter" -Wait
    Write-Output "ippServer crashed. Restarting in 5 seconds..."
    Start-Sleep -Seconds 5
}