// Copyright:
//		2008-2009, Johannes Passing (passing at users.sourceforge.net)
//
// This file is part of cfix.
//
// cfix is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// cfix is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with cfix.  If not, see <http://www.gnu.org/licenses/>.
//
//--------------------------------------------------------------------
// Definitions
//--------------------------------------------------------------------
//--------------------------------------------------------------------
//
//  Values are 32 bit values laid out as follows:
//
//   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
//   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
//  +---+-+-+-----------------------+-------------------------------+
//  |Sev|C|R|     Facility          |               Code            |
//  +---+-+-+-----------------------+-------------------------------+
//
//  where
//
//      Sev - is the severity code
//
//          00 - Success
//          01 - Informational
//          10 - Warning
//          11 - Error
//
//      C - is the Customer code flag
//
//      R - is a reserved bit
//
//      Facility - is the facility code
//
//      Code - is the facility's status code
//
//
// Define the facility codes
//


//
// Define the severity codes
//


//
// MessageId: CFIX_E_MISBEHAVIOUD_GETTC_ROUTINE
//
// MessageText:
//
// The routine exported by the testmodule did not provide the expected data
//
#define CFIX_E_MISBEHAVIOUD_GETTC_ROUTINE ((HRESULT)0x80048000L)

//
// MessageId: CFIX_E_UNSUPPORTED_VERSION
//
// MessageText:
//
// The API version used by the testmodule is not supported by this release
//
#define CFIX_E_UNSUPPORTED_VERSION       ((HRESULT)0x80048001L)

//
// MessageId: CFIX_E_DUP_SETUP_ROUTINE
//
// MessageText:
//
// The fixture defines more than one setup routine
//
#define CFIX_E_DUP_SETUP_ROUTINE         ((HRESULT)0x80048002L)

//
// MessageId: CFIX_E_DUP_TEARDOWN_ROUTINE
//
// MessageText:
//
// The fixture defines more than one teardown routine
//
#define CFIX_E_DUP_TEARDOWN_ROUTINE      ((HRESULT)0x80048003L)

//
// MessageId: CFIX_E_UNKNOWN_ENTRY_TYPE
//
// MessageText:
//
// The test fixture definition exportted by the testmodule contains
// unrecognized entry types.
//
#define CFIX_E_UNKNOWN_ENTRY_TYPE        ((HRESULT)0x80048004L)

//
// MessageId: CFIX_E_FIXTURE_NAME_TOO_LONG
//
// MessageText:
//
// The fixture name is too long
//
#define CFIX_E_FIXTURE_NAME_TOO_LONG     ((HRESULT)0x80048005L)

//
// MessageId: CFIX_E_TESTRUN_ABORTED
//
// MessageText:
//
// The testrun has been aborted
//
#define CFIX_E_TESTRUN_ABORTED           ((HRESULT)0x80048008L)

//
// MessageId: CFIX_E_UNKNOWN_THREAD
//
// MessageText:
//
// A call has been performed on an unknown thread. Register the thread
// appropriately before using any of the framework's APIs in a testcase
//
#define CFIX_E_UNKNOWN_THREAD            ((HRESULT)0x80048009L)

//
// MessageId: CFIX_E_THREAD_ABORTED
//
// MessageText:
//
// The thread has been aborted
//
#define CFIX_E_THREAD_ABORTED            ((HRESULT)0x8004800AL)

//
// MessageId: CFIX_E_SETUP_ROUTINE_FAILED
//
// MessageText:
//
// The setup routine has failed.
//
#define CFIX_E_SETUP_ROUTINE_FAILED      ((HRESULT)0x8004800BL)

//
// MessageId: CFIX_E_TEARDOWN_ROUTINE_FAILED
//
// MessageText:
//
// The teardown routine has failed.
//
#define CFIX_E_TEARDOWN_ROUTINE_FAILED   ((HRESULT)0x8004800CL)

//
// MessageId: CFIX_E_STACKTRACE_CAPTURE_FAILED
//
// MessageText:
//
// Capturing the stack trace failed.
//
#define CFIX_E_STACKTRACE_CAPTURE_FAILED ((HRESULT)0x8004800DL)

//
// MessageId: CFIX_E_TEST_ROUTINE_FAILED
//
// MessageText:
//
// The test routine has failed.
//
#define CFIX_E_TEST_ROUTINE_FAILED       ((HRESULT)0x8004800EL)

//
// MessageId: CFIX_E_DUP_BEFORE_ROUTINE
//
// MessageText:
//
// The fixture defines more than one before-routine
//
#define CFIX_E_DUP_BEFORE_ROUTINE        ((HRESULT)0x8004800FL)

//
// MessageId: CFIX_E_DUP_AFTER_ROUTINE
//
// MessageText:
//
// The fixture defines more than one after-routine
//
#define CFIX_E_DUP_AFTER_ROUTINE         ((HRESULT)0x80048010L)

//
// MessageId: CFIX_E_BEFORE_ROUTINE_FAILED
//
// MessageText:
//
// The before-routine initializing the test case has failed.
//
#define CFIX_E_BEFORE_ROUTINE_FAILED     ((HRESULT)0x80048011L)

//
// MessageId: CFIX_E_AFTER_ROUTINE_FAILED
//
// MessageText:
//
// The after-routine has failed.
//
#define CFIX_E_AFTER_ROUTINE_FAILED      ((HRESULT)0x80048012L)

//
// MessageId: CFIX_E_DBGHELP_MISSING
//
// MessageText:
//
// The library dbghelp.dll could not be found.
//
#define CFIX_E_DBGHELP_MISSING           ((HRESULT)0x80048013L)

//
// MessageId: CFIX_E_LOADING_DBGHELP_FAILED
//
// MessageText:
//
// The library dbghelp.dll could not be loaded.
//
#define CFIX_E_LOADING_DBGHELP_FAILED    ((HRESULT)0x80048014L)

//
// MessageId: CFIX_E_TOO_MANY_CHILD_THREADS
//
// MessageText:
//
// The maximum number of child threads has been exceeded.
//
#define CFIX_E_TOO_MANY_CHILD_THREADS    ((HRESULT)0x80048015L)

//
// MessageId: CFIX_E_FILAMENT_JOIN_TIMEOUT
//
// MessageText:
//
// At least one child thread did not complete within the alotted time.
//
#define CFIX_E_FILAMENT_JOIN_TIMEOUT     ((HRESULT)0x80048016L)

//
// MessageId: CFIX_E_DEFAULT_FILAMENT_CONFLICT
//
// MessageText:
//
// Cannot register more than one default filament.
//
#define CFIX_E_DEFAULT_FILAMENT_CONFLICT ((HRESULT)0x80048017L)

//
// MessageId: CFIX_E_INVALID_FIXTURE_FLAG
//
// MessageText:
//
// Invalid flag specified for fixture.
//
#define CFIX_E_INVALID_FIXTURE_FLAG      ((HRESULT)0x80048018L)

//
// MessageId: CFIX_E_UNSUPPORTED_FIXTURE_REVISION
//
// MessageText:
//
// Fixture revision not supported.
//
#define CFIX_E_UNSUPPORTED_FIXTURE_REVISION ((HRESULT)0x80048019L)

//
// MessageId: CFIX_E_UNSUPPORTED_EVENT_SINK_VERSION
//
// MessageText:
//
// Event sink revision not supported.
//
#define CFIX_E_UNSUPPORTED_EVENT_SINK_VERSION ((HRESULT)0x8004801AL)

//
// MessageId: CFIX_E_MISSING_EVENT_SINK_EXPORT
//
// MessageText:
//
// Event sink DLL does not export a sink creation routine. Check that you are using the correct DLL and that the exports are unmangled.
//
#define CFIX_E_MISSING_EVENT_SINK_EXPORT ((HRESULT)0x8004801BL)

//
// MessageId: CFIX_E_LOADING_EVENTDLL_FAILED
//
// MessageText:
//
// Loading Event DLL failed. Make sure that path and CPU architecture are correct.
//
#define CFIX_E_LOADING_EVENTDLL_FAILED   ((HRESULT)0x8004801CL)

//
// MessageId: CFIX_E_EVENTDLL_INVALID_OPTIONS
//
// MessageText:
//
// Options passed to Event DLL are invalid.
//
#define CFIX_E_EVENTDLL_INVALID_OPTIONS  ((HRESULT)0x8004801DL)

