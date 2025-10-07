/*===========================================================================================
Hello.cpp

Simple GDI way to print a message to an IPP printer. Uses:

StartDoc, StartPage, TextOut, EndPage, and EndDoc APIs are used to illustrate how GDI will run 
document data through the IPP Class Driver when a printer associated with that driver is used.


===========================================================================================*/

#define WIN32_MEAN_AND LEAN 

#include <windows.h>
#include <iostream>

using namespace std;

int main(int argc, wchar_t* argv[])
{
    if (argc != 3) 
    {
        wcout << L"Usage: " << L"Hello: " << L" <PrinterName> <Message>" << endl;
        return 1;
    }

    // Get the command line arguments as wchar_t strings
    wchar_t** wargv = CommandLineToArgvW(GetCommandLineW(), &argc);

    LPCWSTR pPrinterName = wargv[1];
    LPCWSTR pMessage = wargv[2];
    
    wcout << L"GDI To IPP Printer Test\n";

    // Printer device context handle
    HDC hdc;
    DOCINFO di;
    BOOL bSuccess = false;

    memset(&di, 0, sizeof(DOCINFO));
    di.cbSize = sizeof(DOCINFO);
    di.lpszDocName = L"Simple GDI to IPP Print Job";

    // Get a device context for the default printer
    hdc = CreateDC(L"WINSPOOL", pPrinterName, NULL, NULL);
    if (!hdc)
    {
        wprintf(L"Failed to get printer DC.\n");
        return 1;
    }

    // Start the print job
    if (StartDoc(hdc, &di) <= 0)
    {
        wprintf(L"StartDoc failed.\n");
        goto Cleanup;
    }

    // Start the page
    if (StartPage(hdc) <= 0)
    {
        wprintf(L"StartPage failed.\n");
        EndDoc(hdc);
        goto Cleanup;
    }

    // Set text color as black
    SetTextColor(hdc, RGB(0, 0, 0));
    SetBkMode(hdc, TRANSPARENT);
    //Textout sets the coordinates on the page, here x=100, y = 100
    TextOut(hdc, 100, 100, pMessage, wcslen(pMessage));

    if (EndPage(hdc) <= 0)
    {
        wprintf(L"EndPage failed.\n");
        EndDoc(hdc);
        goto Cleanup;
    }

    // End the print job
    if (EndDoc(hdc) <= 0)
    {
        wprintf(L"EndDoc failed.\n");
        goto Cleanup;
    }
    else
    {
        bSuccess = true;
    }

    Cleanup:
    if (bSuccess == false)
    {
        DeleteDC(hdc);
        return 1;
    }

    wprintf(L"Print job sent successfully.\n");
    return 0;
}


