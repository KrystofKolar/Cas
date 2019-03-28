

#if !defined(AFX_APPMONITOR_H__D0B1E941_6B5E_76FA_1E51_C463E6B70F48__INCLUDED_)
#define AFX_APPMONITOR_H__D0B1E941_6B5E_76FA_1E51_C463E6B70F48__INCLUDED_

#include "WinDef.h"
#include "afxwin.h" //CWinApp

//! Changes the child font, Global function called by enum childs
BOOL __stdcall SetChildFont(HWND hwnd, LPARAM lparam);

namespace Sample
{
namespace OptiReqMonitor
{

//! Opti Request Monitor Application
class CAppMonitor
    : public CWinApp
{
public:
    //{{AFX_VIRTUAL(CAppMonitor)            
    virtual BOOL InitInstance();
    //}}AFX_VIRTUAL            
};

}
}


#endif // !defined(AFX_APPMONITOR_H__D0B1E941_6B5E_76FA_1E51_C463E6B70F48__INCLUDED_)