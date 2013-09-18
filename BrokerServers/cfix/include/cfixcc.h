#pragma once

/*----------------------------------------------------------------------
 * Purpose:
 *		Auxilary header file for C++.
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

#include <cfix.h>

#if defined( __cplusplus ) && ! defined( CFIX_KERNELMODE )

#pragma warning( push )
#if _MSC_VER >= 1400 
#pragma warning( disable: 6011; disable: 6387 )
#endif
#include <strsafe.h>
#pragma warning( pop )

#pragma warning( push )
#pragma warning( disable: 4995 )	// strcpy deprecation et al.
#include <sstream>
#pragma warning( pop )

//
// STLport requires explicitly including string.
//
#include <string>

#ifndef CFIXCC_FLOAT_COMPARE_MAX_ULPS 
#define CFIXCC_FLOAT_COMPARE_MAX_ULPS 10
#endif

/*----------------------------------------------------------------------
 *
 * Value formatting.
 *
 */

namespace cfixcc
{

	/*++
		ANSI compatibility.
	 
		Although cfix is UNICODE only, ANSI strings are allowed in assertions.
	 --*/
	inline std::wostream& operator <<( 
		__in std::wostream& os, 
		__in const std::string& str )
	{
		WCHAR Buffer[ 128 ];
		if ( MultiByteToWideChar(
			CP_ACP,
			0,
			str.c_str(),
			-1,
			Buffer,
			_countof( Buffer ) ) > 0 )
		{
			return os << Buffer;
		}
		else
		{
			return os << L"[conversion failed]";
		}
	}

	/*++
		Structure description:
			Wrapper for a value. Used in order to limit the applicability of 
			operator << implementations. 
	--*/
	template< typename ValueT >
	struct ValueWrapper
	{
		const ValueT& Value;

		explicit ValueWrapper( 
			__in const ValueT& val 
			) 
			: Value( val )
		{
		}

	private:
		ValueWrapper( const ValueWrapper& );
		const ValueWrapper& operator=( const ValueWrapper& );
	};

#ifndef CFIXCC_OMIT_DEFAULT_OUTPUT_OPERATOR
	template< typename ValueT >
	inline std::wostream& operator <<( 
		__in std::wostream& os, 
		__in const ValueWrapper< ValueT >& val )
	{
		UNREFERENCED_PARAMETER( val );
		return os << L"(object)";
	}
#endif

	#define __CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( Type )				\
	inline std::wostream& operator <<(										\
		__in std::wostream& os, 											\
		__in const cfixcc::ValueWrapper< Type >& val )						\
	{																		\
		return os << val.Value;												\
	}

	#define __CFIXCC_GENERATE_C_STRING_OUTPUT_OPERATOR( Type )				\
	inline std::wostream& operator <<(										\
		__in std::wostream& os, 											\
		__in const cfixcc::ValueWrapper< Type >& val )						\
	{																		\
		if (  val.Value == NULL )											\
		{																	\
			return os << L"(NULL)";											\
		}																	\
		else																\
		{																	\
			return os << L"\"" << val.Value << L"\"";						\
		}																	\
	}


	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( bool )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( short )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( unsigned short )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( int )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( unsigned int )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( long )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( unsigned long )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( float )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( double )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( LONGLONG )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( ULONGLONG )
	__CFIXCC_GENERATE_C_STRING_OUTPUT_OPERATOR( PSTR )
	__CFIXCC_GENERATE_C_STRING_OUTPUT_OPERATOR( PWSTR )
	__CFIXCC_GENERATE_C_STRING_OUTPUT_OPERATOR( PCSTR )
	__CFIXCC_GENERATE_C_STRING_OUTPUT_OPERATOR( PCWSTR )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( std::string )
	__CFIXCC_GENERATE_PRIMITIVE_OUTPUT_OPERATOR( std::wstring )

}


/*----------------------------------------------------------------------
 *
 * Relations.
 *
 */
namespace cfixcc
{
	/*++
		Routine Description:
			Compares two floating point numbers for almost-equality.
			Algorithm based on the one published on
			http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm.

		Parameters:
			Lhs, Rhs	- comparands.
			MaxUlps		- Max # of ULPs permitted to still be considered equal.
	--*/
	template< typename FloatT, typename IntT >
	bool AlmostEqual2sComplement( 
		__in FloatT Lhs, 
		__in FloatT Rhs, 
		__in IntT MaxUlps
		)
	{
		C_ASSERT( sizeof( FloatT ) == sizeof( IntT ) );

		IntT LhsInt = *( IntT *) &Lhs;
		if ( LhsInt < 0 )
		{
			LhsInt = 0x80000000 - LhsInt;
		}

		IntT RhsInt = *( IntT* )&Rhs;
		if ( RhsInt < 0 )
		{
			RhsInt = 0x80000000 - RhsInt;
		}

		return _abs64( LhsInt - RhsInt ) <= MaxUlps;
	}

	struct Equal
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
 			return Lhs == Rhs;
		}

		template<>
		static bool Relate( 
			__in const float& Lhs, 
			__in const float& Rhs 
			)
		{
			return AlmostEqual2sComplement< float, LONG >( 
				Lhs, Rhs, CFIXCC_FLOAT_COMPARE_MAX_ULPS );
		}

		template<>
		static bool Relate( 
			__in const double& Lhs, 
			__in const double& Rhs 
			)
		{
			return AlmostEqual2sComplement< double, LONGLONG >( 
				Lhs, Rhs, CFIXCC_FLOAT_COMPARE_MAX_ULPS );
		}

		template<>
		static bool Relate( 
			__in const float& Lhs, 
			__in const double& Rhs 
			)
		{
			return AlmostEqual2sComplement< double, LONGLONG >( 
				Lhs, Rhs, CFIXCC_FLOAT_COMPARE_MAX_ULPS );
		}

		template<>
		static bool Relate( 
			__in const double& Lhs, 
			__in const float& Rhs 
			)
		{
			return AlmostEqual2sComplement< double, LONGLONG >( 
				Lhs, Rhs, CFIXCC_FLOAT_COMPARE_MAX_ULPS );
		}

		static std::wstring Mnemonic()
		{
			return L" == ";
		}
	};

	struct NotEqual
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
			return Lhs != Rhs;
		}

		static std::wstring Mnemonic()
		{
			return L" != ";
		}
	};

	struct Less
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
			return Lhs < Rhs;
		}

		static std::wstring Mnemonic()
		{
			return L" < ";
		}
	};

	struct Greater
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
			return Lhs > Rhs;
		}

		static std::wstring Mnemonic()
		{
			return L" > ";
		}
	};

	struct LessOrEqual
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
			return Lhs <= Rhs;
		}

		static std::wstring Mnemonic()
		{
			return L" <= ";
		}
	};

	struct GreaterOrEqual
	{
		template< typename ValueLhsT, typename ValueRhsT >
		static bool Relate( 
			__in const ValueLhsT& Lhs, 
			__in const ValueRhsT& Rhs 
			)
		{
			return Lhs >= Rhs;
		}

		static std::wstring Mnemonic()
		{
			return L" >= ";
		}
	};
}

/*----------------------------------------------------------------------
 *
 * Output Message Handling.
 *
 */

namespace cfixcc
{
	/*++
		Class Description:
			Encapsulates an (empty) assertion message.
	--*/
	class Message
	{
	public:
		Message()
		{
		}

		virtual PCWSTR GetMessage() const
		{
			return NULL;
		}

		virtual ~Message()
		{
		}

	private:
		Message( const Message& );
		const Message& operator=( const Message& );
	};

	/*++
		Class Description:
			Encapsulates a statically allocated assertion message.
	--*/
	class StaticMessage : public Message
	{
	private:
		PCWSTR MessageString;
		BOOL Allocated;

	public:
		StaticMessage( 
			__in PCWSTR String 
			) 
			: MessageString( String )
			, Allocated( FALSE )
		{
		}

		StaticMessage( 
			__in PCSTR AnsiString 
			) 
			: MessageString( NULL )
			, Allocated( TRUE )
		{
			size_t Len = strlen( AnsiString ) + 1;
			WCHAR* Buffer = new WCHAR[ Len ];
			if ( ! MultiByteToWideChar(
				CP_ACP,
				0,
				AnsiString,
				-1,
				Buffer,
				( int ) Len ) )
			{
				delete [] Buffer;
				MessageString = NULL;
			}
			else
			{
				MessageString = Buffer;
			}
		}

		virtual PCWSTR GetMessage() const
		{
			return MessageString;
		}

		virtual ~StaticMessage()
		{
			if ( Allocated )
			{
				delete [] MessageString;
			}
		}

	private:
		StaticMessage( const StaticMessage& );
		const StaticMessage& operator=( const StaticMessage& );
	};

	/*++
		Class Description:
			Encapsulates an assertion message that may be created
			using printf-like formatting. 
	--*/
	class FormattedMessage : public Message
	{
	private:
		FormattedMessage( const FormattedMessage& );
		const FormattedMessage& operator=( const FormattedMessage& );

	protected:
		WCHAR MessageString[ 256 ];

		void Initialize( 
			__in __format_string PCWSTR Format,
			__in va_list Lst
			)
		{
			( VOID ) StringCchVPrintfW(
				MessageString, 
				_countof( MessageString ),
				Format,
				Lst );
		}

		void Initialize( 
			__in __format_string PCSTR Format,
			__in va_list Lst
			)
		{
			CHAR AnsiString[ 256 ];

			//
			// Create ANSI string first, as some of the arguments
			// may be ANSI.
			//
			( VOID ) StringCchVPrintfA(
				AnsiString, 
				_countof( AnsiString ),
				Format,
				Lst );

			//
			// Convert.
			//
			if ( ! MultiByteToWideChar(
				CP_ACP,
				0,
				AnsiString,
				-1,
				MessageString,
				_countof( MessageString ) ) )
			{
				( VOID ) StringCchCopyW(
					MessageString,
					_countof( MessageString ),
					L"(Invalid string)" );
			}
		}

		FormattedMessage()
		{
		}

	public:
		FormattedMessage( 
			__in __format_string PCWSTR Format,
			... 
			)
		{
			va_list Lst;
			va_start( Lst, Format );
			Initialize( Format, Lst );
			va_end( Lst );
		}

		FormattedMessage( 
			__in __format_string PCSTR Format,
			... 
			)
		{
			va_list Lst;
			va_start( Lst, Format );
			Initialize( Format, Lst );
			va_end( Lst );
		}

		FormattedMessage( 
			__in __format_string PCWSTR Format,
			__in va_list Lst
			)
		{
			Initialize( Format, Lst );
		}

		FormattedMessage( 
			__in __format_string PCSTR Format,
			__in va_list Lst
			)
		{
			Initialize( Format, Lst );
		}

		virtual PCWSTR GetMessage() const
		{
			return MessageString;
		}
	};

}

/*----------------------------------------------------------------------
 *
 * Assertions.
 *
 */

namespace cfixcc
{
	template< typename RelateT >
	class Assertion
	{
	private:
		template< typename ValueLhsT, typename ValueRhsT >
		static CFIX_REPORT_DISPOSITION Fail(
			__in const ValueLhsT& Lhs,
			__in const ValueRhsT& Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			std::wostringstream Stream;
			if ( Msg.GetMessage() != NULL )
			{
				Stream << Msg.GetMessage() << L": ";
			}

			Stream << L"["
				   << cfixcc::ValueWrapper< ValueLhsT >( Lhs )	
				   << L"]"
				   << RelateT::Mnemonic()
				   << L"["
				   << cfixcc::ValueWrapper< ValueRhsT >( Rhs ) 
				   << L"]"
				   << L" (Expression: "
				   << ExpressionLhs 
				   << RelateT::Mnemonic() 
				   << ExpressionRhs << L")";

			return CfixPeReportFailedAssertion(
				File,
				Routine,
				Line,
				Stream.str().c_str() );
		}

	public:
		template< typename ValueT >
		static CFIX_REPORT_DISPOSITION Relate(
			__in_opt const ValueT& Lhs,
			__in_opt const ValueT& Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			if ( RelateT::Relate( Lhs, Rhs ) )
			{
				return CfixContinue;
			}
			else
			{
				return Fail(
					Lhs,
					Rhs,
					ExpressionLhs,
					ExpressionRhs,
					Msg,
					File,
					Routine,
					Line );
			}
		}

		template< typename ValueLhsT, typename ValueRhsT >
		static CFIX_REPORT_DISPOSITION RelateCompatible(
			__in_opt const ValueLhsT& Lhs,
			__in_opt const ValueRhsT& Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			if ( RelateT::Relate( Lhs, Rhs ) )
			{
				return CfixContinue;
			}
			else
			{
				return Fail(
					Lhs,
					Rhs,
					ExpressionLhs,
					ExpressionRhs,
					Msg,
					File,
					Routine,
					Line );
			}
		}

		static CFIX_REPORT_DISPOSITION Relate(
			__in_opt PCWSTR Lhs,
			__in_opt PCWSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			if ( Lhs == NULL || Rhs == NULL )
			{
				return Fail(
					Lhs,
					Rhs,
					ExpressionLhs,
					ExpressionRhs,
					Msg,
					File,
					Routine,
					Line );
			}

			return Relate(
				std::wstring( Lhs ),
				std::wstring( Rhs ),
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}

		static CFIX_REPORT_DISPOSITION Relate(
			__in_opt PCSTR Lhs,
			__in_opt PCSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			if ( Lhs == NULL || Rhs == NULL )
			{
				return Fail(
					Lhs,
					Rhs,
					ExpressionLhs,
					ExpressionRhs,
					Msg,
					File,
					Routine,
					Line );
			}
			return Relate(
				std::string( Lhs ),
				std::string( Rhs ),
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}

		static CFIX_REPORT_DISPOSITION RelateStrings(
			__in_opt PCWSTR Lhs,
			__in_opt PCWSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			return Relate(
				( PCWSTR ) Lhs,
				( PCWSTR ) Rhs,
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}

		static CFIX_REPORT_DISPOSITION RelateStrings(
			__in_opt PCSTR Lhs,
			__in_opt PCSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			return Relate(
				( PCSTR ) Lhs,
				( PCSTR ) Rhs,
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}

		static CFIX_REPORT_DISPOSITION Relate(
			__in_opt PWSTR Lhs,
			__in_opt PWSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			return Relate(
				( PCWSTR ) Lhs,
				( PCWSTR ) Rhs,
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}

		static CFIX_REPORT_DISPOSITION Relate(
			__in_opt PSTR Lhs,
			__in_opt PSTR Rhs,
			__in PCWSTR ExpressionLhs,
			__in PCWSTR ExpressionRhs,
			__in const Message& Msg,
			__in PCWSTR File,
			__in PCWSTR Routine,
			__in ULONG Line 
			)
		{
			return Relate(
				( PCSTR ) Lhs,
				( PCSTR ) Rhs,
				ExpressionLhs,
				ExpressionRhs,
				Msg,
				File,
				Routine,
				Line );
		}
	};
}

#define __CFIXCC_ASSERT_RELATION( RelateT, Expected, Actual, Message )	\
	( void ) ( ( CfixBreak != cfixcc::Assertion< RelateT >::Relate(		\
			Expected,													\
			Actual,														\
			__CFIX_WIDE( #Expected ),									\
			__CFIX_WIDE( #Actual ),										\
			Message,													\
			__CFIX_WIDE( __FILE__ ),									\
			__CFIX_WIDE( __FUNCTION__ ),								\
			__LINE__ ) ||												\
		( __debugbreak(), CfixPeFail(), 0 ) ) )

//
// Default macros.
//
#define CFIXCC_ASSERT_EQUALS( Expected, Actual )						\
	__CFIXCC_ASSERT_RELATION( cfixcc::Equal, Expected, Actual,			\
		cfixcc::Message() )

#define CFIXCC_ASSERT_NOT_EQUALS( Expected, Actual )					\
	__CFIXCC_ASSERT_RELATION( cfixcc::NotEqual, Expected, Actual,		\
		cfixcc::Message() )

#define CFIXCC_ASSERT_LESS( Expected, Actual )							\
	__CFIXCC_ASSERT_RELATION( cfixcc::Less, Expected, Actual,			\
		cfixcc::Message() )

#define CFIXCC_ASSERT_GREATER( Expected, Actual )						\
	__CFIXCC_ASSERT_RELATION( cfixcc::Greater, Expected, Actual,		\
		cfixcc::Message() )

#define CFIXCC_ASSERT_LESS_OR_EQUAL( Expected, Actual )					\
	__CFIXCC_ASSERT_RELATION( cfixcc::LessOrEqual, Expected, Actual,	\
		cfixcc::Message() )

#define CFIXCC_ASSERT_GREATER_OR_EQUAL( Expected, Actual )				\
	__CFIXCC_ASSERT_RELATION( cfixcc::GreaterOrEqual, Expected, Actual, \
		cfixcc::Message() )

//
// Macros with message.
//
// N.B. Variadic macros are supported as of cl 14 only.
//
#if _MSC_VER >= 1400 && !defined( CFIXCC_NO_VARIADIC_ASSERTIONS )
#define CFIXCC_ASSERT_EQUALS_MESSAGE( Expected, Actual, Message, ... )			\
	__CFIXCC_ASSERT_RELATION( cfixcc::Equal, Expected, Actual,					\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#define CFIXCC_ASSERT_NOT_EQUALS_MESSAGE( Expected, Actual, Message, ... )		\
	__CFIXCC_ASSERT_RELATION( cfixcc::NotEqual, Expected, Actual,				\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#define CFIXCC_ASSERT_LESS_MESSAGE( Expected, Actual, Message, ... )			\
	__CFIXCC_ASSERT_RELATION( cfixcc::Less, Expected, Actual,					\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#define CFIXCC_ASSERT_GREATER_MESSAGE( Expected, Actual, Message, ... )			\
	__CFIXCC_ASSERT_RELATION( cfixcc::Greater, Expected, Actual,				\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#define CFIXCC_ASSERT_LESS_OR_EQUAL_MESSAGE( Expected, Actual, Message, ... )	\
	__CFIXCC_ASSERT_RELATION( cfixcc::LessOrEqual, Expected, Actual,			\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#define CFIXCC_ASSERT_GREATER_OR_EQUAL_MESSAGE( Expected, Actual, Message, ... )\
	__CFIXCC_ASSERT_RELATION( cfixcc::GreaterOrEqual, Expected, Actual,		\
		cfixcc::FormattedMessage( Message, __VA_ARGS__ ) )

#else	// downlevel cl
#define CFIXCC_ASSERT_EQUALS_MESSAGE( Expected, Actual, Message )				\
	__CFIXCC_ASSERT_RELATION( cfixcc::Equal, Expected, Actual,					\
		cfixcc::StaticMessage( Message ) )

#define CFIXCC_ASSERT_NOT_EQUALS_MESSAGE( Expected, Actual, Message )			\
	__CFIXCC_ASSERT_RELATION( cfixcc::NotEqual, Expected, Actual,				\
		cfixcc::StaticMessage( Message ) )

#define CFIXCC_ASSERT_LESS_MESSAGE( Expected, Actual, Message )					\
	__CFIXCC_ASSERT_RELATION( cfixcc::Less, Expected, Actual,					\
		cfixcc::StaticMessage( Message ) )

#define CFIXCC_ASSERT_GREATER_MESSAGE( Expected, Actual, Message )				\
	__CFIXCC_ASSERT_RELATION( cfixcc::Greater, Expected, Actual,				\
		cfixcc::StaticMessage( Message ) )

#define CFIXCC_ASSERT_LESS_OR_EQUAL_MESSAGE( Expected, Actual, Message )		\
	__CFIXCC_ASSERT_RELATION( cfixcc::LessOrEqual, Expected, Actual,			\
		cfixcc::StaticMessage( Message ) )

#define CFIXCC_ASSERT_GREATER_OR_EQUAL_MESSAGE( Expected, Actual, Message )		\
	__CFIXCC_ASSERT_RELATION( cfixcc::GreaterOrEqual, Expected, Actual,			\
		cfixcc::StaticMessage( Message ) )
#endif

/*----------------------------------------------------------------------
 *
 * Logging helpers.
 *
 */

#ifndef CFIXCC_NO_LOG_CONVERSION

inline void CfixPeReportLogA(
	 __in const std::string& Message 
	 )
{
	CfixPeReportLogA( "%s", Message.c_str() );
}

inline void CfixPeReportLogA(
	 __in const std::wstring& Message 
	 )
{
	CfixPeReportLog( L"%s", Message.c_str() );
}

inline void CfixPeReportLog(
	 __in const std::string& Message 
	 )
{
	CfixPeReportLogA( "%s", Message.c_str() );
}

inline void CfixPeReportLog(
	 __in const std::wstring& Message 
	 )
{
	CfixPeReportLog( L"%s", Message.c_str() );
}

inline void CfixPeReportLogA(
	 __in int Numeric
	 )
{
	//
	// This is a CFIX_LOG( NULL ).
	//
	UNREFERENCED_PARAMETER( Numeric );
}

inline void CfixPeReportLog(
	 __in int Numeric
	 )
{
	//
	// This is a CFIX_LOG( NULL ).
	//
	UNREFERENCED_PARAMETER( Numeric );
}

#ifdef UNICODE
inline void CfixPeReportLog(
	 __in __format_string PCSTR Format,
	 ...
	 )
#else  // UNICODE
inline void CfixPeReportLogA(
	 __in __format_string PCWSTR Format,
	 ...
	 )
#endif // UNICODE
{
	va_list Lst;
	va_start( Lst, Format );
	CfixPeReportLog( L"%s", cfixcc::FormattedMessage( Format, Lst ).GetMessage() );
	va_end( Lst );
}

#endif // CFIXCC_NO_LOG_CONVERSION

//
// Convenience.
//
#define CFIXCC_LOG				CFIX_LOG
#define CFIXCC_ASSERT			CFIX_ASSERT
#define CFIXCC_ASSERT_MESSAGE	CFIX_ASSERT_MESSAGE

/*----------------------------------------------------------------------
 *
 * Test Routine Wrappers.
 *
 */

namespace cfixcc
{
#pragma warning( push )
#pragma warning( disable: 4702 )	// unreachable code

	template< 
		typename ExceptionT, 
		CFIX_PE_TESTCASE_ROUTINE RoutineT >
	inline void __stdcall ExpectExceptionTestRoutineWrapper()
	{
		try
		{
			RoutineT();

			//
			// Expected exception has not been raised.
			//
			if ( CfixBreak == CfixPeReportFailedAssertion(
				NULL, 
				NULL, 
				0, 
				L"Expected exception, but none has been raised" ) )
			{
				__debugbreak();
			}
		}
		catch ( ExceptionT& )
		{
			//
			// Expected.
			//
		}
	}
#pragma warning( pop )
}

/*++
	Macro Description:
		Enhanced CFIX_FIXTURE_ENTRY that checks that a specific
		C++ exception is thrown.
--*/
#define CFIXCC_FIXTURE_ENTRY_EXPECT_CPP_EXCEPTION( Routine, Exception )	\
	{ CfixEntryTypeTestcase,											\
	  __CFIX_WIDE( #Routine ),											\
	  cfixcc::ExpectExceptionTestRoutineWrapper<						\
			Exception,													\
			Routine > },


/*----------------------------------------------------------------------
 *
 * Test class.
 *
 */

namespace cfixcc
{
	class TestFixture
	{
	public:
		TestFixture() {}

		/*++
			Method Description:
				Set up fixture - called before any of the test methods
				is called. Called once for entire fixture.

				May be redefined by test class.
		--*/
		static void SetUp() {}

		/*++
			Method Description:
				Tear down fixture - called after all of the test methods
				have completed. Called once for entire fixture.

				May be redefined by test class.
		--*/
		static void TearDown() {}

		/*++
			Method Description:
				Called before each invokation of a test method, but after
				SetUp has been called. 

				May be overridden by test class.
		--*/
		virtual void Before() {}

		/*++
			Method Description:
				Called after each invokation of a test method, but before
				TearDown is called. 

				May be overridden by test class.
		--*/
		virtual void After() {}

		/*++
			Method Description:
				Destructor. It is advisable not to override the destructor
				but rather override the teardown routine.

				In particular, the destructor should not make use of 
				CFIX_ASSERT* routines, as these routines may throw
				exceptions and can thus interfere with proper object
				cleanup.
		--*/
		virtual ~TestFixture() {}

	private:
		//
		// Fixture classes should never be copied.
		//
		TestFixture( const TestFixture& );
		const TestFixture& operator = ( const TestFixture& );
	};

	/*++
		Routine Description:
			Adapter for SetUp-method.
	--*/
	template< class Cls >
	void __stdcall InvokeSetUp()
	{
		Cls::SetUp();
	}

	/*++
		Routine Description:
			Adapter for TearDown-method.
	--*/
	template< class Cls >
	void __stdcall InvokeTearDown()
	{
		Cls::TearDown();
	}

	/*++
		Routine Description:
			Adapter for Before-method.
	--*/
	template< class Cls >
	void __stdcall InvokeBefore()
	{
		Cls* TestObject = new Cls();
		CfixPeSetValue( CFIX_TAG_RESERVED_FOR_CC, TestObject );
		TestObject->Before();
	}

	/*++
		Routine Description:
			Adapter for After-method.
	--*/
	template< class Cls >
	void __stdcall InvokeAfter()
	{
		Cls* TestObject = static_cast< Cls* >(
			CfixPeGetValue( CFIX_TAG_RESERVED_FOR_CC ) );
		TestObject->After();
		delete TestObject;
	}

	/*++
		Routine Description:
			Adapter for test methods.
	--*/
	template< class Cls, void ( Cls::*Method )() >
	void __stdcall InvokeTestMethod()
	{
		Cls* TestObject = static_cast< Cls* >(
			CfixPeGetValue( CFIX_TAG_RESERVED_FOR_CC ) );
		( TestObject->*Method )();
	}
}

/*++
	Macros for testclass declaration. Usage is as follows:

		class MyFixture : public TestFixture
		{
		public:
			void Method01() {}
			void Method02() {}
		};

		CFIXCC_BEGIN_CLASS( MyFixture )
			CFIXCC_METHOD( Method01 )
			CFIXCC_METHOD( Method02 )
		CFIXCC_END_CLASS()
--*/
#define CFIXCC_BEGIN_CLASS( Class )										\
EXTERN_C __declspec(dllexport)											\
PCFIX_TEST_PE_DEFINITION CFIXCALLTYPE __CfixFixturePe##Class()			\
{																		\
	typedef Class __TestClass;											\
	static ULONG ApiVersion = CFIX_PE_API_MAKEAPIVERSION(				\
		CfixApiTypeCc, 0 );												\
	static CFIX_PE_DEFINITION_ENTRY Entries[] = {						\
	{ CfixEntryTypeSetup, L"SetUp",										\
		cfixcc::InvokeSetUp< __TestClass > },							\
	{ CfixEntryTypeTeardown, L"TearDown",								\
		cfixcc::InvokeTearDown< __TestClass > },						\
	{ CfixEntryTypeBefore, L"Before",									\
		cfixcc::InvokeBefore< __TestClass > },							\
	{ CfixEntryTypeAfter, L"After",										\
		cfixcc::InvokeAfter< __TestClass > },							


#if ! defined( CFIX_KERNELMODE )
#define CFIXCC_BEGIN_CLASS_EX( Class, Flags )							\
EXTERN_C __declspec(dllexport)											\
PCFIX_TEST_PE_DEFINITION CFIXCALLTYPE __CfixFixturePe##Class()			\
{																		\
	typedef Class __TestClass;											\
	static ULONG ApiVersion = CFIX_PE_API_MAKEAPIVERSION_EX(			\
		CfixApiTypeCc, 0, ( Flags ) );									\
	static CFIX_PE_DEFINITION_ENTRY Entries[] = {						\
	{ CfixEntryTypeSetup, L"SetUp",										\
		cfixcc::InvokeSetUp< __TestClass > },							\
	{ CfixEntryTypeTeardown, L"TearDown",								\
		cfixcc::InvokeTearDown< __TestClass > },						\
	{ CfixEntryTypeBefore, L"Before",									\
		cfixcc::InvokeBefore< __TestClass > },							\
	{ CfixEntryTypeAfter, L"After",										\
		cfixcc::InvokeAfter< __TestClass > },							
#endif // CFIX_KERNELMODE



#define CFIXCC_METHOD( MethodName )										\
	{ CfixEntryTypeTestcase, __CFIX_WIDE( #MethodName ),				\
		cfixcc::InvokeTestMethod< __TestClass, &__TestClass::MethodName > },			

#define CFIXCC_METHOD_EXPECT_EXCEPTION( MethodName, Exception )			\
	{ CfixEntryTypeTestcase, __CFIX_WIDE( #MethodName ),				\
		cfixcc::ExpectExceptionTestRoutineWrapper<						\
			Exception,													\
			cfixcc::InvokeTestMethod< __TestClass, &__TestClass::MethodName > > },			

#define CFIXCC_END_CLASS()												\
	{ CfixEntryTypeEnd, NULL, NULL }									\
	};																	\
	static CFIX_TEST_PE_DEFINITION Fixture = {							\
		ApiVersion,														\
		Entries															\
	};																	\
	CFIX_CALL_CRT_INIT_EMBEDDING_REGISTRATION();						\
	return &Fixture;													\
}			

#endif // __cplusplus