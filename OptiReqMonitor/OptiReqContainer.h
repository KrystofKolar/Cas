

#if !defined(AFX_OPTIREQCONTAINER_H__C5C603D5_4E34_90D5_2400_F9E5B635B97C__INCLUDED_)
#define AFX_OPTIREQCONTAINER_H__C5C603D5_4E34_90D5_2400_F9E5B635B97C__INCLUDED_

#include "SampleWinDef.h"
#include "OptiReqItem.h"
#include "SampleOptiReqModi.h"

#include <vector>
#include <map>

namespace Sample
{
namespace OptiReqMonitor
{

class CDbAppMonitor;

typedef std::vector<COptiReqItem> VecOptiReqItems;

//! container holding optimizer request items
class COptiReqContainer
{
public:
    //! filter params
    class FilterParams
    {
    public:
        //! constructor
        FilterParams()
        {
            Reset();
        }

        //! destructor
        virtual ~FilterParams()
        {
            Reset();
        }

        //! default values
        void Reset()
        {
            bUseable = false;
        
            csFpfx = _T("");
            nFlnr = -1;
            nLgnr = -1;
        
            csDepIcao = _T("");
            csDesIcao = _T("");
            csOptiReqCauser = _T("");
            fstSvt.Invalidate();
            fstEvt.Invalidate();
            v_csOptiReqModi.clear();
        }

        //! equality operator
        bool operator==(FilterParams const &other) const
        {
            return
            csFpfx == other.csFpfx &&
            nFlnr == other.nFlnr &&
            nLgnr == other.nLgnr &&
            csDepIcao == other.csDepIcao &&
            csDesIcao == other.csDesIcao &&
            fstSvt == other.fstSvt &&
            fstEvt == other.fstEvt &&
            v_csOptiReqModi == other.v_csOptiReqModi &&
            csOptiReqCauser == other.csOptiReqCauser &&
            bUseable == other.bUseable;
        }

        //! inequality operator            
        bool operator != (FilterParams const &other) const
        {
            return !( operator==(other) );
        }

        //! flightprefix
        CSampleString csFpfx;

        //! flightnumber
        int nFlnr;

        //! flightleg number
        int nLgnr;

        //! flight departure icao
        CSampleString csDepIcao;

        //! flight destination icao
        CSampleString csDesIcao;

        //! flight datetime svt
        CSampleSystemTime fstSvt;

        //! flight datetime evt
        CSampleSystemTime fstEvt;

        //! set of optimizer request modi
        //! @detail like 'XPOI'
        //todow CSampleString csOptiReqModi;
        std::vector<CSampleString> v_csOptiReqModi;

        //! opti request causer
        CSampleString csOptiReqCauser;
        
        //! determine if object is useable
        bool bUseable;  
    };

    //! constructor
    COptiReqContainer(CDbAppMonitor &DbAppMonitor);

    //! destructor
    virtual ~COptiReqContainer();
    
    //! Reload container
    //! @param Filter params
    //! @param true to force a db refresh, else decided automatically
    //! @param Window which handles user registered messages. Messages inform about the status of reloading.
    bool Reload(FilterParams Params, bool bRefresh=false, CWnd *pWndMsgReceiver = 0);

    //! sort rows by column
    //! @param column to sort
    //! @param ascending order 
    //! @return true if any row was sorted, else false(no row or error)
    bool SortRows(int col, bool bAsc=true);

    //! get item in a row
    //! @param row holding the item
    //! @param reference to the resulting item
    //! @return true if the item was found, else false
    bool GetItemByRow(long row, COptiReqItem &Item) const;
    
    //! get item by distinct opti request id
    //! @param opti request id
    //! @param reference to the resulting item
    //! @return true if the item was found, else false
    bool GetItemById(long nOptiReqId, COptiReqItem &Item) const;

    //! Get vector of unique opti request causers
    //! @detail used in combobox
    //! @return vector of unique opti request causers
    std::vector<CSampleString> GetOptiRequestRefCausers() const;

    //! get row for an item, where is the item located?
    //! @param reference for the item to be found
    //! @return -1 if not found, else row number
    size_t GetRow(COptiReqItem &Item) const;

    //! determince if row exists
    //! @return true if exists, else false
    bool IsRow(long row) const;
    
    //! determine if cell exists
    //! @param row number
    //! @param col number
    //! @return true if found, else false
    bool IsCell(long row, int col) const;

    //! get row count
    //! @return row count
    long GetCountRow() const;

    //! return values for member methode "Save"
    enum eSave_Status
    {
        //! Nothing changed during save
        eSave_NothingChanged =0,
    
        //! The mode changes were saved successfully
        eSave_OK_Mode = 1,
        //! The request changes were saved successfully
        eSave_OK_Requesttext,
        
        //! Database error, connection broke down etc. occurs
        eSave_Error_Database,
        //! The mode changes were cancelled, because the data is deprecated
        eSave_Error_Mode_Deprecated,
        //! The request text change was cancelled, because the data is deprecated
        eSave_Error_Requesttext_Deprecated
    };

    //! save opti request item in db and in container
    //! @detail The Opti Request Item is checked forall changes
    //!         The Opti Request Item values in Dialog, the manual Changes, the current database values are compared 
    //!         Example:
    //!         The Opti Request Item in Dialog has mode "F", it is manually changed by userinput to "X"
    //!         If meanwhile the opti Request item in database has changed to "P", you would set it back to "X" which is an error
    //!
    //!         The solution is to compare the previous states with the database values. If they match the change is allowed, else not
    //!
    //! @param opti request item to be saved
    //! @param bitcoded states of successfully or cancelled changes
    //!        @detail: If "Request text" changes are cancelled, the bitcoded "eret_Cancelled_Requesttext_Deprecated" will be found 
    //! @return if false is returned an error occured, you should check the nStatusSave parameter,
    //!         else true
    bool Save(COptiReqItem &Item, int &nStatusSave);

    //! save opti request text in db and in container
    //! @param opti request item
    //! @param text for opti request item
    //! @param[out] status of saving
    //! @return true on success, else false
    bool Save(COptiReqItem &Item, CSampleString csRequestText, int &nStatusSave);

    //! set limit for sql retrieval
	//! @param limit for sql retrieval
    void SetMaxItemCount(int nMaxItemCount = -1);
    
private:
    
    //! current filter params
    FilterParams m_Filter;

    //! previous filter params
    FilterParams m_prevFilter;
    
    //! vector of opti request mode
    CSampleOptiReqModi m_Modi;
    
    //! vector of opti request items
    VecOptiReqItems m_vItemsRef;

    //! vector of unique causers - ordered
    std::vector<CSampleString> m_vItemsRefCausers;

    //! vector of opti request items filtered
    VecOptiReqItems m_vItemsFiltered;

    //! helper db object
    CDbAppMonitor &m_DbAppMonitor;
    
    //! limit for sql retrieval
    int m_nMaxItemCount;
};

}
}


#endif // !defined(AFX_OPTIREQCONTAINER_H__C5C603D5_4E34_90D5_2400_F9E5B635B97C__INCLUDED_)