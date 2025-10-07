#pragma once
/*---------------------------------------------------------------------------------------------------
WinIpp.h


---------------------------------------------------------------------------------------------------*/

#ifdef WINIPP_EXPORTS
#define WINIPP_API __declspec(dllexport)
#else
#define WINIPP_API __declspec(dllimport)
#endif

#include <vcnet/cups-private.h>
#include <cups/raster-testpage.h>
#include <regex.h>
#include <sys/stat.h>
#include <string>
#include <list>


using namespace std;

const int SUCCESS = 1;
const int FAILURE = 0; 
const int npos = -1;
const int TAGSIZE = 150;			//size of tag array


typedef enum ipptool_transfer_e		/**** How to send request data ****/
{
	IPPTOOL_TRANSFER_AUTO,			/* Chunk for files, length for static */
	IPPTOOL_TRANSFER_CHUNKED,		/* Chunk always */
	IPPTOOL_TRANSFER_LENGTH			/* Length always */
} ipptool_transfer_t;

//global pointers on heap to array of strings or to callback 
char** gpArray = NULL;
int giSize;
char** gpJobs = NULL;
int giJobs;

//callback pointers
typedef void(*JobIdCallback)(int iJobTicket);
JobIdCallback pjidCallback = NULL;



//----Functions - All can be consumed by C/C++ or managed code----------
extern "C" WINIPP_API char** WrGetPrinterAttributes(const char* hostname, const char* userName, int* num, int* retCode, unsigned encryption = HTTP_ENCRYPTION_REQUIRED, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API ipp_status_t WrCancelJob(const char* hostname, const char* userName, int jobId, unsigned encryption = HTTP_ENCRYPTION_REQUIRED, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API ipp_status_t WrIdentifyPrinter(const char* hostname, unsigned encryption = HTTP_ENCRYPTION_REQUIRED, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API ipp_status_t WrSendDocument(const char* hostname, const char* filetoPrint, const char* userName, int jobId, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API ipp_status_t WrCreateJob(const char* hostname, const char* filetoPrint, const char* userName, int* piJobId, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API ipp_status_t WrCreateJobSendDocument(const char* hostname, const char* filetoPrint, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption = HTTP_ENCRYPTION_REQUIRED, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API const char* WrGetIppErrorString(ipp_status_t);
extern "C" WINIPP_API ipp_status_t WrValidateJob(const char* hostname, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API char** WrGetJobs(const char* hostname, const char* userName, int* retCode, int* numJobs, bool bCompletedJobs, const char** jobAttributeArray, int numAttributes, unsigned encryption, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API void setCallback(cups_password_cb_t callback);
extern "C" WINIPP_API ipp_status_t WrPrintJob(const char* hostname, const char* filetoPrint, const char* userName, const char** jobAttributeArray, int numAttributes, unsigned encryption = HTTP_ENCRYPTION_REQUIRED, unsigned timeout = 3000, unsigned ip_family = AF_UNSPEC, unsigned chunking = IPPTOOL_TRANSFER_LENGTH);
extern "C" WINIPP_API void setCallback(cups_password_cb_t callback);
extern "C" WINIPP_API void setJobIdCallback(JobIdCallback callback);


//-----Below are specifically for freeing memory allocated in the dll --------
extern "C" WINIPP_API void freePrinterAttributesArrayMemory();
extern "C" WINIPP_API void freePrinterJobsMemory();

// Non-exported helpers
const char* getMimeType(const char* filename);
//ipp_status_t add_job_attribute(ipp_t* request, const char* name, const char* value);
ipp_status_t add_job_attributeEx(ipp_t* request, const char* jobAttributeString);
int findNullIndexInBuffer(char* buffer, int size);
bool searchAttributesArray(const char** attr_vals, int num_entries, char* attribute_name);



