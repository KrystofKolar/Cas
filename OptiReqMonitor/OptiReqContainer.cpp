
#include "SampleWinDef.h"

#include <afxwin.h>

#include "OptiReqContainer.h"

#include "SampleQueryParam.h"
#include "Sysmacro.h" //SAFEDELETEPTR
#include "Sampletemplates.h" //SortByMember
#include "Util01.h" //InRange
#include "defs.h" // Sample_MAX_FPFX ...
#include "oledbhelper.h"
#include <algorithm> // std::find_if, ...

#include "OptiReqItem.h"
#include "DefsSampleOptiReqMonitor.h"
#include "DbAppMonitor.h"

namespace Sample
{
namespace OptiReqMonitor
{

//! Helper for database retrieval, holds opti request data
class CDbOptiReqItem
{
public:
    enum eDbColSize
    {
        c_szOptiReqMode = 1 +1,
        c_szOptiReqCauser = 10 +1,            
        c_szFpfx = Sample_MAX_FPFX +1,
        c_szFsfx = Sample_MAX_FSFX +1,
        c_szCallSgn = 10 +1,

        c_szDepIcao = Sample_MAX_ICAO_APT +1,
        c_szDepIata = Sample_MAX_IATA_APT +1,

        c_szDesIcao = c_szDepIcao,
        c_szDesIata = c_szDepIata,
    };

    //! constructor
    CDbOptiReqItem()
    {
    };

    //! destructor
    virtual ~CDbOptiReqItem()
    {
    };

    //! optimizer request id
    long m_nOptiReqId;

    //! optimizer request insertion datetime
    DBTIMESTAMP m_dbdtOptiReqInserted;

    //! optimizer request processing datetime
    DBTIMESTAMP m_dbdtOptiReqProcessed; DBSTATUS m_dbdtOptiReqProcessed_Status;

    //! optimizer request waituntil datetime
    DBTIMESTAMP m_dbdtOptiReqWaitUntil; DBSTATUS m_dbdtOptiReqWaitUntil_Status;

    //! optimizer request mode
    TCHAR m_csOptiReqMode[c_szOptiReqMode];

    //! causer
    TCHAR m_csOptiReqCauser[c_szOptiReqCauser]; DBSTATUS m_csOptiReqCauser_Status; 

    //! flightdate
    DBTIMESTAMP m_dbdtFldt; DBSTATUS m_dbdtFldt_Status;

    //! flightprefix
    TCHAR m_csFpfx[c_szFpfx]; DBSTATUS m_csFpfx_Status;

    //! flightsuffix
    TCHAR m_csFsfx[c_szFpfx]; DBSTATUS m_csFsfx_Status;

    //! flightnumber
    int m_nFlnr; DBSTATUS m_nFlnr_Status;

    //! flightlegnr 
    int m_nLgnr; DBSTATUS m_nLgnr_Status;

    //! flight departure minute of day, relative flightdate
    int m_nStd; DBSTATUS m_nStd_Status;

    //! flight arrival minute of day, relative flightdate
    int m_nSta; DBSTATUS m_nSta_Status;

    //! flight callsign
    TCHAR m_csCallSgn[c_szCallSgn]; DBSTATUS m_csCallSgn_Status;

    //! flight departure icao
    TCHAR m_csDepIcao[c_szDepIcao]; DBSTATUS m_csDepIcao_Status;

    //! flight departure iata
    TCHAR m_csDepIata[c_szDepIata]; DBSTATUS m_csDepIata_Status;

    //! flight destination icao
    TCHAR m_csDesIcao[c_szDesIcao]; DBSTATUS m_csDesIcao_Status;

    //! flight destination iata
    TCHAR m_csDesIata[c_szDesIata]; DBSTATUS m_csDesIata_Status;
};

}
}

namespace
{

using namespace Sample::OptiReqMonitor;

bool 
HelperIsValidDBTIMESTAMP(DBTIMESTAMP const &dbdtStamp)
{
    DBTIMESTAMP dbdtMin;
    SetMinDBTIMESTAMP(dbdtMin);

    return CompareDBTIMESTAMP(dbdtStamp, dbdtMin) >0;
}


Sample::CSampleSystemTime 
_HelperGetAsSampleSystemTime(DBTIMESTAMP const &dbdtStamp)
{
    Sample::CSampleSystemTime fst; 
    fst.Invalidate(); //called in ctor

    if (!HelperIsValidDBTIMESTAMP(dbdtStamp))
    {
        return fst;
    }

    SYSTEMTIME st;

    if (DBTIMESTAMPToSystemTime(&dbdtStamp, &st))
    {
        fst = st;
    }

    return fst;
}

//! helper for dbbind
ULONG
_HelperDBBindingOffset(Sample::OptiReqMonitor::CDbOptiReqItem *p, void *p1)
{
    return static_cast<ULONG>(reinterpret_cast<char*>(p1) -reinterpret_cast<char*>(p));
}

//! helper datetime and nulls
void
_HelperNullValues(DBTIMESTAMP const &dbdt, DBSTATUS const &Status,  //source
                  Sample::CSampleSystemTime &fst, bool &bIsNull) // destination
{
    if (Status == DBSTATUS_S_ISNULL)
    {
        fst.Invalidate();
        bIsNull = true; 
    }
    else
    {
        fst = _HelperGetAsSampleSystemTime(dbdt);
        bIsNull = false;
    }
}

//! helper string and nulls
void
_HelperNullValues(TCHAR const * const cs, DBSTATUS const &Status,  //source
                  CSampleString &str, bool &bIsNull) // destination
{
    if (Status == DBSTATUS_S_ISNULL)
    {
        str = _T("");
        bIsNull = true; 
    }
    else
    {
        str = static_cast<LPCTSTR>(cs);
        bIsNull = false;
    }
}

//! helper int and nulls
void
_HelperNullValues(int const &n, DBSTATUS const &Status,  //source
                  int &m, bool &bIsNull) // destination
{
    if (Status == DBSTATUS_S_ISNULL)
    {
        m = 0;
        bIsNull = true; 
    }
    else
    {
        m = n;
        bIsNull = false;
    }
}

//! copy members from  CDbOptiReqItem to COptiReqItem
void 
_HelperDbItemToItem(Sample::OptiReqMonitor::CDbOptiReqItem const &DbItem, 
                         Sample::OptiReqMonitor::COptiReqItem &Item)
{
    Sample::CSampleSystemTime fstTmp;
    CSampleString sTmp;
    int nTmp;

    fstTmp.Invalidate();
    sTmp=_T("");
    nTmp=-1;

    Item.m_nOptiReqId = DbItem.m_nOptiReqId;
    Item.m_fstOptiReqInserted = _HelperGetAsSampleSystemTime(DbItem.m_dbdtOptiReqInserted);       
    _HelperNullValues(DbItem.m_dbdtOptiReqProcessed, DbItem.m_dbdtOptiReqProcessed_Status,
                      Item.m_fstOptiReqProcessed, Item.m_fstOptiReqProcessed_IsNull); 
    _HelperNullValues(DbItem.m_dbdtOptiReqWaitUntil, DbItem.m_dbdtOptiReqWaitUntil_Status,
                      Item.m_fstOptiReqWaitUntil, Item.m_fstOptiReqWaitUntil_IsNull); 
    sTmp = static_cast<LPCTSTR>(DbItem.m_csOptiReqMode);
		if (sTmp.empty() || 
			sTmp == _T(" ") || 
			sTmp == _T("  "))
		{
			Item.m_csOptiReqMode = g_Defs.g_csTokenWhitespace;
		}
		else
		{
			Item.m_csOptiReqMode = sTmp;
		}
    _HelperNullValues(DbItem.m_csOptiReqCauser, DbItem.m_csOptiReqCauser_Status,
                      Item.m_csOptiReqCauser, Item.m_csOptiReqCauser_IsNull);
    _HelperNullValues(DbItem.m_dbdtFldt, DbItem.m_dbdtFldt_Status,
                      Item.m_fstFldt, Item.m_fstFldt_IsNull);
    _HelperNullValues(DbItem.m_csFpfx, DbItem.m_csFpfx_Status,
                      Item.m_csFpfx, Item.m_csFpfx_IsNull);
    _HelperNullValues(DbItem.m_csFsfx, DbItem.m_csFsfx_Status,
                      Item.m_csFsfx, Item.m_csFsfx_IsNull);        
    _HelperNullValues(DbItem.m_nFlnr, DbItem.m_nFlnr_Status,
                      Item.m_nFlnr, Item.m_nFlnr_IsNull);
    _HelperNullValues(DbItem.m_nLgnr, DbItem.m_nLgnr_Status,
                      Item.m_nLgnr, Item.m_nLgnr_IsNull);
    _HelperNullValues(DbItem.m_nStd, DbItem.m_nStd_Status,
                      Item.m_nStd, Item.m_nStd_IsNull);
    _HelperNullValues(DbItem.m_nSta, DbItem.m_nSta_Status,
                      Item.m_nSta, Item.m_nSta_IsNull);
    _HelperNullValues(DbItem.m_csCallSgn, DbItem.m_csCallSgn_Status,
                      Item.m_csCallSgn, Item.m_csCallSgn_IsNull);           
    _HelperNullValues(DbItem.m_csDepIcao, DbItem.m_csDepIcao_Status,
                      Item.m_csDepIcao, Item.m_csDepIcao_IsNull);         
    _HelperNullValues(DbItem.m_csDepIata, DbItem.m_csDepIata_Status,
                      Item.m_csDepIata, Item.m_csDepIata_IsNull);          
    _HelperNullValues(DbItem.m_csDesIcao, DbItem.m_csDesIcao_Status,
                      Item.m_csDesIcao, Item.m_csDesIcao_IsNull);          
    _HelperNullValues(DbItem.m_csDesIata, DbItem.m_csDesIata_Status,
                      Item.m_csDesIata, Item.m_csDesIata_IsNull);  
    Item.m_MetaData.fstCreated.SetNow();

    Item.m_MetaData.bUseable = true;
} 

bool 
_HelperSortCausers(CSampleString &csLeft, CSampleString &csRight)
{
	return csLeft < csRight;
}

//! test if changes in filter params make sql query necessary
bool
_HelperFilterParams_IsSQLQueryNecessary(COptiReqContainer::FilterParams prevParams,
                                        COptiReqContainer::FilterParams newParams)
{
    if (!newParams.bUseable)
    {
        ASSERT(false); // a bug
        return false;
    }

    if (!prevParams.bUseable)
    {
        // no previous filter params exists
        return true;
    }
         
    if (newParams.v_csOptiReqModi.empty())
    {
        // no modi selected, no result possible - a sql query won't help
        return false;
    }
    
    // any timerange change will make sql query necessary                     
    return newParams.fstSvt != prevParams.fstSvt ||
		   newParams.fstEvt != prevParams.fstEvt;
}	

bool
_HelperFilterRequested(COptiReqContainer::FilterParams Filter, COptiReqItem const &Item)    
{
    bool bRequested = true;
    // maybe make all uppercase
    
    // fpfx
    if (!Filter.csFpfx.empty())
    {
        if (Item.GetFpfx().find(Filter.csFpfx.c_str()) == std::string::npos)
        {
            return false;
        }
    }

    //flnr
    if (Filter.nFlnr >0)
    {
        if(Item.GetFlnr() != Filter.nFlnr)
        {
            return false;
        }
    }

    //lgnr
    if (Filter.nLgnr >0)
    {
        if(Item.GetLgnr() != Filter.nLgnr)
        {
            return false;
        }
    }

    //fldt
    //opti request causer
    if (Filter.csOptiReqCauser != g_Defs.g_csTokenAny)
    {
        CSampleString csDbCauser = Item.GetOptiReqCauser().GlobalEraseWhitespace();

        if (csDbCauser.empty())
        {
            csDbCauser = g_Defs.g_csTokenEmpty.c_str();
        }
        else if (Item.GetOptiReqCauser_IsNull())
        {
            csDbCauser = g_Defs.g_csTokenNull;
        }

        if (Filter.csOptiReqCauser != csDbCauser)
        {
            return false;
        }
    }

    // Departure apt
    if (!Filter.csDepIcao.empty())
    {
        if (Item.GetDepIcao().find(Filter.csDepIcao.c_str()) == std::string::npos)
        {
            return false;
        }
    }

    // Destination apt        
    if (!Filter.csDesIcao.empty())
    {
        if (Item.GetDesIcao().find(Filter.csDesIcao.c_str()) == std::string::npos)
        {
            return false;
        }        
    }

    return bRequested;
}

}

namespace Sample
{
namespace OptiReqMonitor
{

COptiReqContainer::COptiReqContainer(CDbAppMonitor &DbAppMonitor)
    : m_DbAppMonitor(DbAppMonitor)
{
    m_Modi.Reload();
    m_nMaxItemCount = -1;
}

COptiReqContainer::~COptiReqContainer()
{
};

bool
COptiReqContainer::Reload(FilterParams Filter, bool bRefresh, CWnd *pWndMsgReceiver)
{
    bool bres = true;
    
    if (!m_DbAppMonitor.ConnectDatasourceRetry())
    {
	    ASSERT(false);
        return false;
    }

    bool const bMsgSendStatus = pWndMsgReceiver && ::IsWindow(pWndMsgReceiver->GetSafeHwnd());
    
    if (bRefresh || _HelperFilterParams_IsSQLQueryNecessary(m_prevFilter, Filter))
    {
        bMsgSendStatus && pWndMsgReceiver->SendMessage(g_Defs.m_nMsg_OptiReqContainerReload,
                                                       CDefsSampleOptiReqMonitor::eMsg_OptiReqContainerReload_ReloadDatabase);
    
        HACCESSOR hAccessor;
        Sample::CSampleQueryParam SQLQuery(g_Defs.g_csSp_opti_request_GetByTimerange.c_str(), Sample::CSampleQueryParam::E_SQLType_OLEDB);
        SQLQuery.AddParam(Filter.fstSvt);
        SQLQuery.AddParam(Filter.fstEvt);
        
        CRowset *pRowset = (m_DbAppMonitor.GetConnection())->GetRowset(SQLQuery.GetQuery().c_str());
        
        if(!pRowset)
        {
            return false;
        }
        
        CDbOptiReqItem DbItem;
        DBBINDING dbBind[COptiReqItem::c_Cnt];      
/*
        c_OptiReqId,
        c_OptiReqInserted,
        c_OptiReqProcessed,
        c_OptiReqWaitUntil,
        c_OptiReqMode,
        c_OptiReqCauser
*/
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqId], COptiReqItem::c_OptiReqId +1, DBPART_VALUE, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_nOptiReqId), 0, 0, DBTYPE_I4, sizeof(DbItem.m_nOptiReqId));
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqInserted], COptiReqItem::c_OptiReqInserted +1, DBPART_VALUE,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtOptiReqInserted), 0, 0, DBTYPE_DBTIMESTAMP, sizeof(DbItem.m_dbdtOptiReqInserted));
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqProcessed], COptiReqItem::c_OptiReqProcessed +1, DBPART_VALUE | DBPART_STATUS,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtOptiReqProcessed), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtOptiReqProcessed_Status), DBTYPE_DBTIMESTAMP, sizeof(DbItem.m_dbdtOptiReqProcessed));
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqWaitUntil], COptiReqItem::c_OptiReqWaitUntil +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtOptiReqWaitUntil), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtOptiReqWaitUntil_Status), DBTYPE_DBTIMESTAMP, sizeof(DbItem.m_dbdtOptiReqWaitUntil));
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqMode], COptiReqItem::c_OptiReqMode +1, DBPART_VALUE, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csOptiReqMode), 0, 0, DBTYPE_TSTR, sizeof(DbItem.m_csOptiReqMode));
        SetDBBinding(dbBind[COptiReqItem::c_OptiReqCauser], COptiReqItem::c_OptiReqCauser +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csOptiReqCauser), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csOptiReqCauser_Status), DBTYPE_TSTR, sizeof(DbItem.m_csOptiReqCauser));                                                  
/*
        c_Fldt,
        c_Fpfx,
        c_Fsfx,
        c_Flnr,
        c_Lgnr,
*/
        SetDBBinding(dbBind[COptiReqItem::c_Fldt], COptiReqItem::c_Fldt +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtFldt), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_dbdtFldt_Status), DBTYPE_DBTIMESTAMP, sizeof(DbItem.m_dbdtFldt));
        SetDBBinding(dbBind[COptiReqItem::c_Fpfx], COptiReqItem::c_Fpfx +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csFpfx), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csFpfx_Status), DBTYPE_TSTR, sizeof(DbItem.m_csFpfx));            
        SetDBBinding(dbBind[COptiReqItem::c_Fsfx], COptiReqItem::c_Fsfx +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csFsfx), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csFsfx_Status), DBTYPE_TSTR, sizeof(DbItem.m_csFsfx));                        
        SetDBBinding(dbBind[COptiReqItem::c_Flnr], COptiReqItem::c_Flnr +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_nFlnr), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_nFlnr_Status), DBTYPE_I4, sizeof(DbItem.m_nFlnr));
        SetDBBinding(dbBind[COptiReqItem::c_Lgnr], COptiReqItem::c_Lgnr +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_nLgnr), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_nLgnr_Status), DBTYPE_I4, sizeof(DbItem.m_nLgnr));
/*            
        c_Std,
        c_Sta,
*/
        SetDBBinding(dbBind[COptiReqItem::c_Std], COptiReqItem::c_Std +1, DBPART_VALUE | DBPART_STATUS, 
                   _HelperDBBindingOffset(&DbItem, &DbItem.m_nStd), 0, 
                   _HelperDBBindingOffset(&DbItem, &DbItem.m_nStd_Status), DBTYPE_I4, sizeof(DbItem.m_nStd));
        SetDBBinding(dbBind[COptiReqItem::c_Sta], COptiReqItem::c_Sta +1, DBPART_VALUE | DBPART_STATUS, 
                   _HelperDBBindingOffset(&DbItem, &DbItem.m_nSta), 0,
                   _HelperDBBindingOffset(&DbItem, &DbItem.m_nSta_Status), DBTYPE_I4, sizeof(DbItem.m_nSta));                       
/*                       
        c_CallSgn,
        c_DepIcao,    
        c_DepIata,
        c_DesIcao,
        c_DesIata,
*/
        SetDBBinding(dbBind[COptiReqItem::c_CallSgn], COptiReqItem::c_CallSgn +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csCallSgn), 0, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csCallSgn_Status), DBTYPE_TSTR, sizeof(DbItem.m_csCallSgn));
        SetDBBinding(dbBind[COptiReqItem::c_DepIcao], COptiReqItem::c_DepIcao +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDepIcao), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDepIcao_Status), DBTYPE_TSTR, sizeof(DbItem.m_csDepIcao));
        SetDBBinding(dbBind[COptiReqItem::c_DepIata], COptiReqItem::c_DepIata +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDepIata), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDepIata_Status), DBTYPE_TSTR, sizeof(DbItem.m_csDepIata));                       
        SetDBBinding(dbBind[COptiReqItem::c_DesIcao], COptiReqItem::c_DesIcao +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDesIcao), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDesIcao_Status), DBTYPE_TSTR, sizeof(DbItem.m_csDesIcao));
        SetDBBinding(dbBind[COptiReqItem::c_DesIata], COptiReqItem::c_DesIata +1, DBPART_VALUE | DBPART_STATUS, 
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDesIata), 0,
                    _HelperDBBindingOffset(&DbItem, &DbItem.m_csDesIata_Status), DBTYPE_TSTR, sizeof(DbItem.m_csDesIata));

        m_vItemsRef.clear();

        if(pRowset->Bind(dbBind, COptiReqItem::c_Cnt, hAccessor))
        {
            COptiReqItem Item;
            
            while(pRowset->NextRow())
            {
                if(pRowset->GetRowData(hAccessor, reinterpret_cast<void*>(&DbItem)))
                {
                    _HelperDbItemToItem(DbItem, Item);

                    m_vItemsRef.push_back(Item);
                    
                    if (m_nMaxItemCount > 0 && 
                        static_cast<int>(m_vItemsRef.size()) >= m_nMaxItemCount)
                    {
                        // limit result rowcount
                        break;
                    }
                }
            }
            pRowset->Unbind(hAccessor);
        }
        else
        {
            ASSERT(FALSE);
            bres = false;
        }
        
        SAFEDELETEPTR(pRowset);
    }

    m_vItemsFiltered.clear();
    m_vItemsRefCausers.clear();

    bMsgSendStatus && pWndMsgReceiver->SendMessage(g_Defs.m_nMsg_OptiReqContainerReload,
                                                   CDefsSampleOptiReqMonitor::eMsg_OptiReqContainerReload_Statistic_TotalItems,
                                                   static_cast<LPARAM>(m_vItemsRef.size()));    

    if (!Filter.v_csOptiReqModi.empty())
    {
        // any mode should be shown
        COptiReqItem Item;
        
        bool bShowUnknown = std::find(Filter.v_csOptiReqModi.begin(),
                                      Filter.v_csOptiReqModi.end(),
                                      g_Defs.g_csTokenModeUnknown)
                           != Filter.v_csOptiReqModi.end();
        
        for (VecOptiReqItems::const_iterator citer = m_vItemsRef.begin();
             citer != m_vItemsRef.end();
             ++citer)
        {
            Item = *citer;
            
            //  test for matching of Item mode and filter mode
            bool bFilterMode_Passed = std::find(Filter.v_csOptiReqModi.begin(), 
                                                Filter.v_csOptiReqModi.end(),
                                                Item.GetOptiReqMode().ToUpper()) 
                                      != Filter.v_csOptiReqModi.end();

            if (!bFilterMode_Passed && bShowUnknown)
            {
                // test if filter settings should show unknown modi
                // if so, let the item pass
                bool bUnknown = true;
                
                for(std::vector<CSampleOptiReqMode>::const_iterator citer = m_Modi.begin(); 
                    citer != m_Modi.end();
                    ++citer)
                {
                    if (Item.GetOptiReqMode().ToUpper() == citer->m_csMode)
                    {
                        bUnknown = false;
                        break;
                    }
                }
                
                bFilterMode_Passed = bUnknown;
                
            }
            
            if (bFilterMode_Passed)
            {
                if (Item.GetOptiReqCauser_IsNull())
                {
                    m_vItemsRefCausers.push_back(g_Defs.g_csTokenNull);
                }
                else if(Item.GetOptiReqCauser().GlobalEraseWhitespace().empty())
                {
                    m_vItemsRefCausers.push_back(g_Defs.g_csTokenEmpty);
                }
                else
                {
                    m_vItemsRefCausers.push_back(Item.GetOptiReqCauser());
                }
                
                if (_HelperFilterRequested(Filter, Item))
                {
                    m_vItemsFiltered.push_back(*citer);
                }
            }
        }
    }
    
    bMsgSendStatus && pWndMsgReceiver->SendMessage(g_Defs.m_nMsg_OptiReqContainerReload,
                                                   CDefsSampleOptiReqMonitor::eMsg_OptiReqContainerReload_ReloadGrid);
    
    // keep sorted, unique list of causers
    std::sort(m_vItemsRefCausers.begin(), m_vItemsRefCausers.end(), _HelperSortCausers);
    std::vector<CSampleString>::iterator iter_newEnd;
    iter_newEnd = std::unique(m_vItemsRefCausers.begin(), m_vItemsRefCausers.end());
    
    m_vItemsRefCausers.erase(iter_newEnd, m_vItemsRefCausers.end());

    m_prevFilter = Filter;

    bMsgSendStatus && pWndMsgReceiver->SendMessage(g_Defs.m_nMsg_OptiReqContainerReload,
                                                   CDefsSampleOptiReqMonitor::eMsg_OptiReqContainerReload_ReloadEnd);
          
    return bres;
}

bool
COptiReqContainer::SortRows(int col, bool bAsc)
{
    if (m_vItemsFiltered.size() <1 )
    {
        return false;
    }

    if (!IsCell(0,col))
    {
        ASSERT(false);
        return false;
    }

    COptiReqItem::eColumn ec = static_cast<COptiReqItem::eColumn>(col);

    switch(ec)
    {
        case COptiReqItem::c_OptiReqId:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, long, &COptiReqItem::GetOptiReqId>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;

        case COptiReqItem::c_OptiReqInserted:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleSystemTime, &COptiReqItem::GetOptiReqInserted>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;
     
        case COptiReqItem::c_OptiReqProcessed:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleSystemTime, &COptiReqItem::GetOptiReqProcessed>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);

        case COptiReqItem::c_OptiReqWaitUntil:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleSystemTime, &COptiReqItem::GetOptiReqWaitUntil>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;
 
        case COptiReqItem::c_OptiReqMode:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetOptiReqMode>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;

        case COptiReqItem::c_OptiReqCauser:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetOptiReqCauser>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);               
            break;

        case COptiReqItem::c_Fldt:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleSystemTime, &COptiReqItem::GetFldt>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;

       case COptiReqItem::c_Fpfx:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetFpfx>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;

       case COptiReqItem::c_Fsfx:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetFsfx>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);           
            break;
  
        case COptiReqItem::c_Flnr:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, int, &COptiReqItem::GetFlnr>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);           
            break;

        case COptiReqItem::c_Lgnr:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, int, &COptiReqItem::GetLgnr>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);           
            break;

        case COptiReqItem::c_Std:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, int, &COptiReqItem::GetStd>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);   
            break;
        case COptiReqItem::c_Sta:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, int, &COptiReqItem::GetSta>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc); 
            break;

        case COptiReqItem::c_CallSgn:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetCallSgn>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);
            break;

        case COptiReqItem::c_DepIcao:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetDepIcao>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);            
            break;

        case COptiReqItem::c_DepIata:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetDepIata>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);            
            break;

        case COptiReqItem::c_DesIcao:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetDesIcao>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);            
            break;

        case COptiReqItem::c_DesIata:
            Sample::SortByMember<VecOptiReqItems, COptiReqItem, CSampleString, &COptiReqItem::GetDesIata>
                (m_vItemsFiltered.begin(), m_vItemsFiltered.end(), bAsc);               
            break;
            
    }

    return true;
}

bool
COptiReqContainer::IsRow(long row) const
{
    return InRange(static_cast<long>(row), 0, m_vItemsFiltered.size() -1) == 1;
}

bool
COptiReqContainer::IsCell(long row, int col) const
{
    return IsRow(row) && InRange(static_cast<long>(col), 0, COptiReqItem::c_Cnt -1) ==1 ;
}

bool
COptiReqContainer::GetItemByRow(long row, COptiReqItem &Item) const
{
    // if(IsRow(row)) - not used because of speed 
    try
    {
        Item = m_vItemsFiltered.at(static_cast<size_t>(row));
    }
    catch(std::out_of_range o)
    {
        return false;
    }

    return true;
}

bool
COptiReqContainer::GetItemById(long nOptiReqId, COptiReqItem &Item) const
{
    VecOptiReqItems::const_iterator citer = std::find_if(m_vItemsFiltered.begin(),
                                                        m_vItemsFiltered.end(),
                                                        COptiReqItem::OptiReqIdEquals(nOptiReqId));
    
    if (citer != m_vItemsFiltered.end())
    {
        Item = *citer;
        return true;
    }

    return false;     
}

bool
COptiReqContainer::Save(COptiReqItem &OptiReqItem, int &nStatusSave)
{
    nStatusSave = eSave_NothingChanged;
    bool bret = true;
    
    if(!m_DbAppMonitor.ConnectDatasourceRetry())
    {
        ASSERT(false);
        nStatusSave = eSave_Error_Database;
        return false;            
    }

    COptiReqItem prevItem;
    
    // get the opti request item from last database load
    // compare if anything has changed, that needs to be saved
    if (GetItemById(OptiReqItem.m_nOptiReqId, prevItem) &&
        prevItem != OptiReqItem)
    {
        // anything has changed
#ifdef _Sample_DEBUG_OPTIREQMONITOR
        CSampleString cs;
#endif
        if (prevItem.GetOptiReqMode() != OptiReqItem.GetOptiReqMode())
        {
            // the OptiReqMode has changed

            Sample::CSampleQueryParam SQLQuery(g_Defs.g_csSp_opti_request_GetColumn.c_str(), Sample::CSampleQueryParam::E_SQLType_OLEDB);
            // check if previous item is still the same like in db
            SQLQuery.AddParam(OptiReqItem.GetOptiReqId());
            SQLQuery.AddParam(g_Defs.g_csTable_opti_request_Column_Mode.c_str());

#ifdef _Sample_DEBUG_OPTIREQMONITOR
            cs = SQLQuery.GetQuery();
#endif
            CRowset *pRowset = m_DbAppMonitor.GetConnection()->GetRowset(SQLQuery.GetQuery().c_str());

            if(!pRowset)
            {
                nStatusSave |= eSave_Error_Database;
                return false;
            }

            TCHAR csResMode[2];

            DBBINDING dbBind[1];
            SetDBBinding(dbBind[0], 1, DBPART_VALUE, 0, 0, 0, DBTYPE_TSTR, 2);
            HACCESSOR hAccessor;

            if(pRowset->Bind(dbBind, 1, hAccessor))
            {
                while(pRowset->NextRow())
                {
                    pRowset->GetRowData(hAccessor, reinterpret_cast<void*>(&csResMode));             
                    pRowset->Unbind(hAccessor);
                }

                SAFEDELETEPTR(pRowset);
            }
            
            bool bDbTokenWhitespace = _tcslen(csResMode) <1 ||
                                      _tcscmp(csResMode, _T(" ")) == 0 ||
                                      _tcscmp(csResMode, _T("  ")) == 0;

            if (  !(bDbTokenWhitespace && (prevItem.GetOptiReqMode() == g_Defs.g_csTokenWhitespace)) &&
                   (prevItem.GetOptiReqMode() != csResMode))
            {
                //mode in db changed, changes not allowed
                nStatusSave |= eSave_Error_Mode_Deprecated;
            }
            else
            {
                // mode change allowed
                SQLQuery.ClearParams();
                SQLQuery.SetProcName(g_Defs.g_csSp_opti_request_UpdateColumn.c_str());
                SQLQuery.AddParam(OptiReqItem.m_nOptiReqId);
                SQLQuery.AddParam(g_Defs.g_csTable_opti_request_Column_Mode.c_str());

                if (OptiReqItem.GetOptiReqMode() == g_Defs.g_csTokenWhitespace)
                {
                    SQLQuery.AddParam(_T(" "));
                }
                else
                {
                    SQLQuery.AddParam(OptiReqItem.GetOptiReqMode().c_str());
                }
#ifdef _Sample_DEBUG_OPTIREQMONITOR
                cs = SQLQuery.GetQuery();
#endif
                // save mode change in database

	            if(m_DbAppMonitor.GetConnection()->Execute(SQLQuery.GetQuery().c_str(), NULL, 0, NULL, NULL, NULL, 0, 10))
	            {
                     VecOptiReqItems::iterator citer = std::find_if(m_vItemsFiltered.begin(),
                                                                   m_vItemsFiltered.end(),
                                                                   COptiReqItem::OptiReqIdEquals(OptiReqItem.m_nOptiReqId));

                    nStatusSave |= eSave_OK_Mode;

                    if (citer != m_vItemsFiltered.end())
                    {
                        // update the item in internal "opti req item" vector
                        *citer = OptiReqItem;
                    }
                    else
                    {
                        ASSERT(false); // strange, if your're here - very likly a bug, you need to check
                    }
	            }
	            else
	            {
	                // db execute failed
	                nStatusSave |= (eSave_Error_Mode_Deprecated);
	            }
            }
        }
    }

    return bret;
}

bool
COptiReqContainer::Save(COptiReqItem &OptiReqItem, CSampleString csRequestText, int &nStatusSave)
{
    nStatusSave = eSave_NothingChanged;
    bool bret = true;

    if (!m_DbAppMonitor.IsConnected())
    {
        nStatusSave |= eSave_Error_Database;
        return bret && false;
    }

    //without checking the previous requesttext, the requesttext is just put into database
    Sample::CSampleQueryParam SQLQuery(g_Defs.g_csSp_opti_request_UpdateColumn.c_str(), Sample::CSampleQueryParam::E_SQLType_OLEDB);
    // mode change allowed
    SQLQuery.AddParam(OptiReqItem.m_nOptiReqId);
    SQLQuery.AddParam(g_Defs.g_csTable_opti_request_Column_Request.c_str());
    SQLQuery.AddParam(csRequestText.c_str());

#ifdef _Sample_DEBUG_OPTIREQMONITOR
    CSampleString cs = SQLQuery.GetQuery();
#endif
    // save mode change in database

    if(m_DbAppMonitor.GetConnection()->Execute(SQLQuery.GetQuery().c_str(), NULL, 0, NULL, NULL, NULL, 0, 10))
    {
        // saved to db successfully
        nStatusSave |= eSave_OK_Requesttext;
    }
    else
    {
        // db execute failed
        nStatusSave |= eSave_Error_Database;
    }
    
    return bret;
}

size_t
COptiReqContainer::GetRow(COptiReqItem &OptiReqItem) const
{
    size_t ret=-1;

    VecOptiReqItems::const_iterator citer = std::find_if(m_vItemsFiltered.begin(),
                                                        m_vItemsFiltered.end(),
                                                        COptiReqItem::OptiReqIdEquals(OptiReqItem.m_nOptiReqId));

    if (citer != m_vItemsFiltered.end())
    {
        ret = citer - m_vItemsFiltered.begin();
    }
     
    return ret;
}

long 
COptiReqContainer::GetCountRow() const 
{
    return m_vItemsFiltered.size();
}

std::vector<CSampleString>
COptiReqContainer::GetOptiRequestRefCausers() const
{
    return m_vItemsRefCausers;
}

void
COptiReqContainer::SetMaxItemCount(int nMaxItemCount)
{
    m_nMaxItemCount = nMaxItemCount;
}


}
}