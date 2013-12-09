#pragma once

namespace TradeLibFast
{
	class AFX_EXT_CLASS TLTick
	{
	public:
		const static int MAX_SYM_LENGTH = 28;
		const static int MAX_EX_LENGTH  =  8;
		TLTick(void);
		~TLTick(void);
		TLTick(const TLTick& p);
		TLTick & operator=(const TLTick &rhs);
		char sym[MAX_SYM_LENGTH]; 
		int symid;
		int date;
		int time;
		int size;
		int depth;
		double trade;
		double bid;
		double ask;
		int bs;
		int os;
		//CString be;
		//CString oe;
		//CString ex;
		char be[MAX_EX_LENGTH];
		char oe[MAX_EX_LENGTH]; 
		char ex[MAX_EX_LENGTH];
		bool isValid();
		bool isTrade();
		bool hasBid();
		bool hasAsk();
		CString Serialize(void);
		static TLTick Deserialize(CString message);
	};

	enum TickField
    { // tick message fields from TL server
        ksymbol = 0,
        kdate,
        ktime,
        KUNUSED,
        ktrade,
        ktsize,
        ktex,
        kbid,
        kask,
        kbidsize,
        kasksize,
        kbidex,
        kaskex,
		ktdepth,
    };

}