#pragma once

/*----------------------------------------------------------------------
 * Purpose:
 *		This header file contains the neccessary declarations for creating
 *		a custom event DLL. When registered, a custom event DLL receives
 *		all events generated during a cfix test run. The primary purpose
 *		of event DLLs is thus to process the events and generate appropriate
 *		output to the console, a file, or whatever target device is 
 *		appropriate.
 *
 *		You only need to include this header file, do not include any 
 *		further cfix headers.
 *
 *                cfixaux.h        cfixkrio.h
 *                  ^ ^ ^--------+     ^
 *                 /   \          \   /
 *                /     \          \ /
 *			cfixapi.h  cfixpe.h  cfixkr.h
 *			  ^	^	  ^	  ^         
 *		     /	|	 /	  |         
 *		    /	|	/	  |         
 *		   |  [cfix]	cfix.h      
 *         |         
 *         |         
 *     cfixevent.h         
 *         ^         
 *         |         
 *         |         
 *    [Event DLL] 
 *
 * Copyright:
 *		2010, Johannes Passing (passing at users.sourceforge.net)
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

#include <cfixapi.h>

#define CFIX_EVENT_SINK_VERSION	MAKELONG( 1, 0 )

/*++
	Structure Description:
		Defines the interface of a report sink.

		Event routines are called in the following sequence:

		1.  BeforeFixtureStart is called.
		2.  (Setup routine starts)
		3.  ReportEvent may be called for events raised by setup routine.
		4.  (Setup routine ends)
		5.  BeforeTestCaseStart is called.
		  
		6.  (Before routine starts)
		7.  ReportEvent may be called for events raised by before routine.
		8.  (Before routine ends)
		9.  (Test Case 1 starts)
		10. ReportEvent may be called for events raised by test routine.
		11. (Test Case 1 ends)
		12. (After routine starts)
		13. ReportEvent may be called for events raised by after routine.
		14. )After routine ends)
		15. AfterTestCaseFinish is called.
			(steps 5-11 are repeated for each subsequent test)

		16. (Teardown routine starts)
		17. ReportEvent may be called for events raised by teardown routine.
		18. (Teardown routine ends)
		19. AfterFixtureFinish is called.

		N.B. Implementations need to be thread-safe.
--*/
typedef struct _CFIX_EVENT_SINK
{
	//
	// Interface version, initialize to CFIX_EVENT_SINK_VERSION.
	//
	ULONG Version;

	/*++
		Routine Description:
			Report an event such as a log event, failed assertion,
			or unhandled excpetion.

		Arguments:
			This			- Pointer to the sink structure.
			Thread			- Thread the event occured on.
			Thread			- Thread the event occured on.
			ModuleBaseName	- Base name (i.e. file name w/o path) of module.
			FixtureName		- Name of the fixture.
			TestCaseName	- Name of test case.
			Event			- Structure containing event information.
	--*/
	VOID ( CFIXCALLTYPE * ReportEvent ) (
		__in struct _CFIX_EVENT_SINK *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCWSTR ModuleBaseName,
		__in PCWSTR FixtureName,
		__in PCWSTR TestCaseName,
		__in PCFIX_TESTCASE_EXECUTION_EVENT Event
	);

	/*++
		Routine Description:
			Event routine, see comment above.
			
		Arguments:
			This			- Pointer to the sink structure.
			Thread			- Thread the event occured on.
			ModuleBaseName	- Base name (i.e. file name w/o path) of module.
			FixtureName		- Name of the fixture.
	--*/
	VOID ( CFIXCALLTYPE * BeforeFixtureStart ) (
		__in struct _CFIX_EVENT_SINK *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCWSTR ModuleBaseName,
		__in PCWSTR FixtureName
		);

	/*++
		Routine Description:
			Event routine, see comment above.
			
		Arguments:
			This			- Pointer to the sink structure.
			Thread			- Thread the event occured on.
			ModuleBaseName	- Base name (i.e. file name w/o path) of module.
			FixtureName		- Name of the fixture.
			RanToCompletion	- Indicates whether the fixture, including all
							  tests, ran to completion or whether it was
							  stopped/short-circuited before the last test
							  was able to complete.
	--*/
	VOID ( CFIXCALLTYPE * AfterFixtureFinish ) (
		__in struct _CFIX_EVENT_SINK *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCWSTR ModuleBaseName,
		__in PCWSTR FixtureName,
		__in BOOL RanToCompletion
		);

	/*++
		Routine Description:
			Event routine, see comment above.
			
		Arguments:
			This			- Pointer to the sink structure.
			Thread			- Thread the event occured on.
			ModuleBaseName	- Base name (i.e. file name w/o path) of module.
			FixtureName		- Name of the fixture.
			TestCaseName	- Name of test case.
	--*/
	VOID ( CFIXCALLTYPE * BeforeTestCaseStart ) (
		__in struct _CFIX_EVENT_SINK *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCWSTR ModuleBaseName,
		__in PCWSTR FixtureName,
		__in PCWSTR TestCaseName
		);

	/*++
		Routine Description:
			Event routine, see comment above.
			
		Arguments:
			This			- Pointer to the sink structure.
			Thread			- Thread the event occured on.
			ModuleBaseName	- Base name (i.e. file name w/o path) of module.
			FixtureName		- Name of the fixture.
			TestCaseName	- Name of test case.
			RanToCompletion	- Indicates whether the test case ran to 
							  completion or whether it was stopped/
							  short-circuited.
	--*/
	VOID ( CFIXCALLTYPE * AfterTestCaseFinish ) (
		__in struct _CFIX_EVENT_SINK *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCWSTR ModuleBaseName,
		__in PCWSTR FixtureName,
		__in PCWSTR TestCaseName,
		__in BOOL RanToCompletion
		);
	
	/*++
		Routine Description:
			Increment reference count.
	--*/
	VOID ( CFIXCALLTYPE * Reference ) (
		__in struct _CFIX_EVENT_SINK *This
		);

	/*++
		Routine Description:
			Decrement reference count. Object may be deleted if
			count reaches zero.
	--*/
	VOID ( CFIXCALLTYPE * Dereference ) (
		__in struct _CFIX_EVENT_SINK *This
		);
} CFIX_EVENT_SINK, *PCFIX_EVENT_SINK;

//
// When showing stack traces, include source/line information.
//
#define CFIX_EVENT_SINK_FLAG_SHOW_STACKTRACE_SOURCE_INFORMATION 1

/*++
	Description:
		Creates a new instance of an event sink. This typedef
		defines the signature of the factory function an event
		DLL has to export under the name 'CreateEventSink'.

	Arguments:
		Version		- Interface version expected by cfix.
		Flags		- Combination of CFIX_EVENT_SINK_FLAG_* flags.
		Options		- DLL-specific command line options. Format
					  is defined by event DLL itself.
		Reserved	- Reserved, always 0.
		Handler		- Result.

	Return Value:
		S_OK		- Event sink successfuly created.
		CFIX_E_UNSUPPORTED_EVENT_SINK_VERSION 
					- Requested version not supported.
		(any other failure HRESULT)
--*/
typedef HRESULT ( CFIXCALLTYPE * CFIX_CREATE_EVENT_SINK_ROUTINE ) (
	__in ULONG Version,
	__in ULONG Flags,
	__in_opt PCWSTR Options,
	__reserved ULONG Reserved,
	__out PCFIX_EVENT_SINK *Sink 
	);

/*++
	Routine Description:
		Create a proxy execution context that emits event to a event sink.

		All calls are delegated to the event sink specified in the
		TargetExecContext parameter.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixCreateEventEmittingExecutionContextProxy(
	__in PCFIX_EXECUTION_CONTEXT TargetExecContext,
	__in PCFIX_EVENT_SINK EventSink,
	__out PCFIX_EXECUTION_CONTEXT *Proxy
	);