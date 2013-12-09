#include "stdafx.h"
#include <cfix.h>
#include "TradeLibFast.h"
#include "Util.h"
#include "MultiAuth.h"



using namespace TradeLibFast;

static void __stdcall TestGetURL()
{
	// test url 
	CString turl("https://s3.amazonaws.com/security.pracplay.com/glean/glean.25.acl.txt");
	CString data2;
	data2 = GetURL(turl);
	CFIX_ASSERT(data2.GetLength()>0);
	//CFIX_ASSERT(data2=="*");
}


static void __stdcall TestIsAuth()
{
	// test url 
	CString turl("https://s3.amazonaws.com/security.pracplay.com/glean/glean.25.acl.txt");
	CFIX_ASSERT(isAuthorized(turl,CString("*"),false));
	CFIX_ASSERT(!isAuthorized(turl,CString("testuser"),false));
}

static void __stdcall TestPublicIP()
{
	CString ip("");
	ip = GetPublicIP();
	CFIX_ASSERT(ip!="0.0.0.0");
	CFIX_ASSERT(ip.Find(".")>=0);
	CFIX_ASSERT(ip.GetLength()>=12);
}



CFIX_BEGIN_FIXTURE( TestMultiAuth)
	CFIX_FIXTURE_ENTRY( TestGetURL)
	CFIX_FIXTURE_ENTRY( TestIsAuth)
	CFIX_FIXTURE_ENTRY( TestPublicIP)
CFIX_END_FIXTURE()