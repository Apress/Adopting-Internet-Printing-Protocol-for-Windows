param 
(
    [Parameter(Mandatory=$true)]
    [string]$file,
    
    [Parameter(Mandatory=$true)]
    [string]$printer
)

try 
{
    # Start the print process and get the process object
    $process = Start-Process -FilePath $file -Verb PrintTo -ArgumentList $printer -PassThru
    
    # Did the process start without errors?
    if ($process) 
    {
        Write-Output "Printing initiated. Process ID: $($process.Id)"
        
        # Wait on the process...
        $process.WaitForExit()
        
        if ($process.ExitCode -eq 0) 
        {
            Write-Output "Printing completed..."
        } 
        else 
        {
            Write-Output "Printing process error: $($process.ExitCode)"
        }
    } 
    else 
    {
        Write-Output "Process failed to start correctly.."
    }
} 
catch 
{
    Write-Output "An error occurred: $_"
}