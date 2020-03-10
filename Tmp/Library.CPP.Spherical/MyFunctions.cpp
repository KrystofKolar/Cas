#include "stdafx.h"
#include "MyFunctions.h"

using namespace std;

LPWSTR ConvertString(const std::string& instr)
{
	// Assumes std::string is encoded in the current Windows ANSI codepage
	int bufferlen = ::MultiByteToWideChar(CP_ACP, 0, instr.c_str(), (int)instr.size(), NULL, 0);

	if (bufferlen == 0)
	{
		// Something went wrong. Perhaps, check GetLastError() and log.
		return 0;
	}

	// Allocate new LPWSTR - must deallocate it later
	LPWSTR widestr = new WCHAR[bufferlen + 1];

	::MultiByteToWideChar(CP_ACP, 0, instr.c_str(), (int)instr.size(), widestr, bufferlen);

	// Ensure wide string is null terminated
	widestr[bufferlen] = 0;

	// Do something with widestr
	return widestr;
	//delete[] widestr;
}

//extern "C"
//{
DECLDIR int Messaged(const char* str, int len, int* a, int alength) //LPCWSTR p_szMessage)
{
	size_t size = strlen(str) + 1;
	wchar_t *portname = new wchar_t[size];
	size_t outsize;

	mbstowcs_s(&outsize, portname, size, str, size - 1);

	MessageBox(NULL, portname, L"TEST 2", MB_YESNOCANCEL);

	//delete[] a;
	return a[2];
}
//}

