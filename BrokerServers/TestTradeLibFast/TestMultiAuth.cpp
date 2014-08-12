#include "stdafx.h"
#include <cfix.h>
#include "TradeLibFast.h"
#include "Util.h"
#include "MultiAuth.h"



using namespace TradeLibFast;

static void __stdcall TestGetURL()
{
	// commenting out because of 2019 compilation problems with httpcall dependency
	return;
	// test url 
	CString turl("https://s3.amazonaws.com/security.pracplay.com/glean/glean.25.acl.txt");
	CString data2;
	data2 = GetURL(turl);
	CFIX_ASSERT(data2.GetLength()>0);
	//CFIX_ASSERT(data2=="*");
}


static void __stdcall TestIsAuth()
{
	// commenting out because of 2019 compilation problems with httpcall dependency
	return;
	// test url 
	CString turl("https://s3.amazonaws.com/security.pracplay.com/glean/glean.25.acl.txt");
	CFIX_ASSERT(isAuthorized(turl,CString("*"),3,false));
	CFIX_ASSERT(!isAuthorized(turl,CString("testuser"),3,false));
}

static void __stdcall TestPublicIP()
{
	// commenting out because of 2019 compilation problems with httpcall dependency
	return;
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