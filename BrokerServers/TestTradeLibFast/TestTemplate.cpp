#include "stdafx.h"
#include <cfix.h>
#include "TradeLibFast.h"

using namespace TradeLibFast;

static void __stdcall Basics()
{
	CFIX_ASSERT(true);
}



CFIX_BEGIN_FIXTURE( TestTemplate)
	CFIX_FIXTURE_ENTRY( Basics )
CFIX_END_FIXTURE()