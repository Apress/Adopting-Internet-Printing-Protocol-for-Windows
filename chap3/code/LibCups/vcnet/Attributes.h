#pragma once
#include <pch.h>
#include <string>	
#include <list>
#include "AttributeNode.h"

using namespace std;


class Attributes
{
public:
	Attributes(const char* printer_uri);
	int AddToList(AttributeNode node);
	void Clear();
	int GetNumberOfAttributes() { return m_lAttributes.size(); }
	const char* GetUri() { return uri.c_str(); }
	list<AttributeNode> GetValues() { return m_lAttributes; }
	const char* GetAttributeStringByIndex(int i);

protected:
	string uri;
	list<AttributeNode> m_lAttributes;
};
