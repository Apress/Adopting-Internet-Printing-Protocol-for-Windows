
#include "JobAttributes.h"


JobAttributes::JobAttributes()
{
	m_lJobAttributes.clear();
}

int JobAttributes::AddToList(JobAttributeNode n)
{
	bool bInList = false;
	for (list<JobAttributeNode>::iterator it = m_lJobAttributes.begin(); it != m_lJobAttributes.end(); it++)
	{
		if (!stricmp(it->GetJobAttribute(), n.GetJobAttribute()))
		{
			bInList = true;
			break;
		}
	}
	if (bInList == false)
	{
		m_lJobAttributes.push_back(n);
		return m_lJobAttributes.size();
	}
	else
		return -1;
}

void JobAttributes::Clear()
{
	if (!m_lJobAttributes.empty())
	{
		for (list<JobAttributeNode>::iterator it = m_lJobAttributes.begin(); it != m_lJobAttributes.end(); it++)
		{
			m_lJobAttributes.erase(it);
		}
	}
}