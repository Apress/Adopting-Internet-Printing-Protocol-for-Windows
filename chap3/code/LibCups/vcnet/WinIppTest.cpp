//WinIppTest.cpp 
//

#include "WinIppTest.h"

static string sPrinter, sDocument, sJobId, sUserName;
unsigned CLARGS = 0;
int retVal, iJobId, iNum=0;
JobAttributes _jobAttributes;
bool bJobAttributesIncluded = false;
char** szArray;

int main(int argc, char* argv[])
{
    
    if (ParseCommandLine(argv, argc) == CL_UNDEFINED)
    {
        HelpMe();
        std::cout << L"****Command line syntax error****" << endl;
        return CL_UNDEFINED;
    }

    GetEnvironmentData();

	//Set the password callback if any IPP operations require authentication.
	//setCallback((cups_password_cb_t)password_cb);


    if (CLARGS == (PRINTER | ATTRIBUTES))
    {
        //Show attributes of select printer
       szArray = WrGetPrinterAttributes(sPrinter.c_str(), sUserName.c_str(), &iNum, &retVal, HTTP_ENCRYPTION_REQUIRED);
       if (retVal == ERROR_SUCCESS)
       {
           if (szArray != NULL)
           {
               for (int i = 0; i < iNum; i++)
               {
                   std::cout << szArray[i] << std::endl;
               }
           }
       }
       else
       {
           cout << "Get Printer Attributes for "<<sPrinter.c_str() << " failed, reason: "<<retVal << endl;
       }
       freePrinterAttributesArrayMemory();
    }
	else if (CLARGS == (PRINTER | VALIDATEJOB))
	{
		bJobAttributesIncluded = ReadJobAttributesFile("C:\\Temp\\job_attributes.txt");
		if (bJobAttributesIncluded == true)
		{
			szArray = MakeJobAttributesArray();
			cout << "Job attributes requested:" << endl;
			read_job_attributes((const char**)szArray, iNum);
			cout << endl;
		}
		else
		{
			cout << "No job attributes included" << endl;
		}

		retVal = WrValidateJob(sPrinter.c_str(), sUserName.c_str(), (const char**)szArray, iNum, HTTP_ENCRYPTION_IF_REQUESTED);
		if (retVal == ERROR_SUCCESS)
		{
			cout << "Successfully validated job on printer: "<< sPrinter.c_str() <<endl;
		}
		else
		{
			cout << "Error validating print job, return code: " << retVal << endl;
			cout << WrGetIppErrorString((ipp_status_t)retVal) << endl;
		}
		if (bJobAttributesIncluded == true)
		{
			FreeJobAttributesArray();
		}

	}
    else if (CLARGS == (PRINTER | DOCUMENT))
    {
        //Print a document
        //If you supply job attributes to the print job, be sure to free memory
        //using FreeJobAttributesArray. 
        bJobAttributesIncluded = ReadJobAttributesFile("C:\\Temp\\job_attributes.txt");
        if(bJobAttributesIncluded == true)
        {
            szArray=MakeJobAttributesArray();
            cout << "Job attributes requested:" << endl;
            read_job_attributes((const char**)szArray, iNum);
            cout << endl;
        }
        else
        {
            cout << "No job attributes included" << endl;
        }
        
        retVal = WrPrintJob(sPrinter.c_str(), sDocument.c_str(), sUserName.c_str(), (const char**)szArray, iNum, HTTP_ENCRYPTION_REQUIRED);
         if (retVal == ERROR_SUCCESS)
        {
			cout << "The job was successfully printed" << endl;
        }
        else
        {
            cout << "Error submitting print job, return code: " << retVal << endl;
            cout << WrGetIppErrorString((ipp_status_t)retVal) << endl;
        }
        if (bJobAttributesIncluded == true)
        {
            FreeJobAttributesArray();
        }
    }
	else if (CLARGS == (PRINTER | CREATEJOB_SENDDOCUMENT))
	{
		iJobId = -1;
		char input;
		bJobAttributesIncluded = ReadJobAttributesFile("C:\\Temp\\job_attributes.txt");
		if (bJobAttributesIncluded == true)
		{
			szArray = MakeJobAttributesArray();
			cout << "Job attributes requested:" << endl;
			read_job_attributes((const char**)szArray, iNum);
			cout << endl;
		}
		else
		{
			cout << "No job attributes included" << endl;
		}

		retVal = WrCreateJob(sPrinter.c_str(), sDocument.c_str(), sUserName.c_str(), &iJobId,(const char**)szArray, iNum, HTTP_ENCRYPTION_IF_REQUESTED);

		if (retVal == ERROR_SUCCESS)
		{
			cout << "The job id is: " << iJobId << endl;

			std::cout << "Please enter Y to continue and print or N to not print the job: "; 
			std::cin >> input; 
			input = toupper(input); // Convert input to uppercase for consistency 
			if (input == 'Y')
			{
				retVal = WrSendDocument(sPrinter.c_str(), sDocument.c_str(), sUserName.c_str(), iJobId, (const char**)szArray, iNum, HTTP_ENCRYPTION_IF_REQUESTED);
				if (retVal == ERROR_SUCCESS)
				{
					cout << "The job was successfully printed on: " << sPrinter << endl;
				}
			}
			else
			{
				cout << "User chose not to print job id: "<<iJobId << " on:" << sPrinter << endl;
				cout << "You can cancel the job by ID to remove it from printer memory later.." << endl;
			}
		}
		else
		{
			cout << "Error submitting print job, return code: " << retVal << endl;
			cout << WrGetIppErrorString((ipp_status_t)retVal) << endl;
		}

		if (bJobAttributesIncluded == true)
		{
			FreeJobAttributesArray();
		}
	}
    else if (CLARGS == (PRINTER | JOB))
    {
        //Cancel a job
        retVal = WrCancelJob(sPrinter.c_str(), sUserName.c_str(), stoi(sJobId));
        if (retVal == ERROR_SUCCESS)
        {
            cout << "Job: " << iJobId << " was cancelled" << endl;
        }
        else
        {
            cout << "Error submitting print job cancel request, return code: " << retVal << endl;
        }
    }
    else if (CLARGS == (PRINTER | IDENTIFY))
    {
        //Identify yourself
        retVal = WrIdentifyPrinter(sPrinter.c_str());
        if (retVal == ERROR_SUCCESS)
        {
            cout << "Printer will identify itself" << endl;
        }
        else
        {
            cout << "Error submitting printer identify request, return code: " << retVal << endl;
        }
    }

    else if (CLARGS == (PRINTER | GETJOBS))
    {
        // Look at the number of existing jobs on a printer. Always check the returned number
        // of jobs first. If that is <= 0, the returned buffer will be NULL. If that is > 0, then
        // you can look through the returned array of char pointers. You must release this memory
        // when done with the call with freePrinterJobsMemory..
        int numJobs, retCode;
        const char* szJobAttributes[] = { "job-id", "job-name", "job-state", "job-originating-user-name", "time-at-completed", "job-k-octets" };
        szArray = WrGetJobs(sPrinter.c_str(), sUserName.c_str(), &retCode, &numJobs, true, szJobAttributes, 6, HTTP_ENCRYPTION_IF_REQUESTED);
        if ((szArray != NULL) && (numJobs != -1))
        {
            for (int i = 0; i < numJobs; i++)
            {
                cout << szArray[i] << endl;
            }
            //free memory
            freePrinterJobsMemory();
        }
        else
        {
            cout << "getAllJobsOnPrinter for " << sPrinter.c_str() << " failed with error: " << retCode << endl;
        }
    }

    std::cout << "Fini" << endl;
}

void read_job_attributes(const char** jobAttrArray, int num)
{
    if (jobAttrArray != NULL)
    {
        for (int i = 0; i < num; i++)
        {
            std::cout << jobAttrArray[i] << std::endl;
        }
    }
}

int ParseCommandLine(char* szCommandLine[], int numArgs)
{
    char* pos = NULL;
	if (!szCommandLine)
		return CL_UNDEFINED;

    for (int i = 0; i < numArgs; i++)
    {
		//get the target printer name
        if (strstr(szCommandLine[i], "/p="))
        {
            if (pos=strstr(szCommandLine[i], "="))
            {
                sPrinter.assign(pos + 1);
                CLARGS += PRINTER;
            }
            else
            {
                return CL_UNDEFINED;
            }
        }
		// Get the document
        else if (strstr(szCommandLine[i], "/b="))
        {
            if (pos = strstr(szCommandLine[i], "="))
            {
                sDocument.assign(pos + 1);
                CLARGS += CREATEJOB_SENDDOCUMENT;
            }
            else
            {
                return CL_UNDEFINED;
            }
        }
		// Get the document
		else if (strstr(szCommandLine[i], "/d="))
		{
			if (pos = strstr(szCommandLine[i], "="))
			{
				sDocument.assign(pos + 1);
				CLARGS += DOCUMENT;
			}
			else
			{
				return CL_UNDEFINED;
			}
		}
		//cancel-job request
        else if (!stricmp(szCommandLine[i], "/c="))
        {
            if (pos = strstr(szCommandLine[i], "="))
            {
                sJobId.assign(pos + 1);
                CLARGS += JOB;
            }
            else
            {
                return CL_UNDEFINED;
            }
        }
		//get-printer-attributes request
        else if (!stricmp(szCommandLine[i], "/a"))
        {
            CLARGS += ATTRIBUTES;
        }
		//identify request
        else if (!stricmp(szCommandLine[i], "/i"))
        {
            CLARGS += IDENTIFY;
        }
		//get-jobs request
        else if (!stricmp(szCommandLine[i], "/j"))
        {
            CLARGS += GETJOBS;
        }
		//validate-job request
		else if (!stricmp(szCommandLine[i], "/v"))
		{
			CLARGS += VALIDATEJOB;
		}
    }

    if ((CLARGS == (PRINTER | DOCUMENT)) || (CLARGS == (PRINTER | ATTRIBUTES)) || (CLARGS == (PRINTER | JOB)) || (CLARGS == (PRINTER | GETJOBS)) 
		|| (CLARGS == (PRINTER | VALIDATEJOB)) || (CLARGS == (PRINTER | CREATEJOB_SENDDOCUMENT)) ||(CLARGS == (PRINTER | IDENTIFY)))
    {
        return CL_SUCCESS;
    }
    else
    {
        return CL_UNDEFINED;
    }
}

void HelpMe()
{
    cout << "WinIppTest" << endl;
    cout << "IPP test wrapper utility for OpenPrinting Windows IPP CUPS port" << endl;
    cout << "This is a 64 bit OS application only" << endl;
    cout << "This utility is designed for Windows 10 or greater only." << endl;
	cout << endl;
    cout << "Useage: WinIppTest /p=printer_url /d=pdf_to_print" << endl;
    cout << "Prints document on printer. Note: Only pdf/txt files supported." << endl;
    cout << endl;
	cout << "Useage: WinIppTest /p=printer_url /b=pdf_to_print" << endl;
	cout << "Creates a Job, and retrieves the job Id. Finally sends document to printer." << endl;
	cout << "Note: Only pdf / txt files supported." << endl;
	cout << endl;
    cout << "Useage WinIppTest /p=printer_url /a" << endl;
    cout << "Retrieves attributes from printer." << endl;
    cout << "" << endl;
    cout << "Useage WinIppTest /p=printer_url /c=job_id" << endl;
    cout << "Cancels print on printer using job id returned from previous print request(s)" << endl;
    cout << endl;
    cout << "Useage WinIppTest /p=printer_url /i" << endl;
    cout << "Request printer identify itself with beep, flash, or message display" << endl;
    cout << endl;
    cout << "Useage WinIppTest /p=printer_url /j" << endl;
    cout << "Request printer identify all existing jobs" << endl;
    cout << endl;
	cout << "Useage WinIppTest /p=printer_url /v" << endl;
	cout << "Validate print job attributes on printer" << endl;
	cout << endl;
	cout << "Note: Use C:\\Temp\\job_attributes.txt file as the file of print" << endl;
	cout << "job attributes requested when submitting jobs" << endl;
    cout << "IPP on Windows" << endl;
}

void GetEnvironmentData()
{
    char szBuffer[MAX_PATH] = { 0 };
    if (!GetEnvironmentVariableA((const char*)"UserName", szBuffer, sizeof(szBuffer)))
    {
        sUserName.assign("Unknown");
    }
    else
    {
        sUserName.assign(szBuffer);
    }
}

void AddJobAttribute(string jastring)
{
    int pos = -1;
    if ((pos = jastring.find(":")) != std::string::npos)
    {
        string attribute = jastring.substr(0, pos);
        string value = jastring.substr(pos + 1);
        JobAttributeNode node(attribute, value);
        _jobAttributes.AddToList(node);
    }
}
/// <summary>
/// MakeJobAttributesArray()
/// 
/// This function reads the JobAttributes object and creates an array
/// of job attributes strings. 
/// </summary>
/// <returns></returns>
char** MakeJobAttributesArray()
{
    iNum = _jobAttributes.GetNumberOfJobAttributes();
    int totalSize = 0;
    szArray = new char* [iNum];
    int i = 0;

    if (!szArray)
    {
        return NULL;
    }

    list<JobAttributeNode> nodes = _jobAttributes.GetValues();

    for (list<JobAttributeNode>::iterator it = nodes.begin(); it != nodes.end(); it++)
    {
        totalSize = 0;
        string attr = it->GetJobAttribute();
        string val = it->GetJobAttributeValue();
        totalSize += attr.size();
        totalSize += val.size();
        totalSize += 1; //for seperator (:)
        szArray[i] = (char*) new char* [totalSize];
        if (!szArray[i])
            return NULL;
        strcpy(szArray[i], attr.c_str());
        strcat(szArray[i], ":");
        strcat(szArray[i], val.c_str());
        i++;
    }
    return szArray;
}


void FreeJobAttributesArray()
{
    //Free Memory
    if (szArray != NULL)
    {
        //delete each array element char pointer...
        for (int i = 0; i < iNum; i++)
        {
            delete szArray[i];
        }
        //delete the array pointer
        delete[] szArray;
        szArray = NULL;
    }
}

/// <summary>
/// ReadJobAttributesFile
/// 
/// This function opens a file on disk and reads print job attributes
/// from the file to the JobAttributes object on a line by line basis.
/// </summary>
/// <param name="filepath"></param>
/// <returns></returns>
bool ReadJobAttributesFile(const char* filepath)
{
   ifstream attrfile;
   string line;
   attrfile.open(filepath, ios::in);
   if (!attrfile.is_open())
   {
       return false;
   }
   else
   {
       while (getline(attrfile, line))
       {
           if (line.size() > 0)
           {
               AddJobAttribute(line);
           }
       }
   }
   attrfile._close();
   return true;
}

/// <summary>
/// password_cb
/// 
/// The password callback function that gets called.
/// </summary>
/// <param name="prompt"></param>
/// <param name="http"></param>
/// <param name="method"></param>
/// <param name="resource"></param>
/// <param name="user_data"></param>
/// <returns></returns>
char* password_cb(const char* prompt, http_t* http, const char* method, const char* resource, void* user_data)
{
	static char password[100];
	printf("%s", prompt);
	fgets(password, sizeof(password), stdin);
	password[strcspn(password, "\n")] = '\0';
	return password;
}




