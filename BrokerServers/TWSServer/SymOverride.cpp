#include "stdafx.h"
#include "SymOverride.h"
#include "TradeLink.h"
#include "Util.h"
#include <fstream>
#include <vector>
using namespace std;

namespace TradeLibFast
{

	SymOverride::SymOverride(CString setupline)
	{
		// prep record
		vector<CString> r;
		// split
		gsplit(setupline,CString(","),r);
		// test for records
		if (r.size()!=REQUIRED_RECORDS)
			return;
		raw = setupline;
		// assign
		TLSymbol = r[R_TLSYM];
		IBSymbol = r[R_IBSYM];
		Exchange = r[R_EX];
		IBLocalSymbol = r[R_IBSYMLOCAL];
		Currency = r[R_CUR];
		Account = r[R_ACCT];
		Multipler = r[R_MULT];
		Expiry = r[R_EXPIRY];
		SecurityType = r[R_SECTYPE];
		outsideRTH = atoi(r[R_OUTSIDERTH])==1;
		transmit = atoi(r[R_XMIT])==1;
		TIF = r[R_TIF];
		Strike = atof(r[R_STRIKE]);
		ContractId = atol(r[R_CONID]);

	}

	bool SymOverride::isConIdAvail()
	{
		return ContractId>0;
	}

	bool SymOverride::isValid()
	{
		return (TLSymbol!="");
	}

	bool SymOverride::isIndexed()
	{
		return id>=0;
	}

	CString SymOverride::ToString()
	{
		return raw;
	}

	SymOverride::SymOverride(void)
	{
		raw = CString("");
		id = -1;
		ContractId = 0;
		TLSymbol = CString("");
		transmit = false;
	}

	SymOverride::~SymOverride(void)
	{
	}


}