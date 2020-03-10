#include "stdafx.h"
#include "Unmanaged.h"


int Unmanaged::Hello(void) 
{
	return MessageBox(NULL, L"Unmanaged Text", L"Unmanaged Caption", MB_OK);
}

int Unmanaged::Calculate(void)
{
	return 1;
}
