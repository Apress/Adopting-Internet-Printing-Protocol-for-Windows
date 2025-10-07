#pragma once
#include "pch.h"
#include <string>	
#include <list>
#include "JobAttributeNode.h"

using namespace std;


class JobAttributes
{
public:
	JobAttributes();
	int AddToList(JobAttributeNode node);
	void Clear();
	int GetNumberOfJobAttributes() { return m_lJobAttributes.size(); }
	list<JobAttributeNode> GetValues() { return m_lJobAttributes; }

protected:
	list<JobAttributeNode> m_lJobAttributes;
};