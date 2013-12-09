#pragma once
#include "TradeLink.h"
#include "EWrapper.h"
#include "eclientsocket.h"
#include "TLOrder.h"
#include <vector>
#include "orderstate.h"
#include "Contract.h"
#include "TLServer_IP.h"
#include "BarRequest.h"
#include <atlstr.h>
using namespace std;

namespace TradeLibFast
{

	class SymOverride 
	{
	public:
		SymOverride(void);
		~SymOverride(void);
		SymOverride::SymOverride(CString line);
		int id;
		CString TLSymbol;
		CString IBSymbol;
		CString IBLocalSymbol;
		CString Currency;
		CString Exchange;
		CString Account;
		CString Multipler;
		CString Expiry;
		double Strike;
		CString SecurityType;
		CString TIF;
		CString Right;
		bool outsideRTH;
		bool transmit;
		long ContractId;
		

		bool isValid();
		bool isIndexed();
		bool isConIdAvail();
		static const int REQUIRED_RECORDS = 15;
		static const int R_TLSYM = 0;
		static const int R_IBSYM = 1;
		static const int R_IBSYMLOCAL = 2;
		static const int R_CUR = 3;
		static const int R_EX = 4;
		static const int R_ACCT = 5;
		static const int R_MULT = 6;
		static const int R_EXPIRY = 7;
		static const int R_SECTYPE = 8;
		static const int R_OUTSIDERTH = 9;
		static const int R_XMIT = 10;
		static const int R_TIF = 11;
		static const int R_RIGHT = 12;
		static const int R_STRIKE = 13;
		static const int R_CONID = 14;

		CString ToString();

	private:
		CString raw;

	
	};

}