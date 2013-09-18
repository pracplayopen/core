#pragma once

/*----------------------------------------------------------------------
 * Purpose:
 *		Cfix main header file. 
 *
 *		Note: Test code should include cfix.h.
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

#include <windows.h>
#include <cfixaux.h>
#include <cfixmsg.h>

struct _CFIX_FIXTURE;
struct _CFIX_TEST_CASE;
struct _CFIX_TEST_MODULE;

#ifndef OPTIONAL
#define OPTIONAL
#endif

/*----------------------------------------------------------------------
 *
 * Context Object.
 *
 */

#define CFIX_TEST_CONTEXT_VERSION MAKELONG( 1, 1 )

typedef struct _CFIX_TESTCASE_EXECUTION_EVENT
{
	CFIX_EVENT_TYPE Type;
	union
	{
		struct
		{
			EXCEPTION_RECORD ExceptionRecord;
		} UncaughtException;
		
		struct
		{
			PCWSTR File;
			PCWSTR Routine;
			UINT Line;
			PCWSTR Expression;
			DWORD LastError;
		} FailedAssertion;

		struct 
		{
			PCWSTR Message;
		} Inconclusiveness;

		struct 
		{
			PCWSTR Message;
		} Log;
	} Info;

	//
	// N.B. Structure is of variable length.
	//
	CFIX_STACKTRACE StackTrace;
} CFIX_TESTCASE_EXECUTION_EVENT, *PCFIX_TESTCASE_EXECUTION_EVENT;

/*++
	Structure Description:
		Defines the interface of an execution context object.

		Note that the object can be used from multiple threads
		in parallel. The implemenatation must implement proper
		state tracking on a per-thread basis.

		The MainThreadId identifies the thread that a test case has
		been invoked on. In case a test case spawns child threads,
		and one of the child threads reports an event, the MainThreadId
		will differ from the current thread ID. In all other cases,
		the MainThreadId will equal the current thread ID.
--*/
typedef struct _CFIX_EXECUTION_CONTEXT
{
	/*++
		Field Descritpion:
			Version, set to CFIX_TEST_CONTEXT_VERSION.
	--*/
	ULONG Version;

	/*++
		Routine Description:
			Report an event. See also the discussion of 
			QueryDefaultDisposition.

		Return Value:
			Disposition how caller should proceed.

			In the case of unhandled exceptions, CfixBreak instructs
			the framework not to handle the exception. Most likely,
			the process will experience an unhandled exception and
			the user has the chance to break in using the debugger.
	--*/
	CFIX_REPORT_DISPOSITION ( CFIXCALLTYPE * ReportEvent ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in PCFIX_TESTCASE_EXECUTION_EVENT Event
	);

	/*++
		Routine Description:
			Query the disposition for an event type, regardless of
			a specific instance of the event.

			This routine will not be called for user mode tests -
			ReportEvent is called for each event individually.

			For kernel mode tests, the disposition will be queried
			beforehand by calling this routine. Only after the test case
			has finished, will any events be reported via ReportEvent.
			The disposition returned by ReportEvent will in this case
			be ignored.

		Return Value:
			Disposition how caller should proceed.
	--*/
	CFIX_REPORT_DISPOSITION ( CFIXCALLTYPE * QueryDefaultDisposition ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in CFIX_EVENT_TYPE EventType
	);

	HRESULT ( CFIXCALLTYPE * BeforeFixtureStart ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in struct _CFIX_FIXTURE *Fixture
		);

	VOID ( CFIXCALLTYPE * AfterFixtureFinish ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in struct _CFIX_FIXTURE *Fixture,
		__in BOOL RanToCompletion
		);

	HRESULT ( CFIXCALLTYPE * BeforeTestCaseStart ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in struct _CFIX_TEST_CASE *TestCase
		);

	VOID ( CFIXCALLTYPE * AfterTestCaseFinish ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in struct _CFIX_TEST_CASE *TestCase,
		__in BOOL RanToCompletion
		);

	/*++
		Routine Description:
			Called when the test code intends to spawn a new thread that
			belongs to the currently executing testcase. 
			
			Once the thread has been created, AfterCreateChildThread
			is invoked, passing the Context value returned from 
			BeforeCreateChildThread.

			This way, the execution context is given the opportunity to 
			relate the new thread to its 'parent' 

			This routine is called on the existing ('parent') thread.

			N.B. For kernel mode tests, this callback is never invoked.
	--*/
	HRESULT ( CFIXCALLTYPE * CreateChildThread ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__out PVOID *Context
		);

	/*++
		Routine Description:
			Called before the new child thread begins executing
			user code.

			This routine is called on the child thread.

			N.B. For kernel mode tests, this callback is never invoked.
	--*/
	VOID ( CFIXCALLTYPE * BeforeChildThreadStart ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in_opt PVOID Context
		);

	/*++
		Routine Description:
			Called before the new child thread ends.

			This routine is called on the child thread.

			N.B. For kernel mode tests, this callback is never invoked.
	--*/
	VOID ( CFIXCALLTYPE * AfterChildThreadFinish ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in_opt PVOID Context
		);

	/*++
		Routine Description:
			Called when an unhandled exception occurs in test code.
			
			This gives the object the option to create dump files etc.
	--*/
	VOID ( CFIXCALLTYPE * OnUnhandledException ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This,
		__in PCFIX_THREAD_ID Thread,
		__in PEXCEPTION_POINTERS ExcpPointers
		);

	/*++
		Routine Description:
			Increment reference count.
	--*/
	VOID ( CFIXCALLTYPE * Reference ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This
		);

	/*++
		Routine Description:
			Dencrement reference count. Object may be deleted if
			count reaches zero.
	--*/
	VOID ( CFIXCALLTYPE * Dereference ) (
		__in struct _CFIX_EXECUTION_CONTEXT *This
		);
} CFIX_EXECUTION_CONTEXT, *PCFIX_EXECUTION_CONTEXT;

#define CfixIsValidContext( Context ) ( 							\
	 ( Context ) &&													\
	 ( Context )->Version == CFIX_TEST_CONTEXT_VERSION &&			\
	 ( Context )->ReportEvent &&									\
	 ( Context )->BeforeFixtureStart &&								\
	 ( Context )->AfterFixtureFinish &&								\
	 ( Context )->BeforeTestCaseStart &&							\
	 ( Context )->AfterTestCaseFinish &&							\
	 ( Context )->CreateChildThread &&								\
	 ( Context )->BeforeChildThreadStart &&							\
	 ( Context )->AfterChildThreadFinish &&							\
	 ( Context )->OnUnhandledException &&							\
	 ( Context )->Reference &&										\
	 ( Context )->Dereference )

/*----------------------------------------------------------------------
 *
 * Test Module/Fixture/Case.
 *
 */

//
// Capture stack backtraces for failed reports.
//
#define CFIX_TEST_FLAG_CAPTURE_STACK_TRACES		1

//
// Register current filament as default filament for 
// anonymous threads.
//
#define CFIX_TEST_FLAG_PROVIDE_DEFAULT_FILAMENT 2

/*++
	Description:
		Prototype of setup, teardown and testcase routines.

	Parameters:
		Module		Module fixture belongs to.
		TestCase	Testcase to run.
		Flags		CFIX_TEST_FLAG_*.

	Return Values:
		CFIX_E_TEST_ROUTINE_FAILED	- Failed, but run shall continue.
		CFIX_E_TESTRUN_ABORTED		- Failed and run should be aborted.
		S_OK						- Success.
--*/
typedef HRESULT ( CFIXCALLTYPE * CFIX_TESTCASE_ROUTINE ) (
	__in struct _CFIX_TEST_CASE *TestCase,
	__in PCFIX_EXECUTION_CONTEXT Context,
	__in ULONG Flags
	);

/*++
	Description:
		Prototype of setup, teardown and testcase routines.

	Parameters:
		Module		Module fixture belongs to.
		Fixture		Fixture to setup/teardown.
		Flags		CFIX_TEST_FLAG_*.

	Return Values:
		CFIX_E_SETUP_ROUTINE_FAILED,		
		CFIX_E_TEARDOWN_ROUTINE_FAILED	- Failed, but run shall continue.
		CFIX_E_TESTRUN_ABORTED			- Failed and run should be aborted.
		S_OK							- Success.
--*/
typedef HRESULT ( CFIXCALLTYPE * CFIX_SETUPTEARDOWN_ROUTINE ) (
	__in struct _CFIX_FIXTURE *Fixture,
	__in PCFIX_EXECUTION_CONTEXT Context,
	__in ULONG Flags
	);

/*++
	Routine Description:	
		Increment/Decrement module reference counter.
--*/
typedef VOID ( CFIXCALLTYPE * CFIX_ADJ_REFERENCES_ROUTINE ) (
	__in struct _CFIX_TEST_MODULE *TestModule
	);

typedef struct _CFIX_TEST_CASE
{
	PCWSTR Name;
	ULONG_PTR Routine;
	struct _CFIX_FIXTURE *Fixture;
} CFIX_TEST_CASE, *PCFIX_TEST_CASE;

typedef struct _CFIX_FIXTURE
{
	WCHAR Name[ CFIX_MAX_FIXTURE_NAME_CCH ];
	ULONG_PTR SetupRoutine;
	ULONG_PTR TeardownRoutine;
	ULONG_PTR BeforeRoutine;
	ULONG_PTR AfterRoutine;

	//
	// Implementation-defined.
	//
	ULONG_PTR Reserved;

	//
	// Type of API used to implement this fixture.
	//
	CFIX_API_TYPE ApiType;

	//
	// CFIX_FIXTURE_USES_* flags.
	//
	ULONG Flags;

	//
	// Backpointer to enclosing module.
	//
	struct _CFIX_TEST_MODULE *Module;

	//
	// # of elements in TestCases array.
	//
	ULONG TestCaseCount;
	CFIX_TEST_CASE TestCases[ ANYSIZE_ARRAY ];
} CFIX_FIXTURE, *PCFIX_FIXTURE;

#define CFIX_TEST_MODULE_VERSION MAKELONG( 1, 1 )

typedef struct _CFIX_TEST_MODULE
{
	//
	// Version. Set to CFIX_TEST_MODULE_VERSION.
	//
	ULONG Version;

	struct
	{
		CFIX_TESTCASE_ROUTINE RunTestCase;
		CFIX_SETUPTEARDOWN_ROUTINE Setup;
		CFIX_SETUPTEARDOWN_ROUTINE Teardown;
		CFIX_SETUPTEARDOWN_ROUTINE Before;
		CFIX_SETUPTEARDOWN_ROUTINE After;
		CFIX_ADJ_REFERENCES_ROUTINE Reference;
		CFIX_ADJ_REFERENCES_ROUTINE Dereference;
		CFIX_GET_INFORMATION_STACKFRAME_ROUTINE GetInformationStackFrame OPTIONAL;
	} Routines;
	PCWSTR Name;
	UINT FixtureCount;
	PCFIX_FIXTURE *Fixtures;
} CFIX_TEST_MODULE, *PCFIX_TEST_MODULE;


/*----------------------------------------------------------------------
 *
 * Testrun.
 *
 */
#define CFIX_ACTION_VERSION MAKELONG( 1, 0 )

typedef struct _CFIX_ACTION
{
	/*++
		Interface version - set to CFIX_ACTION_VERSION.
	--*/
	DWORD Version;

	/*++
		Routine Description:
			Run the action. The actual action greatly depends
			on the implementation chosen.
	--*/
	HRESULT ( CFIXCALLTYPE * Run ) (
		__in struct _CFIX_ACTION *This,
		__in PCFIX_EXECUTION_CONTEXT Context
		);
	
	/*++
		Routine Description:
			Increment reference count.
	--*/
	VOID ( CFIXCALLTYPE * Reference ) (
		__in struct _CFIX_ACTION *This
		);

	/*++
		Routine Description:
			Dencrement reference count. Object is deleted if
			count reaches zero.
	--*/
	VOID ( CFIXCALLTYPE * Dereference ) (
		__in struct _CFIX_ACTION *This
		);
} CFIX_ACTION, *PCFIX_ACTION;

#define CfixIsValidAction( Action ) (								\
	Action &&														\
	Action->Version == CFIX_ACTION_VERSION &&						\
	Action->Run &&													\
	Action->Reference &&											\
	Action->Dereference )											\

/*----------------------------------------------------------------------
 *
 * API.
 *
 */

/*++
	Routine Description:	
		Loads a given module and creates a test module
		based on the module's exports.

	Parameters:
		Module			Module handle. 
		                Note: The refcount of the module will *not*
						be incremented.
		TestModule		Module - initial reference count is 1.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixCreateTestModule(
	__in HMODULE ModuleHandle,
	__out PCFIX_TEST_MODULE *TestModule
	);

/*++
	Routine Description:	
		Loads a given module and creates a test module
		based on the module's exports.

	Parameters:
		ModulePath		Path to DLL.
		TestModule		Module - initial reference count is 1.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixCreateTestModuleFromPeImage(
	__in PCWSTR ModulePath,
	__out PCFIX_TEST_MODULE *TestModule
	);

/*++
	Routine Description:	
		Loads a given driver and creates a test module
		based on the drivers's exports.

	Parameters:
		DriverPath		Path to driver.
		Installed		Indicates whether the driver had to be installed
		Loaded			Indicates whether the driver had to be loaded first.
		TestModule		Module - initial reference count is 1.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixklCreateTestModuleFromDriver(
	__in PCWSTR DriverPath,
	__out PCFIX_TEST_MODULE *TestModule,
	__out_opt PBOOL Installed,
	__out_opt PBOOL Loaded
	);

/*++
	Routine Description:
		Compatibility routine. Wrapper for GetNativeSystemInfo,
		also works on downloevel Windows versions.
--*/
CFIXAPI VOID CFIXCALLTYPE CfixklGetNativeSystemInfo(
	__out LPSYSTEM_INFO SystemInfo
	);

/*++
	Routine Description:
		Compatibility routine. Wrapper for IsWow64Process,
		also works on downloevel Windows versions.
--*/
CFIXAPI BOOL CFIXCALLTYPE CfixklIsWow64Process(
	__in HANDLE Process,
	__out PBOOL Wow64Process
	);

//
// If one routine fails, prematurely abort fixture.
//
#define CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_FIXTURE_ON_FAILURE		1

//
// If a setup routine fails, prematurely abort run. The fixture
// will be aborted in anyway.
//
#define CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_RUN_ON_SETUP_FAILURE	2

//
// If a fixture fails, escalate the failure s.t. the entire run
// fails. 
//
// N.B. Should not be used in isolation, use 
// CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_RUN_ON_FAILURE instead.
//
#define CFIX_FIXTURE_EXECUTION_ESCALATE_FIXTURE_FAILUES				4

//
// If one routine fails, prematurely abort run.
// Implies CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_FIXTURE_ON_FAILURE and
// CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_RUN_ON_SETUP_FAILURE.
//
#define CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_RUN_ON_FAILURE			\
	( CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_FIXTURE_ON_FAILURE |		\
	  CFIX_FIXTURE_EXECUTION_SHORTCIRCUIT_RUN_ON_SETUP_FAILURE |	\
	  CFIX_FIXTURE_EXECUTION_ESCALATE_FIXTURE_FAILUES )

//
// Capture stack backtraces for failed reports.
//
#define CFIX_FIXTURE_EXECUTION_CAPTURE_STACK_TRACES					256

/*++
	Routine Description:
		Creates an action that executes sn entire fixture.

	Parameters:
		Fixture		- Fixture to run.
		Flags		- 0 or combination of CFIX_FIXTURE_EXECUTION_*.
		TestCase	- Index of test case to run or -1 to run all.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixCreateFixtureExecutionAction(
	__in PCFIX_FIXTURE Fixture,
	__in ULONG Flags,
	__in ULONG TestCase,
	__out PCFIX_ACTION *Action
	);

/*++
	Routine Description:
		Create a composite action that executes other actions
		in sequence.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixCreateSequenceAction(
	__out PCFIX_ACTION *Action
	);

/*++
	Routine Description:
		Add an action to a sequence action created by 
		CfixCreateSequenceAction.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixAddEntrySequenceAction(
	__in PCFIX_ACTION SequenceAction,
	__in PCFIX_ACTION ActionToAdd
	);

typedef enum CFIX_MODULE_TYPE
{
	CfixModuleDll		= 0,
	CfixModuleExe		= 1,
	CfixModuleDriver	= 2,
} CFIX_MODULE_TYPE;

typedef struct _CFIX_MODULE_INFO
{
	ULONG SizeOfStruct;

	//
	// CPU Architecture. IMAGE_FILE_MACHINE_*.
	//
	WORD MachineType;

	//
	// Subsystem. IMAGE_SUBSYSTEM_*.
	//
	WORD Subsystem;

	BOOL FixtureExportsPresent;

	CFIX_MODULE_TYPE ModuleType;
} CFIX_MODULE_INFO, *PCFIX_MODULE_INFO;

/*++
	Routine Description:
		Query a PE file without actually loading it. May also be used
		for images of a different CPU architecture.
--*/
CFIXAPI HRESULT CFIXCALLTYPE CfixQueryPeImage(
	__in PCWSTR Path,
	__out PCFIX_MODULE_INFO Info
	);
