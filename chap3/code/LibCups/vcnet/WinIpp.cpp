/*===================================================================================================
WinIpp.cpp

This is a wrapper around the CUPS APIs for IPP printing. This is intended as exploratory only code and
is not intended for production use. The accompanying utilities (IPPUtil and WinIppTest) are provided as
WPF and C++ console examples that use this dll. This code is not thread safe and therefore should not be 
used in multi-threaded projects.

Operation					Required Attributes (syntax)							Optional Attributes (syntax)
---------					----------------------------							----------------------------
Cancel-Job					job-id (integer), requesting-user-name (name)
Create-Job					requesting-user-name (name), job-name (name)			Job Attributes
Get-Job-Attributes			job-id (integer), requesting-user-name (name)			requested-attributes (1setOf keyword)
Get-Jobs					requesting-user-name (name)								my-jobs (boolean), requested-attributes (1setOf keyword), which-jobs (keyword)
Get-Printer-Attributes																document-format (mimeMediaType), requested-attributes (1setOf keyword)
Print-Job					requesting-user-name (name), job-name (name)			Job Attributes
Send-Document				job-id (integer), requesting-user-name (name)			document-format (mimeMediaType), document-name (name)


Job Attribute (syntax)					Printer Attribute (Syntax)								Standard
------------------------------------------------------------------------------------------------------------
copies (integer)						copies-supported (integer)								RFC 8011
finishings (1setOf enum)				finishings-ready (1setOf enum)							PWG 5100.1
finishings-col (1setOf collection)		finishings-col-ready (1setOf collection)				PWG 5100.1
media (keyword)							media-ready (1setOf keyword)							RFC 8011
media-col (collection)					media-col-ready (1setOf collection)						PWG 5100.3
output-bin (keyword)					output-bin-supported (1setOf keyword)					PWG 5100.2
page-ranges (rangeOfInteger)			page-ranges-supported (boolean)							RFC 8011
print-color-mode (keyword)				print-color-mode-supported (1setOf keyword)				PWG 5100.13
print-quality (enum)					print-quality-supported (1setOf enum)					RFC 8011
print-scaling (keyword)					print-scaling-supported (1setOf keyword)				PWG 5100.13
Printer-resolution (resolution)			Printer-resolution-supported (1setOf resolution)		RFC 8011
sides (keyword)							sides-supported (1setOf keyword)						RFC 8011

===================================================================================================*/
#include "pch.h"
#include "WinIpp.h"

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
/*---------------------------------------------------------------------------------------
freePrinterAttributesArrayMemory()
 
 Frees memory allocated when function getPrinterAttributesArray is used.
---------------------------------------------------------------------------------------*/
void freePrinterAttributesArrayMemory()
{
	//Free Memory
	if (gpArray != NULL)
	{
		//delete each array element char pointer...
		for (int i = 0; i < giSize; i++)
		{
			delete gpArray[i];
		}
		//delete the array pointer
		delete[] gpArray;
		gpArray = NULL;
		giSize = 0;
	}
}

/*---------------------------------------------------------------------------------------
freePrinterJobsMemory()

 Frees memory allocated when function getAllJobsOnPrinter is used.
---------------------------------------------------------------------------------------*/
void freePrinterJobsMemory()
{
	//Free Memory
	if (gpJobs != NULL)
	{
		//delete each array element char pointer...
		for (int i = 0; i < giJobs; i++)
		{
			delete gpJobs[i];
		}
		//delete the array pointer
		delete[] gpJobs;
		gpJobs = NULL;
		giJobs = 0;
	}
}

/*-----------------------------------------------------------------------------
WrGetPrinterAttributes

Gets available attributes from printer. Note: Caller must de-allocate the returned
pointer.

The returned format is a char array of string pointers. The format of the strings is:

attribute_name | attibute_value

If you get a NULL returned, look to the retCode for the error. Look at the num argument 
for the number of strings in the char array.

Note: The "requested-attributes" attribute lists attributes (or groups of attributes) that the Client is interested in. The 'printer-description' 
group asks for all status and information attributes while the 'job-template' group asks for all capability attributes.

Required:
IN:		hostname - const char pointer
IN/OUT:	num - number of attribute string in the retuned char array
IN/OUT: retCode - the returned code

Returns: Array of char string pointers.
-----------------------------------------------------------------------------*/
char** WrGetPrinterAttributes(const char* hostname, const char* userName, int* num, int* retCode, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	ipp_attribute_t* attrptr;
	const char* name;
	char value[2048];
	int i = 0, numStrings = 0, iPort = 631;  
	ipp_status_t status;
	char szUriTag[TAGSIZE] = { 0 };

	//Check the hostname..
	if((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		*retCode = status;
		return NULL;
	}

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	static const char* const requested_attributes[] =
	{
	  "printer-description",
	  "job-template",
	  "media-col-database"
	};

	http = httpConnect(hostname, iPort, NULL, AF_UNSPEC, (http_encryption_t) encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		*retCode = status;
		return NULL;
	}

	/*-----------------------------------------------------------------------------------
	IPP_OP_GET_PRINTER_ATTRIBUTES is for printer attributes.
	-----------------------------------------------------------------------------------*/
	request = ippNewRequest(IPP_OP_GET_PRINTER_ATTRIBUTES);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_MIMETYPE, "document-format", NULL, "image/pwg-raster");
	ippAddStrings(request, IPP_TAG_OPERATION, IPP_TAG_KEYWORD, "requested-attributes", (int)(sizeof(requested_attributes) / sizeof(requested_attributes[0])), NULL, requested_attributes);

	response = cupsDoRequest(http, request, "/ipp/print");
	status = ippGetStatusCode(response);
	if (status == IPP_STATUS_OK)
	{

		//First time through we get the number of char pointers we need to allocate...
		for (attrptr = ippGetFirstAttribute(response); attrptr; attrptr = ippGetNextAttribute(response))
		{
			name = ippGetName(attrptr);

			if (name)
			{
				ippAttributeString(attrptr, value, sizeof(value));
				numStrings++;
			}
		}

		*num = numStrings;

		//now allocate
		//Allocate array of char pointers...
		gpArray = new char* [numStrings];
		if (gpArray)
		{
			i = 0;
			int nameSize = 0;
			int size = 0;
			int bufSize = 0;
			giSize = numStrings;
			//second time through, this time to add the attributes to the allocated array...
			for (attrptr = ippGetFirstAttribute(response); attrptr; attrptr = ippGetNextAttribute(response))
			{
				name = ippGetName(attrptr);

				if (name)
				{
					nameSize = strlen(name) + 2; //for the preceding ':' and (eventually) the null
					ippAttributeString(attrptr, value, sizeof(value));
					size = strlen(value);
					bufSize = sizeof(char) * (size + nameSize);
					gpArray[i] = (char*) new char*[size + nameSize];
					strcpy_s(gpArray[i], bufSize, name);
					strcat_s(gpArray[i], bufSize, "|");
					strcat_s(gpArray[i], bufSize, value);
					i++;
				}
			}
		}
		else
		{
			*num = -1;
			*retCode = IPP_STATUS_CUPS_INVALID;
			return NULL;
		}
	}
	else
	{
		*num = -1;
		*retCode = status;
	}

	httpClose(http);
	ippDelete(request);
	ippDelete(response);
	return gpArray;
}

/*---------------------------------------------------------------------------------------
WrValidateJob

Validates the job against the target printer to ensure the printer supports 
all job attributes requested.


---------------------------------------------------------------------------------------*/

ipp_status_t WrValidateJob(const char* hostname, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	char szUriTag[TAGSIZE] = { 0 };
	int iResult = SUCCESS;
	ipp_status_t status;
	int iPort = 631;

	//Check the hostname.
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if (!hostname)
		return (ipp_status_t)IPP_STATUS_ERROR_URI_SCHEME;


	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t)encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	request = ippNewRequest(IPP_OP_VALIDATE_JOB);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);

	//Now add the job attributes provided by the requestor to the print job request...
	for (int i = 0; i < numAttributes; i++)
	{
		if (add_job_attributeEx(request, jobAttributeArray[i]) != IPP_STATUS_OK)
		{
			iResult &= FAILURE;
			status = IPP_STATUS_ERROR_BAD_REQUEST;
		}
	}

	if (iResult == SUCCESS) 
	{ 
		response = cupsDoRequest(http, request, "/ipp/print"); 
		status = ippGetStatusCode(response); 
		if (response) 
		{ 
			ippDelete(response); 
		} 
	} 
	return status;
}
/*-----------------------------------------------------------------------------------------------
WrCreateJob
Wraps the CreateJob Request

-----------------------------------------------------------------------------------------------*/
ipp_status_t WrCreateJob(const char* hostname, const char* filetoPrint, const char* userName, int* piJobId, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	char szUriTag[TAGSIZE] = { 0 };
	int iResult = SUCCESS;
	ipp_status_t status;
	int iJobId = -1, iPort = 631;

	//Check the hostname.
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if (!hostname)
		return (ipp_status_t)IPP_STATUS_ERROR_URI_SCHEME;

	if (!filetoPrint)
		return (ipp_status_t)IPP_STATUS_ERROR_DOCUMENT_ACCESS;

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t)encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	request = ippNewRequest(IPP_OP_CREATE_JOB);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);

	//Now add the job attributes provided by the requestor to the print job request...
	for (int i = 0; i < numAttributes; i++)
	{
		if (add_job_attributeEx(request, jobAttributeArray[i]) != IPP_STATUS_OK)
		{
			iResult &= FAILURE;
			status = IPP_STATUS_ERROR_BAD_REQUEST;
		}
	}

	if (iResult == SUCCESS)
	{
		response = cupsDoFileRequest(http, request, "/ipp/print", filetoPrint);
		status = ippGetStatusCode(response);
		if (status == IPP_STATUS_OK)
		{
			*piJobId = ippGetInteger(ippFindAttribute(response, "job-id", IPP_TAG_INTEGER), 0);
		}
		else
		{
			*piJobId = -1;
		}

		if (response)
		{
			ippDelete(response);
		}
	}


	httpClose(http);
	if (request)
		ippDelete(request);
	return status;
}
/*-----------------------------------------------------------------------------------------------
WrSendDocument
Wraps the SendDocument Request

-----------------------------------------------------------------------------------------------*/
ipp_status_t WrSendDocument(const char* hostname, const char* filetoPrint, const char* userName, int jobId, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, *response;
	char szUriTag[TAGSIZE] = { 0 };
	int iResult = SUCCESS;
	ipp_status_t status;
	int iPort = 631;

	//Check the hostname.
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if (!hostname)
		return (ipp_status_t)IPP_STATUS_ERROR_URI_SCHEME;
	if (!filetoPrint)
		return (ipp_status_t)IPP_STATUS_ERROR_DOCUMENT_ACCESS;

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t)encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	request = ippNewRequest(IPP_OP_CREATE_JOB);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);

	request = ippNewRequest(IPP_OP_SEND_DOCUMENT);
	status = ippGetStatusCode(request);
	if ((status == IPP_STATUS_OK) || (status == IPP_STATUS_OK_BUT_CANCEL_SUBSCRIPTION))
	{
		ippAddInteger(request, IPP_TAG_OPERATION, IPP_TAG_INTEGER, "job-id", jobId);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_MIMETYPE, "document-format", NULL, getMimeType(filetoPrint));
		//must include the 'last-document' op or this will fail..
		ippAddBoolean(request, IPP_TAG_OPERATION, "last-document", 1);
		response = cupsDoFileRequest(http, request, "/ipp/print", filetoPrint);
		status = ippGetStatusCode(response);

		if (response)
		{
			ippDelete(response);
		}
	}

	httpClose(http);
	if (request)
		ippDelete(request);
	return status;
}
/*---------------------------------------------------------------------------------------
WrCreateJobSendDocument

Uses the Create-Job and Send-Document IPP operation.

Create a print job and send this to the printer. Printer will return a job id via a callback.
We can use this job id later if we want to delete it...
---------------------------------------------------------------------------------------*/
ipp_status_t WrCreateJobSendDocument(const char* hostname, const char* filetoPrint, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, *response;
	char szUriTag[TAGSIZE] = { 0 };
	int iResult = SUCCESS;
	ipp_status_t status;
	int iJobId = -1, iPort = 631;

	//Check the hostname.
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if (!hostname)
		return (ipp_status_t)IPP_STATUS_ERROR_URI_SCHEME;
	if (!filetoPrint)
		return (ipp_status_t)IPP_STATUS_ERROR_DOCUMENT_ACCESS;

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t)encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	request = ippNewRequest(IPP_OP_CREATE_JOB);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
	
	//Now add the job attributes provided by the requestor to the print job request...
	for (int i = 0; i < numAttributes; i++)
	{
		if (add_job_attributeEx(request, jobAttributeArray[i]) != IPP_STATUS_OK)
		{
			iResult &= FAILURE;
			status = IPP_STATUS_ERROR_BAD_REQUEST;
		}
	}

	if (iResult == SUCCESS)
	{
		response = cupsDoFileRequest(http, request, "/ipp/print", filetoPrint);
		status = ippGetStatusCode(response);
		if (status == IPP_STATUS_OK)
		{
			iJobId = ippGetInteger(ippFindAttribute(response, "job-id", IPP_TAG_INTEGER), 0);
		}
		else
		{
			iJobId = -1;
		}
		
		if (response)
		{
			ippDelete(response);
		}
	}

	//Send job id via callback
	if (pjidCallback)
	{
		pjidCallback(iJobId);
	}


	request = ippNewRequest(IPP_OP_SEND_DOCUMENT);
	status = ippGetStatusCode(request);
	if ((status == IPP_STATUS_OK)||(status == IPP_STATUS_OK_BUT_CANCEL_SUBSCRIPTION))
	{
		ippAddInteger(request, IPP_TAG_OPERATION, IPP_TAG_INTEGER, "job-id", iJobId);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_MIMETYPE, "document-format", NULL, getMimeType(filetoPrint));
		//must include the 'last-document' op or this will fail..
		ippAddBoolean(request, IPP_TAG_OPERATION, "last-document", 1);
		response = cupsDoFileRequest(http, request, "/ipp/print", filetoPrint);
		status = ippGetStatusCode(response);

		if (response)
		{
			ippDelete(response);
		}
	}

	httpClose(http);
	if(request)
		ippDelete(request);
	return status;
}

/*---------------------------------------------------------------------------------------
WrPrintJob

Send a document to a printer along with any of the job attributes specified. This function can be
consumed by unmanaged C/C++ applications. This combines the Create-Job and Send-Document requests.

Job Status Attributes

Job objects have two main status attributes: "job-state" and "job-state-reasons".
The "job-state" attribute is a number that describes the general state of the Job:

'3': The Job is queued and pending.
'4': The Job has been held, e.g., for "PIN printing".
'5': The Job is being processed (printed, faxed, etc.)
'6': The Job is stopped (out of paper, etc.)
'7': The Job was canceled by the user.
'8': The Job was aborted by the Printer.
'9': The Job completed successfully.

The "job-state-reasons" attribute is a list of keyword strings that provide details about the Job's state:

'none': Everything is super, nothing to report.
'document-format-error': The Document could not be printed due to a file format error.
'document-unprintable-error': The Document could not be printed for other reasons (too complex, out of memory, etc.)
'job-incoming': The Job is being received from the Client.
'job-password-wait': The Printer is waiting for the user to enter the PIN for the Job.

IPPTOOL_TRANSFER_AUTO,			Chunk for files, length for static
IPPTOOL_TRANSFER_CHUNKED,		Chunk always
IPPTOOL_TRANSFER_LENGTH			Length always

User Name is in format "John Doe"

Printers are identified using Universal Resource Identifiers ("URIs") with the "ipp" or "ipps" scheme. Print Jobs are identified
using the Printer's URI and a Job number that is unique to that Printer.

HTTP Encryption choices
------------------------
HTTP_ENCRYPTION_IF_REQUESTED,		// Encrypt if requested (TLS upgrade)
HTTP_ENCRYPTION_NEVER,				// Never encrypt
HTTP_ENCRYPTION_REQUIRED,			// Encryption is required (TLS upgrade)
HTTP_ENCRYPTION_ALWAYS				// Always encrypt (HTTPS)

Example:
int jobId;
Encryption type (http_encryption_t)

 HTTP_ENCRYPTION_IF_REQUESTED,	// Encrypt if requested (TLS upgrade)		3
 HTTP_ENCRYPTION_NEVER,			// Never encrypt							4
 HTTP_ENCRYPTION_REQUIRED,		// Encryption is required (TLS upgrade)		5
 HTTP_ENCRYPTION_ALWAYS			// Always encrypt (HTTPS)					6


---------------------------------------------------------------------------------------*/
ipp_status_t WrPrintJob(const char* hostname, const char* filetoPrint, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout,  unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	char szUriTag[TAGSIZE] = {0};
	int iResult = SUCCESS;
	ipp_status_t status;
	int iPort = 631;

	//Check the hostname.
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if (!hostname)
		return (ipp_status_t)IPP_STATUS_ERROR_URI_SCHEME;
	if (!filetoPrint)
		return (ipp_status_t)IPP_STATUS_ERROR_DOCUMENT_ACCESS;

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t) encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	//Print-Job operation allows you to create a print Job
	request = ippNewRequest(IPP_OP_PRINT_JOB);

	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_MIMETYPE, "document-format", NULL, getMimeType(filetoPrint));


	//Now add the job attributes provided by the requestor to the print job request...
	for (int i = 0; i < numAttributes; i++)
	{
		if (add_job_attributeEx(request, jobAttributeArray[i]) != IPP_STATUS_OK)
		{
			iResult &= FAILURE;
			status = IPP_STATUS_ERROR_BAD_REQUEST;
		}
	}

	if (iResult == SUCCESS)
	{
		response = cupsDoFileRequest(http, request, "/ipp/print", filetoPrint);
		status = ippGetStatusCode(response);
		if (response)
		{
			ippDelete(response);
		}
	}

	httpClose(http);
	if(request)
		ippDelete(request);
	return status;
}

/*-----------------------------------------------------------------------------
WrCancelJob

Cancel a print job, given a print job ID.

Example: cancelPrintJob("P233005", "John Doe", 35);
-----------------------------------------------------------------------------*/
ipp_status_t WrCancelJob(const char* hostname, const char* userName, int jobId, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	//string printer_uri;
	ipp_status_t status;
	char szUriTag[TAGSIZE] = {0};
	int iPort = 631;

	//Check the hostname..
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, (http_encryption_t)encryption, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	//canx the job
	request = ippNewRequest(IPP_OP_CANCEL_JOB);

	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
	ippAddInteger(request, IPP_TAG_OPERATION, IPP_TAG_INTEGER, "job-id", jobId);
	ippAddBoolean(request, IPP_TAG_OPERATION, "purge-job", 1);

	//In request.c This function sends the IPP request to the specified server, 
	//The request is freed with ippDelete


	response = cupsDoRequest(http, request, "/jobs");
	status = ippGetStatusCode(response);
  
	httpClose(http);
	if(request)
		ippDelete(response);
	if (response)
		ippDelete(response);
	return status;
}

/*-----------------------------------------------------------------------------
WrIdentifyPrinter

Make the printer beep, flash, or display a message for identification.

Returns ipp_status_t 
-----------------------------------------------------------------------------*/
ipp_status_t WrIdentifyPrinter(const char* hostname, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request, * response;
	//string printer_uri;
	char szUriTag[TAGSIZE] = { 0 };
	ipp_status_t status;
	int iPort = 631;

	//Check the hostname..
	if ((!hostname) || (strlen(hostname) > 70))
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		return status;
	}

	if ((encryption == HTTP_ENCRYPTION_ALWAYS) || (encryption == HTTP_ENCRYPTION_REQUIRED))
	{
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
		iPort = 631;
	}
	else
	{
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");
		iPort = 631;
	}

	http = httpConnect(hostname, iPort, NULL, ip_family, HTTP_ENCRYPTION_IF_REQUESTED, 1, timeout, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_SERVICE_UNAVAILABLE;
		return status;
	}

	//Identify Yourself
	request = ippNewRequest(IPP_OP_IDENTIFY_PRINTER);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, "ipp://printer.example.com/ipp/print");

	response = cupsDoRequest(http, request, "/ipp/print");

	status = cupsLastError();

	httpClose(http);
	ippDelete(response);
	return status;
}

/*-----------------------------------------------------------------------------------------------------------------------
WrGetJobs



-----------------------------------------------------------------------------------------------------------------------*/
char** WrGetJobs(const char* hostname, const char* userName, int* retCode, int* numJobs, bool bCompletedJobs, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout, unsigned ip_family, unsigned chunking)
{
	http_t* http;
	ipp_t* request;
	ipp_t* response;
	ipp_status_t status;
	int jobId = 30;
	char szUriTag[150] = { 0 };
	ipp_attribute_t* attrptr;					/* Current attribute */
	bool bVerified = FALSE;
	char szItemBuffer[MAX_PATH * 2] = { 0 };		/* Temp Storage for Job Value Item */
	string sAttribute, sJobValues;
	int jobItemCount = 0;						/* Counter for each job item have numAttribute items for each job */
	int jobsCount = 0;							/* Counter for the total number of jobs returned from the printer */
	int jobAdded = 0;							/* Counter for each job - this is determined by the returned vaue of jobs on the printer */
	int numRetAttributes = 0;


	if (encryption == HTTP_ENCRYPTION_ALWAYS)
		sprintf_s(szUriTag, "%s%s%s", "ipps://", hostname, "/ipp/print");
	else
		sprintf_s(szUriTag, "%s%s%s", "ipp://", hostname, "/ipp/print");


	http = httpConnect(hostname, 631, NULL, AF_UNSPEC, HTTP_ENCRYPTION_IF_REQUESTED, 1, 30000, NULL);
	if (http == NULL)
	{
		ipp_status_t status = IPP_STATUS_ERROR_BAD_REQUEST;
		*retCode = status;
		*numJobs = -1;
		return NULL;
	}

	static const char* const attrs[] =
	{
	  "document-format",
	  "job-id"
	};


	request = ippNewRequest(IPP_OP_GET_JOBS);

	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_URI, "printer-uri", NULL, szUriTag);
	ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_NAME, "requesting-user-name", NULL, userName);
	//Below can only be one of the values in which-jobs-supported attribute returned from the printer.
	if (bCompletedJobs == true)
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_KEYWORD, "which-jobs", NULL, "completed");
	else
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_KEYWORD, "which-jobs", NULL, "not-completed");

	//add the job attributes we are looking for
	for (int i = 0; i < numAttributes; i++)
	{
		sAttribute.assign(jobAttributeArray[i]);
		ippAddString(request, IPP_TAG_OPERATION, IPP_TAG_KEYWORD, "requested-attributes", NULL, sAttribute.c_str());
		sAttribute.clear();
	}

	// Make the request...
	response = cupsDoRequest(http, request, "/ipp/print");

	// For each job info item returned, convert this to a 'ITEM:VALUE' string and add that to each print job.
	// Seperate each job info item with a comma.
	// We will then return an array of char pointers each of which represents a job
	if (ippGetStatusCode(response) == IPP_STATUS_OK)
	{
		if (response != NULL)
		{
			//First, get the number of jobs we need to allocate for..
			attrptr = ippGetFirstAttribute(response);
			while (attrptr != NULL)
			{
				while (attrptr && attrptr->group_tag != IPP_TAG_JOB)
				{
					attrptr = attrptr->next;
				}

				if (!attrptr)
				{
					break;
				}

				//Count this as a JOB if we match the first attribute
				if (!stricmp(attrptr->name, jobAttributeArray[0]))
				{
					jobsCount++;
				}

				attrptr = attrptr->next;
			}

			// Now get the number of attributes in the query that are supported by the printer. Some attributes are RECOMMENDED but not required by IPP standard.
			// Thus even though we ask for them, they might not be returned in the get-print-jobs request. So we will account for only the returned (from printer).
			// First we find a IPP_TAG_JOB attribute group tag, then we search until we find another IPP_TAG_JOB attribute group tag. We will compare the returned
			// job attributes to what we requested.
			attrptr = ippGetFirstAttribute(response);
			while (attrptr && attrptr->group_tag != IPP_TAG_JOB)
			{
				attrptr = attrptr->next;
			}
			do
			{
				//Count the number of returned (supported attributes)
				if (attrptr != NULL)
				{
					if (attrptr->group_tag != IPP_TAG_JOB)
						break;

					//Now see what attributes are returned in this IPP_TAG_JOB. We have to find out the number of attributes the printer
					//supports - some attributes are RECOMMENDED but not required by IPP standard. We only do this 1x.
					{
						if (searchAttributesArray(jobAttributeArray, numAttributes, attrptr->name) == TRUE)
						{
							numRetAttributes++;
						}
					}
					attrptr = attrptr->next;
				}
				else
				{
					break;
				}
			} while (1);

			//Allocate array of char pointers - we need one for each job - each job has multiple values associated with it
			gpJobs = new char* [jobsCount];
			if (!gpJobs)
			{
				*retCode = IPP_STATUS_CUPS_INVALID;
				*numJobs = -1;
				giJobs = 0;
				gpJobs = 0;
				return NULL;
			}

			//Now get the jobs...
			attrptr = ippGetFirstAttribute(response);
			while (attrptr != NULL)
			{
				//Skip leading attributes until we hit a job...

				while (attrptr && attrptr->group_tag != IPP_TAG_JOB)
				{
					attrptr = attrptr->next;
				}

				if (!attrptr)
				{
					break;
				}

				if (attrptr && attrptr->group_tag == IPP_TAG_JOB)
				{
					memset(szItemBuffer, 0, sizeof(szItemBuffer));
					if (attrptr->value_tag == IPP_TAG_INTEGER)
					{
						if (!attrptr->values[0].integer)
							sprintf(szItemBuffer, "%s:%s", attrptr->name, "N/A");
						else
							sprintf(szItemBuffer, "%s:%d", attrptr->name, attrptr->values[0].integer);
						jobItemCount++;
						sJobValues += szItemBuffer;
					}
					else
					{
						if (!attrptr->values[0].string.text)
							sprintf(szItemBuffer, "%s:%s", attrptr->name, "N/A");
						else
							sprintf(szItemBuffer, "%s:%s", attrptr->name, attrptr->values[0].string.text);
						jobItemCount++;
						sJobValues += szItemBuffer;
					}

					//job Values complete - copy to output pointer to char buffers and increment jobAdded for next addition.
					if (jobItemCount == numRetAttributes)
					{
						jobItemCount = 0;
						//OutputDebugStringA(sJobValues.c_str());
						gpJobs[jobAdded] = (char*) new char* [sJobValues.length() + 1];
						strcpy_s(gpJobs[jobAdded], sJobValues.length() + 1, sJobValues.c_str());
						sJobValues.clear();
						jobAdded++;
					}
					else //Add comma as delimeter if we are not yet done with all job values
					{
						sJobValues += ",";
					}
				}
				attrptr = attrptr->next;
			}
		}
	}
	else // ippGetStatusCode was not OK
	{
		*retCode = ippGetStatusCode(response);
		*numJobs = -1;
		giJobs = 0;
		gpJobs = NULL;
	}

	//Give the caller the number of jobs in the allocated buffer returned.
	*numJobs = jobAdded;
	giJobs = jobAdded;

	httpClose(http);
	if (request)
		ippDelete(response);
	if (response)
		ippDelete(response);
	return gpJobs;
}

/*-----------------------------------------------------------------------------
WrGetIppErrorString

Returns an error string when ipp_status_t provided.
-----------------------------------------------------------------------------*/
const char * WrGetIppErrorString(ipp_status_t status)
{
	switch (status)
	{
		case IPP_STATUS_CUPS_INVALID:
			return "Invalid status name for @link ippErrorValue@";
		case IPP_STATUS_OK:
			return "Success";
		case IPP_STATUS_OK_IGNORED_OR_SUBSTITUTED:
			return "successful - ok - ignored - or -substituted - attributes";
		case IPP_STATUS_OK_CONFLICTING:
			return "successful-ok-conflicting-attributes";
		case IPP_STATUS_OK_IGNORED_SUBSCRIPTIONS:
			return "successful-ok-ignored-subscriptions";
		case IPP_STATUS_OK_IGNORED_NOTIFICATIONS :
			return "successful - ok - ignored - notifications @private@";
		case IPP_STATUS_OK_TOO_MANY_EVENTS:
			return "successful - ok - too - many - events";
		case IPP_STATUS_OK_BUT_CANCEL_SUBSCRIPTION:
			return " successful-ok-but-cancel-subscription @private@";
		case IPP_STATUS_OK_EVENTS_COMPLETE:
			return "successful - ok - events - complete";
		case IPP_STATUS_REDIRECTION_OTHER_SITE:
			return "redirection-other-site @private@";
		case  IPP_STATUS_CUPS_SEE_OTHER:
			return "cups-see-other @private@";
		case IPP_STATUS_ERROR_BAD_REQUEST:
			return "client-error-bad-request";
		case IPP_STATUS_ERROR_FORBIDDEN:
			return "client-error-forbidden";
		case IPP_STATUS_ERROR_NOT_AUTHENTICATED:
			return "client-error-not-authenticated";
		case IPP_STATUS_ERROR_NOT_AUTHORIZED:
			return "client-error-not-authorized";
		case IPP_STATUS_ERROR_NOT_POSSIBLE:
			return "client-error-not-possible";
		case IPP_STATUS_ERROR_TIMEOUT:
			return "client-error-timeout";
		case IPP_STATUS_ERROR_NOT_FOUND:
			return "client-error-not-found";
		case IPP_STATUS_ERROR_GONE:	
			return "client - error - gone";
		case IPP_STATUS_ERROR_REQUEST_ENTITY:	
			return "client - error - request - entity - too - large";
		case IPP_STATUS_ERROR_REQUEST_VALUE:	
			return "client-error-request-value-too-long";
		case IPP_STATUS_ERROR_DOCUMENT_FORMAT_NOT_SUPPORTED:  
			return "client-error-document-format-not-supported";
		case IPP_STATUS_ERROR_ATTRIBUTES_OR_VALUES: 
			return "client-error-attributes-or-values-not-supported";
		case IPP_STATUS_ERROR_URI_SCHEME:		
			return "client-error-uri-scheme-not-supported";
		case IPP_STATUS_ERROR_CHARSET:		
			return "client-error-charset-not-supported";
		case IPP_STATUS_ERROR_CONFLICTING:		
			return "client-error-conflicting-attributes";
		case IPP_STATUS_ERROR_COMPRESSION_NOT_SUPPORTED:  
			return "client-error-compression-not-supported";
		case IPP_STATUS_ERROR_COMPRESSION_ERROR:	
			return "client-error-compression-error";
		case IPP_STATUS_ERROR_DOCUMENT_FORMAT_ERROR: 
			return "client-error-document-format-error";
		case IPP_STATUS_ERROR_DOCUMENT_ACCESS:	
			return "client-error-document-access-error";
		case IPP_STATUS_ERROR_ATTRIBUTES_NOT_SETTABLE:
			return "client-error-attributes-not-settable";
		case IPP_STATUS_ERROR_IGNORED_ALL_SUBSCRIPTIONS:
			return "client-error-ignored-all-subscriptions";
		case IPP_STATUS_ERROR_TOO_MANY_SUBSCRIPTIONS:
			return "client-error-too-many-subscriptions";
		case IPP_STATUS_ERROR_IGNORED_ALL_NOTIFICATIONS:
			return "client-error-ignored-all-notifications @private@";
		case IPP_STATUS_ERROR_PRINT_SUPPORT_FILE_NOT_FOUND:
			return "client-error-print-support-file-not-found @private@";
		case IPP_STATUS_ERROR_DOCUMENT_PASSWORD:	
			return "client - error - document - password - error";
		case IPP_STATUS_ERROR_DOCUMENT_PERMISSION:	
			return "client-error-document-permission-error";
		case IPP_STATUS_ERROR_DOCUMENT_SECURITY:	
			return "client-error-document-security-error";
		case IPP_STATUS_ERROR_DOCUMENT_UNPRINTABLE: 
			return "client-error-document-unprintable-error";
		case IPP_STATUS_ERROR_ACCOUNT_INFO_NEEDED:	
			return "client-error-account-info-needed";
		case IPP_STATUS_ERROR_ACCOUNT_CLOSED:	
			return "client-error-account-closed";
		case IPP_STATUS_ERROR_ACCOUNT_LIMIT_REACHED:
			return "client-error-account-limit-reached";
		case IPP_STATUS_ERROR_ACCOUNT_AUTHORIZATION_FAILED:
			return "client-error-account-authorization-failed";
		case IPP_STATUS_ERROR_NOT_FETCHABLE:	
			return "client-error-not-fetchable";
		case IPP_STATUS_ERROR_INTERNAL:	
			return "server-error-internal-error";
		case IPP_STATUS_ERROR_OPERATION_NOT_SUPPORTED:
			return "server-error-operation-not-supported";
		case IPP_STATUS_ERROR_SERVICE_UNAVAILABLE:	
			return "server-error-service-unavailable";
		case IPP_STATUS_ERROR_VERSION_NOT_SUPPORTED:
			return "server-error-version-not-supported";
		case IPP_STATUS_ERROR_DEVICE:		
			return "server-error-device-error";
		case IPP_STATUS_ERROR_TEMPORARY:		
			return "server-error-temporary-error";
		case IPP_STATUS_ERROR_NOT_ACCEPTING_JOBS:	
			return "server-error-not-accepting-jobs";
		case IPP_STATUS_ERROR_BUSY:		
			return "server-error-busy";
		case IPP_STATUS_ERROR_JOB_CANCELED:	
			return "server-error-job-canceled";
		case IPP_STATUS_ERROR_MULTIPLE_JOBS_NOT_SUPPORTED:
			return "server-error-multiple-document-jobs-not-supported";
		case IPP_STATUS_ERROR_PRINTER_IS_DEACTIVATED:
			return "server-error-printer-is-deactivated";
		case IPP_STATUS_ERROR_TOO_MANY_JOBS:	
			return "server-error-too-many-jobs";
		case IPP_STATUS_ERROR_TOO_MANY_DOCUMENTS:	
			return "server-error-too-many-documents";
		case IPP_STATUS_ERROR_CUPS_AUTHENTICATION_CANCELED:
			return "cups-authentication-canceled - Authentication canceled by user";
		case IPP_STATUS_ERROR_CUPS_PKI:		
			return "cups-pki-error - Error negotiating a secure connection";
		case IPP_STATUS_ERROR_CUPS_UPGRADE_REQUIRED:
			return "cups-upgrade-required - TLS upgrade required";
		default:
			return "Unknown error";
	}
	
}

/*-----------------------------------------------------------------------------
add_job_attributeEx

Adds a job attribute to the request. Note: This is incomplete as yet and
only supports a subset of available print job attributes supported.

Supported Attributes:
---------------------
copies
media
sides
output-bin
print-color-mode
print-scaling
page-ranges
print-quality
orientation-requested

If not supported, function returns IPP_STATUS_ERROR_BAD_REQUEST.

Returns ipp_status_t
-----------------------------------------------------------------------------*/
ipp_status_t add_job_attributeEx(ipp_t* request, const char* jobAttributeString)
{
	ipp_tag_e tag;
	int intValue;
	string sName, sValue;
	int lower, upper;
	string strAttributeString = jobAttributeString;
	
	int pos = strAttributeString.find(":");
	if (pos != npos)
	{
		sName = strAttributeString.substr(0, pos);
		sValue = strAttributeString.substr(pos + 1);
	}
	else
		return IPP_STATUS_ERROR_BAD_REQUEST;

	//Below are the job attrbutes currently supported
	if (!_stricmp(sName.c_str(), "copies"))
		tag = IPP_TAG_INTEGER;
	else if (!_stricmp(sName.c_str(), "media"))
		tag = IPP_TAG_KEYWORD;
	else if (!_stricmp(sName.c_str(), "sides"))
		tag = IPP_TAG_KEYWORD;
	else if (!_stricmp(sName.c_str(), "output-bin"))
		tag = IPP_TAG_KEYWORD;
	else if (!_stricmp(sName.c_str(), "print-color-mode"))
		tag = IPP_TAG_KEYWORD;
	else if (!_stricmp(sName.c_str(), "print-scaling"))
		tag = IPP_TAG_KEYWORD;
	else if (!_stricmp(sName.c_str(), "page-ranges"))
		tag = IPP_TAG_RANGE;
	else if (!_stricmp(sName.c_str(), "print-quality"))
		tag = IPP_TAG_ENUM;
	else if (!_stricmp(sName.c_str(), "orientation-requested"))
		tag = IPP_TAG_ENUM;
	else
		return IPP_STATUS_ERROR_BAD_REQUEST;

	switch (tag)
	{
	case IPP_TAG_ENUM:
		try
		{
			intValue = stoi(sValue);
			ippAddInteger(request, IPP_TAG_JOB, IPP_TAG_ENUM, sName.c_str(), intValue);
		}
		catch (exception e)
		{
			return IPP_STATUS_ERROR_BAD_REQUEST;
		}
		break;
	case IPP_TAG_INTEGER:
		try
		{
			intValue = stoi(sValue);
			ippAddInteger(request, IPP_TAG_JOB, IPP_TAG_INTEGER, sName.c_str(), intValue);
		}
		catch (exception e)
		{
			return IPP_STATUS_ERROR_BAD_REQUEST;
		}
		break;
	case IPP_TAG_BOOLEAN:
		if (!_cups_strcasecmp(sValue.c_str(), "true"))
			ippAddBoolean(request, IPP_TAG_ZERO, sName.c_str(), 1);
		else
			ippAddBoolean(request, IPP_TAG_ZERO, sName.c_str(), (char)atoi(sValue.c_str()));
		break;
	case IPP_TAG_KEYWORD:
		ippAddString(request, IPP_TAG_JOB, IPP_TAG_KEYWORD, sName.c_str(), NULL, sValue.c_str());
		break;
	case IPP_TAG_RANGE:
		pos = sValue.find(",");
		if (pos != std::string::npos)
		{
			try
			{
				string sl = sValue.substr(0, pos);
				string su = sValue.substr(pos + 1);
				lower = stoi(sl);
				upper = stoi(su);
				ippAddRange(request, IPP_TAG_JOB, sName.c_str(), lower, upper);
			}
			catch (exception e)
			{
				return IPP_STATUS_ERROR_BAD_REQUEST;
			}
		}
		else
			return IPP_STATUS_ERROR_BAD_REQUEST;
		break;
	default:
		return IPP_STATUS_ERROR_BAD_REQUEST;
	}

	return IPP_STATUS_OK;
}

/*-----------------------------------------------------------------------------
GetMimeType

IN: const char pointer to file to print, including extension.

Returns const char pointer to Mime Type
-----------------------------------------------------------------------------*/

const char* getMimeType(const char* filename)
{
	char* ext;

	if ((ext = (char*)strrchr(filename, '.')) != NULL)
	{
		// Guess the MIME media type based on the extension...
		if (!_strnicmp(ext, ".gif", 5))
			return "image/gif";
		else if (!_strnicmp(ext, ".htm", 5) || !_strnicmp(ext, ".htm.gz", 5) || !_strnicmp(ext, ".html", 5) || !_strnicmp(ext, ".html.gz", 5))
			return "text/html";
		else if (!_strnicmp(ext, ".jpg", 5) || !_strnicmp(ext, ".jpeg", 5))
			return "image/jpeg";
		else if (!_strnicmp(ext, ".pcl", 5) || !_strnicmp(ext, ".pcl.gz", 5))
			return "application/vnd.hp-PCL";
		else if (!_strnicmp(ext, ".pdf", 5))
			return "application/pdf";
		else if (!_strnicmp(ext, ".png", 5))
			return "image/png";
		else if (!_strnicmp(ext, ".ps", 5) || !_strnicmp(ext, ".ps.gz", 5))
			return "application/postscript";
		else if (!_strnicmp(ext, ".pwg", 5) || !_strnicmp(ext, ".pwg.gz", 5) || !_strnicmp(ext, ".ras", 5) || !_strnicmp(ext, ".ras.gz", 5))
			return "image/pwg-raster";
		else if (!_strnicmp(ext, ".pxl", 5) || !_strnicmp(ext, ".pxl.gz", 5))
			return "application/vnd.hp-PCLXL";
		else if (!_strnicmp(ext, ".tif", 5) || !_strnicmp(ext, ".tiff", 5))
			return "image/tiff";
		else if (!_strnicmp(ext, ".txt", 5) || !_strnicmp(ext, ".txt.gz", 5) || !_strnicmp(ext, ".csv", 5))
			return "text/plain";
		else if (!_strnicmp(ext, ".urf", 5) || !_strnicmp(ext, ".urf.gz", 5))
			return "image/urf";
		else if (!_strnicmp(ext, ".xps", 5))
			return "application/openxps";
		else
			return "application/octet-stream";
	}
	else
	{
		// Use the "auto-type" MIME media type...
		return "application/octet-stream";
	}
}
/*---------------------------------------------------------------------------------------
searchAttributesArray

---------------------------------------------------------------------------------------*/
bool searchAttributesArray(const char** attr_vals, int num_entries, char* attribute_name)
{
	for (int i = 0; i < num_entries; i++)
	{
		if (!stricmp((const char*)attr_vals[i], attribute_name))
		{
			return true;
		}
	}
	return false;
}

/*---------------------------------------------------------------------------------------
SetCallback

Sets the callback function for case IPP.

---------------------------------------------------------------------------------------*/
void setCallback(cups_password_cb_t callback)
{
	cupsSetPasswordCB(callback, NULL);  
}



/*---------------------------------------------------------------------------------------
setJobIdCallback

Sets the job Id callback function. This needs to be done only once for calling application
to use the job callback mechanism.

---------------------------------------------------------------------------------------*/
void setJobIdCallback(JobIdCallback callback)
{
	pjidCallback = callback;
}

/*---------------------------------------------------------------------------------------
FindNullIndexInBuffer

 Find a null char in a given buffer. Caller supplies buffer and size of buffer.
 If the returned value is the size of the buffer, a null was not found. 
---------------------------------------------------------------------------------------*/
int findNullIndexInBuffer(char* buffer, int size)
{
	char* ptr = buffer;
	int loc = 0;
	for (int i = 1; i <= size; i++)
	{
		ptr++;
		if (*ptr == '\0')
		{
			//add 1 for the null...
			loc = i + 1;
			return loc;
		}
	}
	return size;
}

