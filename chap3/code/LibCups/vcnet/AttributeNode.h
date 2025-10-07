#pragma once
/*---------------------------------------------------------------------------------------------------
AttributeNode

This class represents an IPP printer attribute. Each IPP printer attribute can have multiple
values, and thus we store these in a list of values.

---------------------------------------------------------------------------------------------------*/
#include <pch.h>
#include <string>	
#include <list>

using namespace std;


class AttributeNode
{
public: 
	AttributeNode(const char* attribute);
	int AddToList(char* properties);
	list<string> GetAttributeList() { return m_lValues; }
	const char* GetAttributeString() { return m_sAttribute.c_str(); }

protected:
	string m_sAttribute;
	list<string> m_lValues;
};