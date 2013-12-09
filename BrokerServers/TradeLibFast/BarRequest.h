
#pragma once

namespace TradeLibFast 
{
	class AFX_EXT_CLASS BarRequest
	{
	public:
		BarRequest();
		~BarRequest();
		CString Symbol;
		int Interval;
		int StartDate;
		int StartTime;
		int EndDate;
		int EndTime;
		long ID;
		int CustomInterval;
		int BarsBackExplicit;
		CString Tag;
		CString Client;
		static BarRequest Deserialize(CString msg);
		bool isValid();
	};

	enum BarRequestFields
	{
		brsym,
		brint,
		brsd,
		brst,
		bred,
		bret,
		brid,
		brci,
		brclient,
		bbexplict,
		
	};
}