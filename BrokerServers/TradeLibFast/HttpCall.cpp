#include "stdafx.h"
#include "HttpCall.h"

CString EnCodeStr(CString ToCode)
{
	CString RetStr,AddStr;
	int i,max;
	unsigned short asc;
	unsigned char c;
	max = (unsigned int)ToCode.GetLength();
	for(i=0;i<max;i++)
	{
		c = ToCode[i];
		asc = c;//(unsigned int)c;
		if(asc>47 && asc<58)
		{
			RetStr+=c;//Interim[(int)i];
		}
		else if(asc>64 && asc<91)
		{
			RetStr+=c;//Interim[(int)i];
		}
		else if(asc>96 && asc<123)
		{
			RetStr+=c;//Interim[(int)i];
		}
		else if(asc==32)
		{
			RetStr+="+";
		}
		else
		{
			AddStr.Format("%%%2x",asc);
			int iv = (int)AddStr.GetAt(1);
			if((int)AddStr.GetAt(1)==32)
			{
				AddStr.SetAt(1,'0');
			}
			RetStr+=AddStr;
		}
	}
	return RetStr;
}

CString DeCodeStr(CString ToCode)
{
	CString RetStr,AddStr;
	int i,max;
	unsigned short asc;
	unsigned char c;
	max = (unsigned int)ToCode.GetLength();
	for(i=0;i<max;)
	{
		c = ToCode[i];
		asc = c;//(unsigned int)c;
		if(asc==37)
		{
			AddStr=ToCode.Mid(i+1,2);
			i+=3;
			sscanf((LPCTSTR)AddStr,"%2x",&asc);
			RetStr+=(char)asc;
		}
		else if(asc==43)
		{
			RetStr += ' ';
			i++;
		}
		else
		{
			RetStr += c;
			i++;
		}
	}
	return RetStr;
}

void DeCodeFile(CFile* DataFile, CHttpFile* HttpFile)
{
	CString AddStr;
	int i,max;
	unsigned short asc;
	unsigned char c;
	max = (unsigned int)HttpFile->GetLength();
//	HttpFile->SeekToBegin();
	for(i=0;i<max;i++)
	{
		HttpFile->Read(&c,1);
		asc = c;
		if(asc==37)
		{
			HttpFile->Read(AddStr.GetBuffer(2),2);
			AddStr.ReleaseBuffer();
			sscanf((LPCTSTR)AddStr,"%2x",&asc);
			DataFile->Write(&asc,1);
		}
		else if(asc==43)
		{
			c=(char)32;
			DataFile->Write(&c,1);
		}
		else
		{
			DataFile->Write(&c,1);
		}
	}
	return;
}

CString GetTheUserName()
{ 
	int status;
	char lpName[255];
	unsigned long lpnLength = 255;
	// Get the log-on name of the person using product
	status = GetUserName(lpName, &lpnLength);
	return( lpName );
}

int HttpCall(const char* ServerAddress,
			 const char* QueryString,
			 CString* Content)
{
	long lread,max;
	// No Need to parse URL we create entities themselves.
	DWORD ServiceType = INTERNET_SERVICE_HTTP;

	CString lf;
	lf.Format("%c%c",char(13),char(10)); // need unix line feed

	//CString EnCodeData = EnCodeStr(DataBlock);

	//
 	// Setup Server
	DWORD AccessType = PRE_CONFIG_INTERNET_ACCESS;

		CString UserDef = "tradelink.org::brokerservers::util::geturl";	// User Application
	
	CHttpSession* pHttpSession = new CHttpSession(UserDef,1,INTERNET_OPEN_TYPE_PRECONFIG);

	CHttpConnection* pHttpConnection = NULL;
	pHttpConnection = pHttpSession->GetHttpConnection(ServerAddress);
	
	CHttpFile* pHttpFile = NULL;
	DWORD HttpRequestFlags;
	CString Query;
	
	Query = QueryString;
	//if(Query.GetLength()>0) Process += '?'+EnCodeStr(Query);
	// Open the request and send it;
	try	// if the request has an invalid port it fails. Need to look for option for error test
	{
		HttpRequestFlags = INTERNET_FLAG_EXISTING_CONNECT | INTERNET_FLAG_RELOAD | INTERNET_FLAG_DONT_CACHE;


		DWORD TotalLen;
		TotalLen = 0;
		if(TotalLen>0)
		{
			pHttpFile = pHttpConnection->OpenRequest(CHttpConnection::HTTP_VERB_POST,
				Query, NULL, 1, NULL, (LPCTSTR)"1.1", HttpRequestFlags);
		}
		else
		{
			pHttpFile = pHttpConnection->OpenRequest(CHttpConnection::HTTP_VERB_GET,
				Query, NULL, 1, NULL, (LPCTSTR)"1.1", HttpRequestFlags);
		}
		// Use direct write to posting field!
		//CString strHeaders = "Accept: text/*\r\n";
		//strHeaders += "User-Agent: HttpCall\r\n";
		//strHeaders += "Accept-Language: en-us\r\n";

		//pHttpFile->AddRequestHeaders((LPCSTR)strHeaders);


		if(TotalLen>0)
		{
			pHttpFile->SendRequestEx(TotalLen,HSR_INITIATE,1);
			//pHttpFile->WriteString((LPCTSTR)EnCodeData);
			pHttpFile->EndRequest();
		}
		else
		{
			pHttpFile->SendRequest();
		}
		max = pHttpFile->GetLength();
	}
	catch (CException* ex)
	{
		CString err("");
		char* buf = err.GetBuffer(1024);
		if (!ex->GetErrorMessage(buf,1024,NULL))
		{
			err = CString("");
		}
		Content->Append(err);
	}
	DWORD dwRet;
	pHttpFile->QueryInfoStatusCode(dwRet);
	//
	//Check Return Code.
	//DataFile->SeekToBegin();
	// Read Data
	CString strRetBufLen;
	pHttpFile->QueryInfo(HTTP_QUERY_CONTENT_LENGTH, strRetBufLen);
	max = atol((LPCSTR)strRetBufLen);
	if(max<=0)
	{
		max = pHttpFile->GetLength();
	}
	// Read Data
	lread=1000;
	char c[1000];
	while(lread>0)
	{
		lread = pHttpFile->Read(c,1000);
		
		if(lread>0) 
		{
			//DataFile->Write(c,lread);
			Content->Append(c,1000);
		}
	}

	pHttpFile->Close();
	pHttpConnection->Close();
	pHttpSession->Close();
	delete pHttpFile;
	delete pHttpConnection;
	delete pHttpSession;
	return dwRet;
}

#define BUFFER_SIZE 4095

/////////////////////////////////////////////////////////////////////////////
// CHttpSession

CHttpSession::CHttpSession(LPCTSTR szAgentName = NULL, 
								 DWORD dwContext = 1, 
								 DWORD dwAccessType = INTERNET_OPEN_TYPE_PRECONFIG)//,
								 //LPCTSTR pstrProxyName = NULL, 
								 //LPCTSTR pstrProxyBypass = NULL, 
								 //DWORD dwFlags = 0 )
			:CInternetSession(szAgentName,
								dwContext, 
								dwAccessType, 
								NULL,//pstrProxyName, 
								NULL,//pstrProxyBypass, 
								0)//dwFlags) 
{
    m_pStatusWnd = NULL;
    try {
        EnableStatusCallback(TRUE);
    }
    catch (...)
    {}
}

CHttpSession::~CHttpSession()
{
}

/*
CHttpSession::CommonConstruct() 
{
    m_pStatusWnd = NULL;
    try {
        EnableStatusCallback(TRUE);
    }
    catch (...)
    {}
}*/
// Do not edit the following lines, which are needed by ClassWizard.
#if 0
BEGIN_MESSAGE_MAP(CHttpSession, CInternetSession)
	//{{AFX_MSG_MAP(CHttpSession)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
#endif	// 0

/////////////////////////////////////////////////////////////////////////////
// CHttpSession member functions

void CHttpSession::OnStatusCallback(DWORD dwContext, 
                                       DWORD dwInternetStatus, 
                                       LPVOID lpvStatusInformation, 
                                       DWORD dwStatusInformationLength)
{
    // Status callbacks need thread-state protection. 
    AFX_MANAGE_STATE( AfxGetAppModuleState( ) );

    CString str;

	switch (dwInternetStatus)
	{
	case INTERNET_STATUS_RESOLVING_NAME:
		str.Format("Resolving name for %s", lpvStatusInformation);
		break;

	case INTERNET_STATUS_NAME_RESOLVED:
		str.Format("Resolved name for %s", lpvStatusInformation);
		break;

	case INTERNET_STATUS_HANDLE_CREATED:
		//str.Format("Handle %8.8X created", hInternet);
		break;

	case INTERNET_STATUS_CONNECTING_TO_SERVER:
		{
		str.Format("Connecting to socket address "); 
		}
		break;

	case INTERNET_STATUS_REQUEST_SENT:
		str.Format("Request sent");
		break;

	case INTERNET_STATUS_SENDING_REQUEST:
		str.Format("Sending request...");
		break;

	case INTERNET_STATUS_CONNECTED_TO_SERVER:
		str.Format("Connected to socket address");
		break;

	case INTERNET_STATUS_RECEIVING_RESPONSE:
        return;
		str.Format("Receiving response...");
		break;

	case INTERNET_STATUS_RESPONSE_RECEIVED:
		str.Format("Response received");
		break;

	case INTERNET_STATUS_CLOSING_CONNECTION:
		str.Format("Closing the connection to the server");
		break;

	case INTERNET_STATUS_CONNECTION_CLOSED:
		str.Format("Connection to the server closed");
		break;

	case INTERNET_STATUS_HANDLE_CLOSING:
        return;
		str.Format("Handle closed");
		break;

	case INTERNET_STATUS_REQUEST_COMPLETE:
        // See the CInternetSession constructor for details on INTERNET_FLAG_ASYNC.
        // The lpvStatusInformation parameter points at an INTERNET_ASYNC_RESULT 
        // structure, and dwStatusInformationLength contains the final completion 
        // status of the asynchronous function. If this is ERROR_INTERNET_EXTENDED_ERROR, 
        // the application can retrieve the server error information by using the 
        // Win32 function InternetGetLastResponseInfo. See the ActiveX SDK for more 
        // information about this function. 
		if (dwStatusInformationLength == sizeof(INTERNET_ASYNC_RESULT))
		{
			INTERNET_ASYNC_RESULT* pResult = (INTERNET_ASYNC_RESULT*) lpvStatusInformation;
			str.Format("Request complete, dwResult = %8.8X, dwError = %8.8X",
				        pResult->dwResult, pResult->dwError);
		}
		else
			str.Format("Request complete");
		break;

	case INTERNET_STATUS_CTL_RESPONSE_RECEIVED:
	case INTERNET_STATUS_REDIRECT:
	default:
		str.Format("Unknown status: %d", dwInternetStatus);
		break;
	}

    SetStatus(str);

    TRACE("CHttpSession::OnStatusCallback: %s\n",str);
}

void CHttpSession::SetStatus(LPCTSTR fmt, ...)
{
    va_list args;
    TCHAR buffer[512];

    va_start(args, fmt);
    _vstprintf(buffer, fmt, args);
    va_end(args);

    TRACE1("CHttpSession::SetStatus: %s\n", buffer);

    if (m_pStatusWnd)
    {
        m_pStatusWnd->SetWindowText(buffer);
        m_pStatusWnd->RedrawWindow();
    }
}

