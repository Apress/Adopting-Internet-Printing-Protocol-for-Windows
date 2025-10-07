#pragma once
/*---------------------------------------------------------------------------------------------------
JobAttributeNode

This class represents an IPP job attribute. Each IPP job attribute should be set in accordance to 
RFC 8011 standards.

---------------------------------------------------------------------------------------------------*/
//#include <pch.h>
#include <string>	

using namespace std;


class JobAttributeNode
{
public:
	JobAttributeNode(string ja, string jav) { job_attribute = ja; job_attribute_value = jav; }
	const char* GetJobAttribute() { return job_attribute.c_str(); }
	const char* GetJobAttributeValue() { return job_attribute_value.c_str(); }
protected:
	string job_attribute;
	string job_attribute_value;
};