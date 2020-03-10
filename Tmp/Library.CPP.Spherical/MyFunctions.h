#pragma once

#include "stdafx.h"

#include <windows.h>
#include <string>

#if defined DLL_EXPORT
#define DECLDIR __declspec(dllexport) 
#else
#define DECLDIR __declspec(dllimport)

#endif

using namespace std;

extern "C" DECLDIR int Messaged(const char* str, int len, int* a, int alength); // LPCWSTR p_szMessage);
