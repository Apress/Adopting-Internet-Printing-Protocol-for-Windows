#pragma once
#include "pch.h"
#include "JobAttributeNode.h"
#include "JobAttributes.h"
#include "WinIpp.h"
#include <string>
#include <list>
#include <iostream>
#include <fstream>
#include <sstream>

const int CL_UNDEFINED = -1;
const int CL_SUCCESS = 0;
const int PRINTER = 1;
const int DOCUMENT = 2;
const int JOB = 4;
const int ATTRIBUTES = 8;
const int CANCEL = 16;
const int IDENTIFY = 32;
const int GETJOBS = 64;
const int VALIDATEJOB = 128;
const int CREATEJOB_SENDDOCUMENT= 256;


using namespace std;

void print_attributes_array(string);
int ParseCommandLine(char* [], int);
void HelpMe();
void AddJobAttribute(string);
char** MakeJobAttributesArray();
void FreeJobAttributesArray();
void GetEnvironmentData();
void read_job_attributes(const char**, int);
bool ReadJobAttributesFile(const char*);
char* password_cb(const char* prompt, http_t* http, const char* method, const char* resource, void* user_data);