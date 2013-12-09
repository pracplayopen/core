#include "StdAfx.h"
#include "TLTick.h"
#include "Util.h"

namespace TradeLibFast
{
	TLTick::TLTick(void)
	{
		symid = -1;
		memset(sym, '\0', sizeof(sym)); //  = "";
		size = 0;
		bs = 0;
		os = 0;
		trade = 0;
		bid = 0;
		ask = 0;
		/*ex = "";
		be = "";
		oe = "";*/
		memset(ex, '\0', sizeof(ex)); //  = "";
		memset(be, '\0', sizeof(be)); //  = "";
		memset(oe, '\0', sizeof(oe)); //  = "";
		date = 0;
		time = 0;
		depth = 0;
	}

	TLTick::TLTick( const TLTick& p )
	{
		// use the operator= overload
		*this = p;
	}

	bool TLTick::isTrade()
	{
		return (sym[0]!='\0') && (size*trade!=0);
	}
	bool TLTick::hasAsk() { return (sym[0]!='\0') && (ask*os!=0); }
	bool TLTick::hasBid() { return (sym[0]!='\0') && (bid*bs!=0); }
	bool TLTick::isValid()
	{
		return (sym[0]!='\0') && (isTrade() || hasAsk() || hasBid());
	}
	CString TLTick::Serialize(void)
	{
		CString m;
		char d = ',';
		m.Append(sym);
		m.AppendChar(d);
		m.AppendFormat("%i,%i,,",date,time);
		m.AppendFormat("%f",trade);
		m.AppendChar(d);
		m.AppendFormat("%i",size);
		m.AppendChar(d);
		m.Append(ex);
		m.AppendChar(d);
		m.AppendFormat("%f,%f,",bid,ask);
		m.AppendFormat("%i,%i",bs,os);
		m.AppendChar(d);
		m.Append(be);
		m.AppendChar(d);
		m.Append(oe);
		m.AppendChar(d);
		m.AppendFormat("%i",depth);
		//m.Format(_T("%s,%i,%i,,%f,%i,%s,%f,%f,%i,%i,%s,%s,%i"),sym,date,time,trade,size,ex,bid,ask,bs,os,be,oe,depth);
		return m;
	}
	TLTick TLTick::Deserialize(CString message)
	{
		TLTick k;
		std::vector<CString> r;
		gsplit(message,_T(","),r);
		strncpy_s(k.sym, r[ksymbol], TLTick::MAX_SYM_LENGTH-1);
		k.date = _tstoi(r[kdate]);
		k.time = _tstoi(r[ktime]);
		k.trade = _tstof(r[ktrade]);
		k.size = _tstoi(r[ktsize]);
		//k.ex = r[ktex];
		strncpy_s(k.ex, r[ktex], TLTick::MAX_EX_LENGTH-1);
		k.bid = _tstof(r[kbid]);
		k.ask = _tstof(r[kask]);
		k.bs = _tstoi(r[kbidsize]);
		k.os = _tstoi(r[kasksize]);
		//k.be = r[kbidex];
		strncpy_s(k.be, r[kbidex], TLTick::MAX_EX_LENGTH-1);
		//k.oe = r[kaskex];
		strncpy_s(k.oe, r[kaskex], TLTick::MAX_EX_LENGTH-1);
		k.depth = _tstoi(r[ktdepth]);
		return k;
	}

	TLTick::~TLTick(void)
	{

	}

	TLTick & TLTick::operator=( const TLTick &rhs )
	{
		if (this != &rhs) 
		{
			strncpy_s(sym, rhs.sym, TLTick::MAX_SYM_LENGTH);
			date = rhs.date;
			time =   rhs.time;
			trade =	 rhs.trade;
			size =	 rhs.size;
			//ex = 	 rhs.ex;
			strncpy_s(ex, rhs.ex, TLTick::MAX_EX_LENGTH);
			bid = 	 rhs.bid;
			ask = 	 rhs.ask;
			bs = 	 rhs.bs;
			os = 	 rhs.os;
			//be = 	 rhs.be;
			strncpy_s(be, rhs.be, TLTick::MAX_EX_LENGTH);
			//oe = 	 rhs.oe;
			strncpy_s(oe, rhs.oe, TLTick::MAX_EX_LENGTH);
			depth =  rhs.depth;
			symid = rhs.symid;
		}

		return *this;
	}


}


