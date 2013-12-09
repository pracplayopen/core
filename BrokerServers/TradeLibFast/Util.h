#pragma once
#include <vector>
#include <bitset>
using namespace std;


	bool isAuthorized(CString url, CString key, bool appendrand);
	
	CString GetURL(CString url);
	CString GetPublicIP(void);

double unpack(long i);
void gsplit(CString msg, CString del, vector<CString>& rec);
CString gjoin(vector<CString>& vec, CString del);
void TLTimeNow(vector<int> & nowtime);
char* cleansvnrev(const char * dirtyrev);
CString SerializeIntVec(std::vector<int> input);
CString UniqueWindowName(CString rootname);


unsigned charsplit(const char* &tosplit, vector<const char*> &parts, const char* delimiters);
const char* charjoin(std::vector<const char*>& vec, const char* del);

// 16 size must match TLTick::MAX_SYM_LENGTH
void symcp(char (& dest)[28], const char * source);
// 8 size must match TLTick::MAX_EX_LENGTH
void excp(char (& dest)[8], const char * source);
bool isstrsame(const char* s1, const char* s2);

void InstallFaultHandler();
void RevertFaultHandler();


enum TLTimeField
{
	TLdate,
	TLtime,
};

class CTokenizer
{
public:
	CTokenizer(const CString& cs, const CString& csDelim);
	void SetDelimiters(const CString& csDelim);

	bool Next(CString& cs);
	CString	Tail() const;

private:
	CString m_cs;
	std::bitset<256> m_delim;
	int m_nCurPos;
};