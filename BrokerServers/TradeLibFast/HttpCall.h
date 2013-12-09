#if !defined(MAB_HTTP_CALL_INCLUDED_)
#define MAB_HTTP_CALL_INCLUDED_


#include <afxinet.h>	// MFC Internet Connectivity Classes
#include <afxsock.h>	// MFC socket extensions

class CHttpSession : public CInternetSession
{
// Construction
public:
	CHttpSession(LPCTSTR szAgentName,
		DWORD dwContext,
		DWORD dwAccessType);

	virtual ~CHttpSession();
    //CommonConstruct();

// Operations
public:
    void SetStatus(LPCTSTR fmt, ...);
    void SetStatusWnd(CWnd* pWnd)     { m_pStatusWnd = pWnd; }

// Overrides
public:
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CHttpSession)
	//}}AFX_VIRTUAL
    virtual void OnStatusCallback(DWORD dwContext, DWORD dwInternetStatus, 
                                  LPVOID lpvStatusInformation, 
                                  DWORD dwStatusInformationLength);

	// Generated message map functions
	//{{AFX_MSG(CHttpSession)
	//}}AFX_MSG

// Attributes
protected:
    CWnd* m_pStatusWnd;
};

CString EnCodeStr(CString ToCode);
CString DeCodeStr(CString ToCode);
void DeCodeFile(CFile* DataFile, CHttpFile* HttpFile);
CString GetTheUserName();
int HttpCall(const char* ServerAddress,
			 const char* QueryString,
			 CString* Content);
#endif