#pragma once

/*----------------------------------------------------------------------
 * Purpose:
 *		Shared header file. Do not include directly!
 *
 *            cfixaux.h        cfixkrio.h
 *              ^ ^ ^--------+     ^
 *             /   \          \   /
 *            /     \          \ /
 *		cfixapi.h  cfixpe.h  cfixkr.h
 *			^	  ^	  ^         
 *			|	 /	  |         
 *			|	/	  |         
 *		  [cfix]	cfix.h      
 *                    ^         
 *                    |         
 *                    |         
 *          [Test DLLs/Drivers] 
 *
 * Copyright:
 *		2008-2009, Johannes Passing (passing at users.sourceforge.net)
 *
 * This file is part of cfix.
 *
 * cfix is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * cfix is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with cfix.  If not, see <http://www.gnu.org/licenses/>.
 */

#ifdef _WIN64
#define CFIXCALLTYPE
#else
#define CFIXCALLTYPE __stdcall
#endif

#if !defined(CFIXAPI)
#define CFIXAPI EXTERN_C __declspec(dllimport)
#endif
#define ___CFIX_WIDE( str ) L ##  str 
#define __CFIX_WIDE( str ) ___CFIX_WIDE( str )

//
// Maximum length of a fixture name, including NULL terminator.
//
#define CFIX_MAX_FIXTURE_NAME_CCH 128

#define CFIX_SYSTEM_THREAD_FLAG_SYSTEM_CONTEXT	1

//
// Embedding-related constants.
//
#define CFIX_EMB_INIT_ENVVAR_NAMEA "CFIX_INIT_ROUTINE"
#define CFIX_EMB_INIT_ENVVAR_NAME L"CFIX_INIT_ROUTINE"
#define CFIX_S_EXIT_PROCESS ( ( HRESULT ) 0x4AFF0UL )

typedef enum CFIX_REPORT_DISPOSITION
{
	//
	// Continue execution.
	//
	CfixContinue		= 0,

	//
	// Break if in debugger.
	//
	CfixBreak			= 1,

	//
	// Always break.
	//
	CfixBreakAlways		= 2,

	//
	// Abort testrun.
	//
	CfixAbort			= 3
} CFIX_REPORT_DISPOSITION;

typedef enum
{
	CfixEventFailedAssertion		= 0,
	CfixEventUncaughtException		= 1,
	CfixEventInconclusiveness		= 2,
	CfixEventLog					= 3
} CFIX_EVENT_TYPE;

#define CFIX_EXIT_THREAD_ABORTED 0xffffffff


/*++
	Routine Description:
		Retrieve information about a stackframe.
--*/
typedef HRESULT ( CFIXCALLTYPE * CFIX_GET_INFORMATION_STACKFRAME_ROUTINE ) (
	__in ULONGLONG Frame,
	__in SIZE_T ModuleNameCch,
	__out_ecount_z(ModuleNameCch) PWSTR ModuleName,
	__in SIZE_T FunctionNameCch,
	__out_ecount_z(FunctionNameCch) PWSTR FunctionName,
	__out PULONG Displacement,
	__in SIZE_T SourceFileCch,
	__out_ecount_z(SourceFileCch) PWSTR SourceFile,
	__out PULONG SourceLine 
	);

/*++
	Structure Description:
		Contains (minimum) stacktrace information, i.e.
		the PCs of each stack.
--*/
typedef struct _CFIX_STACKTRACE
{
	//
	// Number of frames in Frames array.
	//
	ULONG FrameCount;

	//
	// Function to use to resolve symbolic information.
	//
	CFIX_GET_INFORMATION_STACKFRAME_ROUTINE GetInformationStackFrame;

	//
	// PC-Addresses of frames. Index 0 contains the topmost frame.
	//
	ULONGLONG Frames[ ANYSIZE_ARRAY ];
} CFIX_STACKTRACE, *PCFIX_STACKTRACE;

/*++
	Structure Description:
		Identifies the thread and its association to a filament/main
		thread.
--*/
typedef struct _CFIX_THREAD_ID
{
	//
	// Windows Thread ID.
	//
	ULONG ThreadId;

	//
	// ID of the filament/main thread, i.e. the initial thread of
	// the individual test case execution.
	//
	ULONG MainThreadId;
} CFIX_THREAD_ID, *PCFIX_THREAD_ID;

typedef enum CFIX_API_TYPE
{
	//
	// C/C++ Base API.
	//
	CfixApiTypeBase					= 1,

	//
	// C++ API.
	//
	CfixApiTypeCc					= 2,

	//
	// WinUnit.
	//
	CfixApiTypeWinUnitFixture		= 3,
	CfixApiTypeWinUnitStandalone	= 4,

	CfixApiTypeMin					= 1,
	CfixApiTypeMax					= 4
} CFIX_API_TYPE;
