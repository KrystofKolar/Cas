
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

using Cwa;
using Cwa.Cas;
using CwaCommon;
using CwaNotes;
using CwaScreenSystem;
using CasBase;

namespace Cas
{

// Summary:
//     Holds data to load a specific file from the iso.storage
//     The file(string) is linked to a note(DateTime)
//     The filename is defined in PairIsofile.
internal class IsoLoadRequest
{
    // Summary:
    //     note key. Unique like the unified datetime of a bucket.
    //     Sample: 
    //            2018-05-23 12:45
    public String key;

    // Summary:
    //     Link a datatime to a filename(string)
    //
    //     todo value currently unused, cause filenames have a special format
    //          which is created out of the datetime
    public KeyValuePair<DateTime, string> PairIsofile;

    // Summary:
    //     ctor
    public IsoLoadRequest()
    {
        key = String.Empty;
        PairIsofile = new KeyValuePair<DateTime, string>(DateTime.MinValue, String.Empty);
    }
}

// Summary:
//      see the event using this delegate
public delegate void DelegateRemoveClickInfo(casId idBucket);

// Summary:
//      see the event using this delegate
public delegate void DelegateRemoveNote(String md5Note);

// Summary:
//      see the event using this delegate
public delegate bool DelegateSaveBucketPositionToNote(String md5Note);

// Summary:
//      see the event using this delegate
public delegate bool DelegateSetBucketPosition(casId idBucket, Vector2 posSu);

//
// Summary:
//     The datetime is linked to a bucket, a bucket is a timerange of 30 mins
//
//     Currently one note is linked to one bucket and one "notefile"
//     The note file holds data of the CwaNote class, which are properties like
//     position, check status etc.
//     Optional this note is linked to a file like a texture/picture/file.
//
public class CwaTexNotesManager : IDisposable
{
    // Summary:
    //     Indicate if already disposed.
    bool _disposed = false;

    // Summary:
    //     Immediately releases the unmanaged resources and marks managed ones as unused
    //     user has to call.
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Dispose(true);

        if (_disposed) // The call to SuppressFinalize should only occur if Dispose(true)
        {              // executes successfully.
            GC.SuppressFinalize(this);  
        }
    }

    // Summary:
    //     Helper for the Dipose interface
    //     If its called by the GC, the parameter is false and just the 
    //     unmanaged resources are disposed
    //     If its called manually by calling Dipose(), the parameter is true
    //     and managed and unmanaged resourcs are disposed
    //
    //     All resources implementing IDisposable have to be listed here.
    protected virtual void Dispose(bool manual)
    {
        if (_disposed)
        {
            return;
        }

        if (manual)
        {
            if (_texNoteAlpha != null)
            {
                _texNoteAlpha.Dispose();
            }

            if (NoteStatsImage != null)
            {
                NoteStatsImage.Dispose();
            }

            if (_NoteMenuBlank != null)
            {
                _NoteMenuBlank.Dispose();
            }

            for (int i = 0; i < (int)casTextureVariant.CountTint; ++i)
            {
                _texsNoteEmpty[i].Dispose();

                _texsNoteOverlayUnchecked[i].Dispose();

                _texsNoteOverlayChecked[i].Dispose();

                _texsNoteMenu[i].Dispose();
            }
        }

        _disposed = true;
    }

    // Summary:
    //     Default bucket position in the farseer world [simulation units]
    Vector2[] _BucketPosSuDefault;

    // Summary:
    //     Datetime center of all calculations
    public DateTime dtCenter = DateTime.MinValue;

    // Summary:
    //     Datetime range around the center plus some extra time(width of note image)
    public DateTime[] dtCenterLeftRightPlus = new DateTime[2];

    // Summary:
    //     Update Calculation of requests to load files from the iso.storage
    //     Sample:
    //        New note is in range, thus create request for loading files linked 
    //        to this note.
    bool _updateCalcIsoLoadRequest = false;

    // Summary:
    //     When the PhotoChooser is triggerd, this application is deactivated.
    //     The note(id) that was calling the PhotoChooser is saved in this persistant property.
    public CwaIsolatedStorage.IsolatedStorageProperty<String> PhotoChooser_NoteId;

    // Summary:
    //     Remove item from outside items like scrollingbackground.
    //     Called when note is deleted, out of viewport or hidden because of texturevariant.
    public event DelegateRemoveClickInfo EventNoteRemoved_ScrollingBackgroundRemoveItem;

    // Summary:
    //     Just empty the cached buckets(containing note) in scrollingbackground.
    //     Called when note is deleted, out of viewport or hidden because of texturevariant.
    //
    //     Always used with "DelegateRemoveClickInfo"
    public event DelegateRemoveNote EventNoteRemoved_NoteRemovedCauseCleanup;

    // Summary:
    //     Save the bucket position of a note.
    //     todo asap save new position, on release
    public event DelegateSaveBucketPositionToNote EventNote_SaveBucketPositionToNote;

    // Summary:
    //     Set the bucket position of a note.
    //     Called when buckets are getting visible.
    public event DelegateSetBucketPosition EventNote_SetBucketPosition;

    // Summary:
    //     Note statistics 
    //     calculated around a datetime
    CwaNoteStats _NoteStats;

    // Summary:
    //     Image given an overview of notes
    public CwaNoteStatsImage NoteStatsImage;

    // Summary:
    //     Half Viewport width as [h]. This is used for
    //     calculations to get the visible notes.
    int _hourViewportOver2;

    // Summary:
    //     SpriteBatch, extern used
    SpriteBatch _sb;

    // Summary:
    //     GraphicsDevice
    GraphicsDevice _gd;

    // Summary:
    //     ContentManger
    ContentManager _cm;

    // Summary:
    //     Textures loaded from somewhere will get this width
    public const int TEXWIDTH = 270;

    // Summary:
    //     Textures loaded from somewhere will get this height
    public const int TEXHEIGHT = 265;

    // Summary:
    //     Notes are created iterative. Notes of visible buckets are loaded first,
    //     then the hidden ones. In worst case - thousends of notes to load - but the
    //     app is only started for seconds, the app will not be able to load all immediatly,
    //     but typical case is, that thousends are loaded within a half minute, so this
    //     should be good enough.
    //
    //     Any case the viewable notes are loaded and visible practically immediatly.
    public Dictionary<String, CwaNote> Notes;

    // Summary:
    //     List of loadrequests for the isolated storage. Those are requests to load
    //     files for notes, the visible notes first, then the more far away.
    List<IsoLoadRequest> _IsoLoadRequests = new List<IsoLoadRequest>();

    // Summary:
    //     Indicate the texturevariant for all notes. Persistent stored.
    CwaIsolatedStorage.IsolatedStorageProperty<casTextureVariant> _tvAllNotes;
    public casTextureVariant tvAllNotes { get { return _tvAllNotes.Value; } }

    // Summary
    //     The scenario like original, grey the NotesManager is using for drawing.
    Scenario _Scenario = Scenario.Undef;

    // Summary:
    //     The outer scenario is translated to an internal(of this class)one.
    //     Thats because only grey and default scenarios are supported in NotesManager.
    //
    //     Sample:
    //        The App uses Scenario.Greymix to show a menu colored and other items in grey. 
    //        One of this "grey" items is the NotesManager and this is configured here.
    //
    //        Then the App uses Scenario.Original to show all items in "original" colors.
    //        All those changes are configured here.
    //
    // Return Values:
    //     Scenario the NotesManager is configured
    //
    public Scenario ScenarioRequested
    {
        get
        {
            return _Scenario;
        }

        set
        {
            switch (value)
            {
                case Scenario.Grey:
                case Scenario.GreyDepth:
                case Scenario.GreyMix:
                    _Scenario = Scenario.Grey;
                    break;

                default:
                    _Scenario = Scenario.Original;
                    break;
            }
        }
    }

    // Summary:
    //     List of textures for the empty note.
    //     the empty note is one with no file/picture linked to it
    //     The list holds the texturevariants of this note.
    Texture2D[] _texsNoteEmpty;

    // Summary:
    //     Alpha value to be used for all notes.
    //     Used to create the border.
    Texture2D _texNoteAlpha;

    // Summary:
    //     Texture for the unchecked state.
    //     Also a list of texturevariants.
    Texture2D[] _texsNoteOverlayUnchecked;

    // Summary:
    //     Texture for the checked state.
    //     Also a list of texturevariants.
    Texture2D[] _texsNoteOverlayChecked;

    // Summary:
    //     Background texture for the menu of an existing note.
    //     Counterpart is the blank menu - for creating an note.
    Texture2D[] _texsNoteMenu;

    // Summary:
    //     Delete button for the menu of an existing note.
    Texture2D _texMenuButtonDelete;

    // Summary:
    //     Scenario button for the menu of an existing note.
    //     This changes the texturevariant of the note.
    Texture2D _texMenuButtonScenarioIter;

    // Summary:
    //     Check button for the menu of an existing note.
    Texture2D _texMenuButtonCheckNote;

    // Summary:
    //     Pin texture, just used as an texture for drawing.
    Texture2D _texPin;

    // Summary:
    //     Menu for creating notes. 
    //     Currently no note exists at this datetime and for
    //     this case this menu is shown.
    NoteMenuBlank _NoteMenuBlank;

    // Summary:
    //     Click region for the menu, which is shown for an active note
    //
    //     e.q. on mouseover on a datetime which is linked to a note
    //          then the note will be shown and this menu.
    Regions<casId> _NoteMenuExistsClickable;

    // Summary:
    //     Scale for the click-regions of a the menu.
    //     There was a discussion about, currently set to 1F !
    float _ClickRegionsNoteMenuScale;

    // Summary:
    //      Datetime of the clickevent of an menuitem/button.
    //      If you click the menu texture, but no button the datetime
    //      is not changed.
    DateTime _NoteMenuItemTestClicked;

    // Summary:
    //     Files/Pictures with a filename beginning with "PIC"
    //     will be parsed, then imported to the app
    const string FILENAMEPICTURE = "PIC";

    // Summary:
    //     Holds the data of a note. Properties like check status, 
    //     date, positon, ... are saved. See CwaNote members.
    const string FILENAMENOTE = "NTE";

    // Summary:
    //
    //     Each day is split into 48 buckets(0.5 hrs a bucket) - identified by "casId"
    //     Because only filled buckets are stored in this attribute, the "String" holds
    //     the note id(which is a string).
    public Dictionary<casId, String> Buckets;

    // Summary:
    //
    //     Iterate the given texturevariant from Original->Grey->Hidden->Original
    //
    // Parameters:
    //   tv:
    //     texturevariant to be used for iteration
    //
    // Return Values:
    //     the "new" texturevariant
    casTextureVariant GetNextTextureVariant(casTextureVariant tv)
    {
        casTextureVariant next;
 
        switch (tv)
        {
            case casTextureVariant.Original:
                next = casTextureVariant.Grey;
                //Debug.WriteLine("New Note texturevariant is grey");
                break;

            case casTextureVariant.Grey:
                next = casTextureVariant.Hidden;
                //Debug.WriteLine("New Note texturevariant is hidden");
                break;

            default:
                next = casTextureVariant.Original;
                //Debug.WriteLine("New Note texturevariant is original");
                break;
        }

        return next;
    }

    // Summary:
    //     Change the texturevariant of the note by clicking on the bucket
    //
    // Parameters:
    //   idBucket:
    //
    // Return Values:
    //      note key
    public String ActionScenarioForNoteIter(casId idBucket)
    {
        String key = Buckets[idBucket];
        CwaNote Note = Notes[key];

        switch (_Scenario)
        {
            case Scenario.Original:
            case Scenario.Undef:
                // the scenario of the Notes and the whole ScreenCas is Original/Undef
                Note.cfgTextureVariantOfScene_Original = GetNextTextureVariant(Note.cfgTextureVariantOfScene_Original);
                Note.SaveIso();

                break;

            default:
                // the scenario of the Notes and the whole ScreenCas is Greymix or others
                Note.cfgTextureVariantOfScene_Greymix = GetNextTextureVariant(Note.cfgTextureVariantOfScene_Greymix);
                Note.SaveIso();

                break;
        }

        return key;
    }

    // Summary:
    //     Action, Event which toggles the check status of a note
    //     in a bucket.
    //
    // Parameters:
    //   bucket
    //     The bucket has to hold a note. Caller is responsible. 
    //     If an empty bucket is used, an exception will be thrown.
    public void ActionDetailCheckToggle(casId bucket)
    {
        try
        {
            String md5 = Buckets[bucket];

            CwaNote Note = Notes[md5];

            Note.CheckToggle();
        }
        catch (Exception e)
        {
            Debug.WriteLine("ActionDetailCheckToggle " + e.Message.ToString());
        }
    }

    // Summary:
    //     Action - when button pressed - iterates the texturevariant for all notes.
    public void ActionAllNotesVariantIter()
    {
        switch (_tvAllNotes.Value)
        {
            case casTextureVariant.Hidden:
                _tvAllNotes.Value = casTextureVariant.Original;
                Debug.WriteLine("ActionAllNotesVariantIter Blank->Original");
                break;

            case casTextureVariant.Original:
                _tvAllNotes.Value = casTextureVariant.Grey;
                Debug.WriteLine("ActionAllNotesVariantIter Original->Grey");
                break;

            case casTextureVariant.Grey:
                _tvAllNotes.Value = casTextureVariant.Hidden;
                Debug.WriteLine("ActionAllNotesVariantIter Grey->Hidden");
                break;

            default: // should never get here, because cases above should be possible only
                _tvAllNotes.Value = casTextureVariant.Original;
                Debug.WriteLine("ActionAllNotesVariantIter Default->Original, " +
                                "now the specific items configuration is used");
                break;
        }

        return;
    }

    // Summary:
    //
    //     Create or reload(thus maybe) a note from isostore to Notes container.
    //
    //     Processing:
    //     *) For a given date, check if the note is already loaded in "Notes",
    //        if this is the case, its not an error, but that should not happen - because
    //        you're calling this methode, to load a note, which is already loaded.
    //
    //     *) if not then try to load it from the isostore
    //        that means the note was created before, but not loaded yet into application.
    //        if this is the case, the application is typically running for a short time and wasn't
    //        able to load the note on its own.
    //
    //     *) if not then create a new note.
    //        That is the default case. 
    //
    // Parameters:
    //   dtNote:
    //     Unified date is the notes key. Caller takes care if valid
    //
    // Return Values:
    //     Empty string on error, else the note key.
    //
    public string CreateMaybeNote(DateTime dtNote)
    {
        String key = dtNote.ToString();

        if (Notes.ContainsKey(key))
        {
            Debug.WriteLine("CreateMaybeNote: Note already loaded, Should be fixed. {0}", key);

            return key;
        }

        String fname = CwaTexNotesManager.GetFilenameCwaNote(dtNote);

        CwaNote Note = null;

        if (CwaIsolatedStorage.IsolatedStoragePropertyHelper.Store.Contains(fname))
        {
            Debug.WriteLine("Note loading from iso now, instead later automatically. {0}", fname);

            CwaIsolatedStorage.IsolatedStorageProperty<CwaNote> NoteIso =
                new CwaIsolatedStorage.IsolatedStorageProperty<CwaNote>(fname, new CwaNote());

            Note = NoteIso.Value;
        }
        else
        {
            Debug.WriteLine("Note creating {0}", key);

            Note = new CwaNote();

            Note.dtUnified = dtNote;
            Note.posSuBucket = _BucketPosSuDefault[(int)Note.idBucket - (int)casId.markBucket0];

            Note.SaveIso();
        }

        Note.LoadContent(_gd,
                        _sb,
                        _cm,
                        _texsNoteEmpty,
                        _texNoteAlpha,
                        _texsNoteOverlayChecked,
                        _texsNoteOverlayUnchecked,
                        _texsNoteMenu,
                        _texMenuButtonScenarioIter,
                        _texMenuButtonDelete,
                        _texMenuButtonCheckNote,
                        _texPin,
                        new Point(150, 15),
                        new Point(30, 150),
                        new Point(150, 150));

        Notes[key] = Note;
        NoteStats_CreateDelete();

        NoteUpdate(null, true, key);
        _CalcBucketsUpdateNeeded = true;

        return key;
    }

    // Summary:
    //
    //     Delete note with 
    //     *) all linked files of the note. These are the pictures, textures ...
    //     *) the note on the isostore itself
    //     *) the note from the notesmanager(container Notes)
    //     *) the note from outside notesmanager
    //
    // Parameters:
    //   idBucket:
    //     Bucket of the note. Caller is responsible to test if valid.
    //
    // Return Values:
    //     false on error, else true.
    //
    public bool NoteDelete(casId idBucket)
    {
        String key = Buckets[idBucket];

        CwaNote Note = Notes[key];

        foreach (var file in Note.Isonames)
        {
            // delete all linked files
            CwaIsolatedStorage.IsolatedStorageHelper.RemoveFileFromIso(file.Value);
        }

        // delete note
        String fname = CwaTexNotesManager.GetFilenameCwaNote(Note.dtUnified);
        CwaIsolatedStorage.IsolatedStorageHelper.RemoveFileFromIso(fname);

        // delete from app
        Notes.Remove(key);

        // delete from outside notesmanager
        // like ScrollingBackground, which shows notes.
        EventBulk_RemoveOuterNoteComplete(key, idBucket);

        //update note statistics
        NoteStats_CreateDelete();

        return true;
    }

    // Summary:
    //     Inform statisics about create/delete of a note
    public void NoteStats_CreateDelete()
    {
        _NoteStats.CalcStats_CauseCreateDelete(dtCenter);
    }
    //
    // Summary:
    //     Inform statistics about moving the datetime.
    public void NoteStats_Moving()
    {
        _NoteStats.CalcStats_CauseMoving(dtCenter);
    }
    //
    // Summary:
    //     Get previous note relative to datetime
    //
    // Returns:
    //     previous note relative to datetime 
    public String NoteStatsPrev()
    {
        return _NoteStats.Prev();
    }
    //
    // Summary:
    //     Get next note relative to datetime
    // Returns:
    //     next note relative to datetime
    public String NoteStatsNext()
    {
        return _NoteStats.Next();
    }
    //
    // Summary:
    //     Indicate that buckets need to be recalculated
    private bool _CalcBucketsUpdateNeeded = true;
    //
    // Summary:
    //     Indicates that all notes for bucket were loaded from isostore.
    //     Still its possible that notes exist in the gallery for the bucket.
    private bool _StoreBucketsReady = false;

    // Summary:
    //     Indicates that all notes were loaded from isostore.
    //     Still its possible that notes exist in the gallery for the bucket.   
    private bool _StoreReady = false;
    //
    // Summary:
    //     Enumerator of the isostore. Used internally to load one note
    //     after another from the isostore.
    private System.Collections.IEnumerator _StoreEnum = null;
    //
    // Summary:
    //     Load iterative two(performance) notes after another from isostore
    //     *) Test if bucket has a file in isostore and load it.
    //     *) Then enumerate files in isostore, test if the name matches our pattern
    //             and load them into app. Iterative load because of performance.
    //
    // Returns:
    //     true when all notes processed, false if there need to be notes still loaded
    public bool IterIsoLoadNotes()
    {
        if (_StoreReady)
        {
            return true;
        }

        // iterate over each bucket and test if a note exists in store
        if (_StoreBucketsReady == false)
        {
            // iter over each bucket from left to right
            // test and maybe load from database
            // create note

            // get range of unified bucket datetimes.
            // in practice thats 6 hrs -> 12 buckets
            TimeSpan ts = dtCenterLeftRightPlus[1] - dtCenterLeftRightPlus[0];
            int bc = (int)ts.TotalMinutes / (int)CwaCommon.eDatetime.eMinutesPerHourOver2 + 1; // buckets

            DateTime key = dtCenterLeftRightPlus[0];
            key = Cwa.CwaNote.GetDateTimeUnified(key);

            int n = 0; // notes to load

            for (int i = 0; i < bc; i++)
            {
                if (Notes.ContainsKey(key.ToString()) == false)
                {
                    // test if a note in the isostore exists

                    // create the filename to be searched for
                    string fname = CwaTexNotesManager.GetFilenameCwaNote(key);

                    if (CwaIsolatedStorage.IsolatedStoragePropertyHelper.Store.Contains(fname))
                    {
                        // found a note, which is used in a bucket and thus visible to the user

                        if (CreateMaybeNote(key) != String.Empty)
                        {
                            _CalcBucketsUpdateNeeded = true;
                            //Debug.WriteLine("Loaded and created note in bucket-loop \"{0}\"", fname);
                        }

                        if (++n > 5) //perf load just some notes, to keep the app responsive
                            break;
                    }
                }

                if (i == bc-1)
                {
                    //Debug.WriteLine("Notes for all buckets loaded");
                    _StoreBucketsReady = true;

                    break;
                }

                key = key.AddMinutes(30); //todo magic

            } // for bucketrange

            if (_StoreBucketsReady == false)
            {
                return false; // don't load more notes, first finish the buckets
            }
        }

        // all buckets were loaded, so try to loade notes outside viewport

        if (_StoreReady == false)
        {
            if (_StoreEnum == null)
            {
                _StoreEnum = CwaIsolatedStorage.IsolatedStoragePropertyHelper.Store.Keys.GetEnumerator();
            }

            String key;
            int n = 0;

            while (true)
            {
                _StoreReady = _StoreEnum.MoveNext() == false;

                if (_StoreReady)
                {
                    //Debug.WriteLine("Store is ready loaded");
                    break;
                }

                key = _StoreEnum.Current.ToString();

                if (key.Substring(0, CwaTexNotesManager.FILENAMENOTE.Length) == CwaTexNotesManager.FILENAMENOTE)
                {
                    DateTime dt;

                    if (GetDateFromFilenameNote(key, out dt))
                    {
                        if (CreateMaybeNote(dt) != String.Empty)
                        {
                            _CalcBucketsUpdateNeeded = true;
                            Debug.WriteLine("Loaded and created note outside bucket \"{0}\"", key);
                            n++;
                        }
                    }
                }

                if (n >= 2)
                    break;
            }
        }

        return _StoreReady;
    }
    //
    // Summary:
    //     SLOW PERF, calc before call
    //     Calc buckets and their linked notes
    //     to calc, the _ProcessDatabase_AllFinished must be finished
    //
    // Returns:
    //     true when buckets where calculated
    public bool CalcBuckets(bool Calc,  DateTime dt)
    {
#if DEBUG
        try
        {
#endif
            if (Calc || _CalcBucketsUpdateNeeded)
            {
                dtCenter = dt;

                const int extra = 2;

                dtCenterLeftRightPlus[0] = dtCenter.AddHours(-extra - _hourViewportOver2);
                dtCenterLeftRightPlus[1] = dtCenter.AddHours(+extra + _hourViewportOver2);

                _CalcBucketsUpdateNeeded = true;

                if (_StoreReady == false)
                {
                    _StoreBucketsReady = false;
                }
            }

            if (_CalcBucketsUpdateNeeded == false)
            {
                return false;
            }

            _CalcBucketsUpdateNeeded = false;

            //Debug.WriteLine("Calculating buckets");

            // Notes keeps the "notes" for a while, they dont need to be removed,(just because of memory yes)
            // in any case it has loaded all available in current bucketrange

            // list of notes ordered by distance to center
            var NotesInRange = Notes.Values.ToList()
                               .Where(note => note.dtUnified >= dtCenterLeftRightPlus[0] && note.dtUnified <= dtCenterLeftRightPlus[1]);
                               //.Where(note => (int)note.idBucket >= bucketLeftRight[0] && (int)note.idBucket <= bucketLeftRight[1]);
                               // order descending(first the far away notes).. because the far aways notes 
                               // are overdrawn in the draw call update scrollingbackground
                               //.OrderByDescending(note => Math.Abs((note.dtUnified - aCalcCenter).Ticks));

            Dictionary<casId, String> BucketsPrev = new Dictionary<casId, String>(Buckets); // save previous

            Buckets.Clear();

            foreach (var note in NotesInRange)
            {
                // there can be only one note to one bucket(30 min wide), but many isofile linked to one note
                Buckets[note.idBucket] = note.dtUnified.ToString(); // md5 maybe later

                if (_updateCalcIsoLoadRequest == false) // test if any isoloadrequest is necessary
                {
                    // only set once
                    if (note.Isonames.Count() > 0 && (note.Isonames.Count() != note.TexturesIsoLoaded.Count())) //todo not only the count, use the name
                    {
                        _updateCalcIsoLoadRequest = true;
                    }
                }
            }

            foreach (var b in BucketsPrev)
            {
                String md5 = b.Value;

                if (Buckets.ContainsValue(md5) == false && // note isnt in bucket anymore
                    Notes.ContainsKey(md5)) // note still exists(vs deleted)
                {
                    EventNote_SaveBucketPositionToNote(md5); // gone/out of view note, save bucket position
                }
            }

            foreach (var b in Buckets)
            {
                String key = b.Value;

                if (BucketsPrev.ContainsValue(key) == false)// new note, move bucket to new position, which is saved in note
                {
                    CwaNote Note = Notes[key];

                    EventNote_SetBucketPosition(Note.idBucket, Note.posSuBucket); 
                }
            }
#if DEBUG
        }
        catch (Exception e)
        {
            Debug.WriteLine("CalcBuckets exception");
        }
#endif
            return true;
    }
    //
    // Summary:
    //     Remove the note from the "outer" items.
    //
    //     Remove from Scrollingbackground the bucket and the icon of the bucket
    //     and remove from the main app the caching info of the note.
    //
    // Parameters:
    //   key:
    //     Notekey to be removed.
    //   idBucket:
    //     bucketid to be removed
    void EventBulk_RemoveOuterNoteComplete(String key, casId idBucket)
    {
        EventNoteRemoved_ScrollingBackgroundRemoveItem(idBucket); // note texture
        EventNoteRemoved_ScrollingBackgroundRemoveItem((casId)(idBucket + 48)); // note front texture

        EventNoteRemoved_NoteRemovedCauseCleanup(key);
    }
    //
    // Summary:
    //     Remove those textures from the outside "items" like the scrolling background
    //     which are not visible, because too far away from the centerdate.
    //
    //     Call during movement allowed, about every 2 secs, little perf loss
    public void CleanupUnuseddNotesTextures() 
    {
        // get list of notes ordered by distance desc. to targettime
        // the most far away should be removed

        // take notes out of visible range with textures
        // sort by distance from calccenter

        var NotesCleanup = Notes.Values.ToList()
                   .Where(n => n.TexturesIsoLoaded.Count() > 0) // only those with textures !!
                   .Where(n => n.dtUnified < dtCenterLeftRightPlus[0] || n.dtUnified > dtCenterLeftRightPlus[1])
                   .OrderByDescending(n => Math.Abs((n.dtUnified - dtCenter).TotalMinutes)) // order by distance to center
                   .Take(5); 

        foreach (var n in NotesCleanup)
        {
            Debug.WriteLine("Cleaning up note textures {0}", n.dtUnified.ToString());
            EventBulk_RemoveOuterNoteComplete(n.dtUnified.ToString(), n.idBucket);

            n.TexturesIsoLoaded.Clear();
            n.tv = casTextureVariant.Undef;
        }
    }
    //
    // Summary:
    //     Get filename of a picture file that is/will be linked to a note
    // Returns:
    //     filename of a picture file that is/will be linked to a note
    private static string GetFilenamePicture(DateTime dt)
    {
        return GetFilenameCommon(dt, true);
    }
    //
    // Summary:
    //     Get filename of a note file that is/will be linked to a note
    // Returns:
    //     filename of a note file that is/will be linked to a note
    static public string GetFilenameCwaNote(DateTime dt)
    {
        return GetFilenameCommon(dt, false);
    }
    //
    // Summary:
    //     Get the filename of the picture or note
    //
    // Parameters:
    //   dt:
    //     Datetime of the note or picture
    //   bPicture:
    //     Determine if you create a picturename or notename
    //
    // Returns:
    //     the filename of the picture or note
    private static string GetFilenameCommon(DateTime dt, bool bPicture)
    {
        //PICAD0020180327_1234_55_120
        bool bAD = true;

        string fname = string.Format("{0}{1}{2,6:000000}{3,2:00}{4,2:00}_{5,2:00}{6,2:00}_{7,2:00}_{8,3:000}",
                                     bPicture ? FILENAMEPICTURE : FILENAMENOTE,
                                     bAD ? "AD" : "BC",
                                     dt.Year,
                                     dt.Month,
                                     dt.Day,
                                     dt.Hour,
                                     dt.Minute,
                                     dt.Second,
                                     dt.Millisecond);
        return fname;
    }
    //
    // Summary:
    //     Get the date from picture filename
    //
    // Parameters:
    //   fname:
    //      picture filename
    //   dt:
    //      resulting datetime
    //
    // Returns:
    //   false on error, else true
    static bool GetDateFromFilenamePicture(string fname, out DateTime dt)
    {
        return GetDateFromFilename(fname, out dt, FILENAMEPICTURE);
    }
    //
    // Summary:
    //     Get the date from note filename
    //
    // Parameters:
    //   fname:
    //      note filename
    //   dt:
    //      resulting datetime
    //
    // Return Values:
    //   false on error, else true
    static bool GetDateFromFilenameNote(string fname, out DateTime dt)
    {
        return GetDateFromFilename(fname, out dt, FILENAMENOTE);
    }
    //
    // Summary:
    //     Get the date from filename. Type of filename is configure in parameter
    //
    //     01234567890123456789012345
    //     PIC_0020180327_1234_55_120     picture
    //     PICAD0020180327_1234_55_120
    //     NTEAD0020180327_1234_00_000    CwaNote
    //
    // Parameters:
    //   fname:
    //      note filename
    //   dt:
    //      resulting datetime
    //
    // Return Values:
    //   false on error, else true
    static bool GetDateFromFilename(string fname, out DateTime dt, String StartsWith)
    {
        dt = DateTime.MinValue;

        if (fname.StartsWith(StartsWith) == false)
        {
            return false;
        }

        bool bAD = false;
        string sAD;

        int year = -1;
        string syear;

        int mon = -1;
        string smon;

        int day = -1;
        string sday;

        int hh = -1;
        string shh;

        int mm = -1;
        string smm;

        int ss = -1;
        string sss;

        int ms = -1;
        string sms;

        sAD = fname.Substring(3, 2);
        if (sAD == "AD")
        {
            bAD = true;
        }

        syear = fname.Substring(5, 6);
        smon = fname.Substring(11, 2);
        sday = fname.Substring(13, 2);

        shh = fname.Substring(16, 2);
        smm = fname.Substring(18, 2);

        sss = fname.Substring(21, 2);
        sms = fname.Substring(24, 3);

        if (!Int32.TryParse(syear, out year))
        {
            return false;
        }
        if (!Int32.TryParse(smon, out mon))
        {
            return false;
        }
        if (!Int32.TryParse(sday, out day))
        {
            return false;
        }



        if (!Int32.TryParse(shh, out hh))
        {
            return false;
        }
        if (!Int32.TryParse(smm, out mm))
        {
            return false;
        }

        if (!Int32.TryParse(sss, out ss))
        {
            return false;
        }

        if (!Int32.TryParse(sms, out ms))
        {
            return false;
        }

        dt = new DateTime(year, mon, day, hh, mm, ss, ms);

        return true;
    }
    //
    // Summary:
    //     Common way for baking a texture for this app.
    //
    // Parameters:
    //   tex:
    //     texture for baking
    //
    // Return Values:
    //   resulting texture, in case of error an exception will be thrown.
    Texture2D CommonTextureBaking(Texture2D tex)
    {
        return CwaCommon.Texture.ResizeNonUniform(_gd, _sb, tex, TEXWIDTH, TEXHEIGHT, Color.White);
    }
    //
    // Summary:
    //     When the photochooser task completed, this function will be called
    // 
    // Parameters:
    //   texResult:
    //     resulting, chosen texture
    public void OnPhotoChooserTaskCompleted(Texture2D texResult)
    {
        String key = PhotoChooser_NoteId.Value; // Note key

        PhotoChooser_NoteId.Value = String.Empty; // consume

        if (Notes.ContainsKey(key) == false)
        {
            Debug.WriteLine("Internal error: OnPhotoChooserTaskCompleted key {0} not exists", key);

            return;
        }

        DateTime dtUnified = Notes[key].dtUnified;
        String fname = GetFilenamePicture(dtUnified);

        Texture2D texBaked = CommonTextureBaking(texResult);

        CwaIsolatedStorage.IsolatedStorageHelper.SaveTextureToIso(fname, texBaked); //overwrite file
        Notes[key].TexturesIsoLoaded.Clear(); // clear all previous !

        Notes[key].IsonamesAdd(dtUnified, fname);
        _CalcBucketsUpdateNeeded = true;
     }
    //
    // Summary:
    //     File handle enumerator to a picture in the medialibrary.
    IEnumerator<Picture> ProcessPicturenamesEnumerator = null;
    //
    // Summary:
    //     Determine if the medialibrary was processed completly.
    bool ProcessMedialibraryFinished = false;
    //
    // Summary:
    //     Iterate in the medialibrary and copy pictures to the isostore
    //     and show them as notes.
    //
    // Parameters:
    //   anyFile:
    //     Determine if any file will be processed, or only those with a special filename
    //   intervalTest:
    //     Retest the medialibrary in time interval
    //
    // Return Values:
    //   false if no picture was processed(maybe none in the medialibary) or on error, else true
    public bool IterCopyPicturesFromMedialibraryToIsostorage(bool anyFile, bool intervalTest) //todo test param
    {
        try
        {
            if (ProcessMedialibraryFinished)
            {
                if (intervalTest)
                {
                    // test library again
                    //Debug.WriteLine("IterCopyPicturesFromMedialibraryToIsostorage IntervalTest repeat");
                    ProcessMedialibraryFinished = false;
                    ProcessPicturenamesEnumerator = null;
                }
                else
                {
                    // once checked the library, now finish
                    return false;
                }
            }

            if (ProcessPicturenamesEnumerator == null)
            {
                MediaLibrary mediaLib = new MediaLibrary();

                ProcessPicturenamesEnumerator = mediaLib.Pictures.GetEnumerator();

                if (ProcessPicturenamesEnumerator == null)
                {
                    Debug.WriteLine("Error. No access to medialibrary");
                    return false; // still no access to medialib, so leave
                }
            }

            // try to get the next picture
            ProcessMedialibraryFinished = ProcessPicturenamesEnumerator.MoveNext() == false;

            if (ProcessMedialibraryFinished)
            {
                ProcessPicturenamesEnumerator = null;

                return false;
            }

            Picture picture = ProcessPicturenamesEnumerator.Current;

            string fname = picture.Name;

            int idx = fname.IndexOf("."); // remove extension
            if (idx > 0)
            {
                fname = fname.Substring(0, idx);
            }

            // get date and test eq. PICAD0020180327_1234_55_120
            DateTime dt;

            bool parsed = GetDateFromFilenamePicture(fname, out dt);

            if (parsed == false && anyFile)
            {
                // Picture name not valid, so take the date from the file
                parsed = true;
                    
                dt = picture.Date;
                fname = GetFilenamePicture(dt);
            }

            if (parsed == false)
            {
                // thats normal
                // eq. file exists in media, put is not named
                // to be used as a picture for this app
                return false;
            }
            
            if (CwaIsolatedStorage.IsolatedStorageHelper.FileExists(fname))
            {
                // thats normal
                // eq. media pictures were copied to isostore before
                // Debug.WriteLine("IterCopyPicturesFromMedialibrary - just info: file {0} already exists", fname);

                return false;
            }

            if (parsed == false)
                Debug.WriteLine("Creating from media library, configured anyfile: ({0})", fname);
            else
                Debug.WriteLine("Creating from media library                    :({0})", fname);

            dt = CwaNote.GetDateTimeUnified(dt);

            String key = CreateMaybeNote(dt); //save to iso value, it makes a recalc by changing CalcCenter

            if (key == String.Empty)
            {
                Debug.WriteLine("Error in IterCopyPicturesFromMedialibrary, id is empty");
                return false;
            }

            using (Texture2D pictureTex = Texture2D.FromStream(_gd, picture.GetImage()))
            {
                int WidthOver2 = pictureTex.Bounds.Center.X;
                int HeightOver2 = pictureTex.Bounds.Center.Y;

                int ptBegin = WidthOver2 - HeightOver2; //assume: width > heigth
                Rectangle rSource = new Rectangle(ptBegin, 0, WidthOver2, WidthOver2); // skip left/right

                using (RenderTarget2D rt = new RenderTarget2D(_gd, WidthOver2, WidthOver2))
                {
                    //fea make drawing in one call
                    //perf

                    // cut an area from the picturetex.
                    _gd.SetRenderTarget(rt);
                    _gd.Clear(Color.Transparent);
                    _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    _sb.Draw(pictureTex, rt.Bounds, rSource, Color.White);
                    _sb.End();
                    _gd.SetRenderTarget(null);

                    using (Texture2D texBaked = CommonTextureBaking(rt)) // resize to application format
                    {
                        CwaIsolatedStorage.IsolatedStorageHelper.SaveTextureToIso(fname, texBaked);

                        Notes[key].IsonamesAdd(dt, fname);
                        _CalcBucketsUpdateNeeded = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Internal error in IterCopyPictureFromMedialibrary");
            Debug.WriteLine(e.Message);

            ProcessMedialibraryFinished = false;

            return false;
        }

        return true;
    }
    //
    // Summary:
    //     Calculate ALL isoloadrequests for ALL buckets at once
    //     Has to be called if a note has changed in the buckets
    //     fast, internally enabled,
    public bool CalcIsoLoadRequestForBuckets()
    {
        if (_updateCalcIsoLoadRequest == false)
        {
            return false;
        }

#if DEBUG
        try
        {
#endif
            _IsoLoadRequests.Clear();

            foreach (var b in Buckets)
            {
                // Bucket always is filled with a Note
                /*
                if (b.Value == String.Empty)
                {
                    // Bucket has a note without an image
                    // no isoload request necessary
                    Debug.WriteLine("Internal error, test again in CalcIsoloadRequest"); //todo
                    continue;
                }
                */

                CwaNote Note = Notes[b.Value];

                foreach (KeyValuePair<DateTime, String> pair in 
                         Note.Isonames.Where(p => Note.TexturesIsoLoaded.ContainsKey(p.Key) == false))
                {
                    // isoname exists, but no texture

                    IsoLoadRequest req = new IsoLoadRequest();
                    req.key = Note.dtUnified.ToString();
                    req.PairIsofile = pair;

                    Debug.WriteLine("IsoloadRequest adding ({0}, {1})", pair.Key, pair.Value);

                    _IsoLoadRequests.Add(req);
                }
            }

            _IsoLoadRequests = _IsoLoadRequests // sort note by distance to targettime, should load the nearest first
                               .OrderBy(p => Math.Abs((Notes[p.key].dtUnified - dtCenter).Ticks))
                               .ToList();


#if DEBUG
        }
        catch(Exception e)
        {
            Debug.WriteLine("Internal error in CalcIsoLoadlist");
        }
#endif

        _updateCalcIsoLoadRequest = false;

        return true;
    }
    //
    // Summary:
    //     Load iterative pictures from the isostore from isostore-request.
    //
    //     The IsoLoadRequest class holds data.
    //
    // Returns:
    //     true if an request was processed successfully, 
    //     else false. 
    public bool IterLoadPicturesFromRequests()
    {
        if (_IsoLoadRequests.Count < 1)
        {
            return false;
        }

        IsoLoadRequest req = _IsoLoadRequests.First();

        if (Notes[req.key].LoadPicture(req.PairIsofile.Key))
        {
            _IsoLoadRequests.Remove(req);

            return true;
        }
        else
        {
            return false;
        }
    }
    //
    // Summary:
    //     Notes manager holding Notes, Statistics and a statistics image class.
    //
    //     Notes are loaded from isostore and medialibrary iterative and copied
    //     to isostore.
    //     Statistics like: "Notes in the next week etc." are calculated around a
    //     date.
    //     Statistics image is created also around a date.
    public CwaTexNotesManager()
    {
        Notes = new Dictionary<String, CwaNote>();
        Buckets = new Dictionary<casId, String>();

        _tvAllNotes =  new CwaIsolatedStorage.IsolatedStorageProperty<casTextureVariant>("Property_MenuButtonOverrides", casTextureVariant.Undef);

        _NoteStats = new CwaNoteStats();
        _NoteStats.Preset();

        NoteStatsImage = new CwaNoteStatsImage();
        NoteStatsImage.Enabled = false;

        _NoteStats.EventStatsRecalculated += NoteStatsImage.EventOccured_StatsRecalculated;
        

        _texsNoteEmpty = new Texture2D[(int)casTextureVariant.CountTint];
        _texsNoteOverlayUnchecked = new Texture2D[(int)casTextureVariant.CountTint];
        _texsNoteOverlayChecked = new Texture2D[(int)casTextureVariant.CountTint];

        _texsNoteMenu = new Texture2D[(int)casTextureVariant.CountTint];

        _NoteMenuExistsClickable = new Regions<casId>();
        _ClickRegionsNoteMenuScale = 1F;

        PhotoChooser_NoteId = new CwaIsolatedStorage.IsolatedStorageProperty<String>("CwaTexNotesItemsActionId", String.Empty);
    }
    //
    // Summary:
    //     Get statistics results
    //
    // Parameters:
    //   prop:
    //     The statistics property we're interested
    //
    // Returns::
    //     statistics result of asked property
    public int Stats(CwaNotes.StatProperty prop)
    {
        return _NoteStats.Result[prop];
    }
    //
    // Summary:
    //     Init and set some parameters
    //
    // Parameters:
    //   BucketPosSuDefault:
    //     Get the default position of the buckets in the farseer world
    //
    //   hourViewportOver2:
    //     Width of the ViewportOver2 in hours. Typically 3 hrs.
    //
    //   posNoteStatsImage:
    //     Position for the statistics image
    //     relative to left upper corner of viewport in pixels
    public void Init(Vector2[] BucketPosSuDefault, int hourViewportOver2, Vector2 posNoteStatsImage)
    {
        _BucketPosSuDefault = BucketPosSuDefault;

        _hourViewportOver2 = hourViewportOver2;
        NoteStatsImage.pos =  posNoteStatsImage;

        _NoteStats.Init(Notes, _hourViewportOver2);
    }
    //
    // Summary:
    //     Test which button was clicked in the note menu
    // Parameters:
    //   pos:
    //     positions, relative to upper left corner of menu
    // Returns:
    //     buttonid or casId.Undef
    public casId NoteMenuExistsTest(Vector2 pos)
    {
        casId id = _NoteMenuExistsClickable.Test(pos);

        if (id != casId.Min)
        {
            _NoteMenuItemTestClicked = DateTime.Now;
        }

        return id;
    }
    //
    // Summary:
    //     Test for a given date(usually mouselocation) if a note exists
    //     Algorithm: Get the bucket for the date.
    //                search in the filled buckets and return the note
    //
    // Parameter:
    //    dt:
    //      Test datetime if a note exists
    //
    // Returns:
    //      Tuple. First value indicates if the bucket is free.
    //             If a Note is found, then it is returned, else
    //             a Default Note(with default date) is returned
    //
    //      e.q. <false, CwaNoteFound> 
    //              false means a bucket is filled with CwaNoteFound
    //
    public KeyValuePair<bool, CwaNote> BucketTest(DateTime dt)
    {
        try
        {
            // find the bucket of mouselocation
            DateTime dtUnified = Cwa.CwaNote.GetDateTimeUnified(dt);
            casId idBucket = Cwa.CwaNote.GetBucket(dtUnified);

            if (Buckets.ContainsKey(idBucket))
            {
                String md5 = Buckets[idBucket];
                CwaNote Note = Notes[md5];

                // false means a filled bucket
                return new KeyValuePair<bool, CwaNote>(false, Note);
            }
            else
            {
                // true means an empty bucket
                return new KeyValuePair<bool, CwaNote>(true, new CwaNote());
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in GetKeyValuepair in GetMaybeNote");

            return new KeyValuePair<bool, CwaNote>(true, new CwaNote()); // true means free
        }
    }
    //
    // Summary:
    //     Load the content and build texture variations
    //
    // Parameters:
    //   mgr:
    //     ScreenManager holds references to GraphicsDevice, SpriteBatch etc.
    public void LoadContent(CwaScreensManager mgr)
    {
        _gd = mgr.GraphicsDevice;
        _sb = mgr.SpriteBatch;
        _cm = mgr.Content;

        NoteStatsImage.LoadContent(mgr);

        Texture2D texNoteEmpty = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteEmpty2");
        CwaCommon.Texture.Resize_CreateVariations(
            _gd,
            _sb,
            texNoteEmpty,
            texNoteEmpty.Height,
            true,
            1,
            false,
            true,
            out _texsNoteEmpty[(int)casTextureVariant.Original],
            out _texsNoteEmpty[(int)casTextureVariant.Grey],
            out _texsNoteEmpty[(int)casTextureVariant.White]
        );

        _texNoteAlpha = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteEmpty_Transparency");

        Texture2D texNoteOverlayUnchecked = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteOverlayUnchecked");
        CwaCommon.Texture.Resize_CreateVariations(
            _gd,
            _sb,
            texNoteOverlayUnchecked,
            texNoteOverlayUnchecked.Height,
            true,
            1,
            false,
            true,
            out _texsNoteOverlayUnchecked[(int)casTextureVariant.Original],
            out _texsNoteOverlayUnchecked[(int)casTextureVariant.Grey],
            out _texsNoteOverlayUnchecked[(int)casTextureVariant.White]
        );

        Texture2D texNoteOverlayChecked = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteOverlayChecked");
        CwaCommon.Texture.Resize_CreateVariations(
            _gd,
            _sb,
            texNoteOverlayChecked,
            texNoteOverlayChecked.Height,
            true,
            1,
            false,
            true,
            out _texsNoteOverlayChecked[(int)casTextureVariant.Original],
            out _texsNoteOverlayChecked[(int)casTextureVariant.Grey],
            out _texsNoteOverlayChecked[(int)casTextureVariant.White]
        );

        Texture2D texNoteMenu = mgr.Content.Load<Texture2D>("Background/Mark/FrameMenuBackground");
        CwaCommon.Texture.Resize_CreateVariations(
            _gd,
            _sb,
            texNoteMenu,
            texNoteMenu.Height,
            true,
            1,
            false,
            true,
            out _texsNoteMenu[(int)casTextureVariant.Original],
            out _texsNoteMenu[(int)casTextureVariant.Grey],
            out _texsNoteMenu[(int)casTextureVariant.White]
        );



        _texMenuButtonScenarioIter = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteButtonScenario");

        _texMenuButtonDelete = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteButtonDelete");
        
        _texMenuButtonCheckNote = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteButtonCheckNote");

        _texPin = mgr.Content.Load<Texture2D>("Background/Mark/FrameNoteEmpty_PinRed");

        _NoteMenuBlank = new Cwa.NoteMenuBlank();
        _NoteMenuBlank.LoadContent(mgr);

        mgr.Game.ResetElapsedTime(); //todo some general solution
    }
    //
    // Summary:
    //     Get the menu with buttons of a requested note
    // 
    // Parameters:
    //   key:
    //     Note key
    //
    // Returns:
    //     Texture of menu
    public Texture2D NoteMenuDetailResult(String key)
    {
        try
        {
            return Notes[key].Menu; // details of existing note
        }
        catch(Exception e)
        {
            Debug.WriteLine("ERROR: NoteMenuDetailResult"); //todo

            return null;
        }
    }
    //
    // Summary:
    //     Test the menu with buttons of a requested note
    // 
    // Parameters:
    //   key:
    //     Note key
    //   v:
    //     Position on the menu(relative to upper left corner) to be tested.
    //
    // Return Values:
    //   Item id of this click position
    public casId NoteMenuDetailTest(String key, Vector2 v)
    {
        try
        {
            return Notes[key].MenuRegions.Test(v);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception NoteMenuDetailTest" + e.Message.ToString());

            return casId.Undef;
        }
    }
    //
    // Summary:
    //     In the menu with buttons the event "check" occured.
    //     The note has to informed about it and check status toggles.
    //
    // Parameters:
    //   key:
    //     Note key
    public void NoteMenuDetailEventCheck(String Notekey)
    {
        try
        {
            Notes[Notekey].CheckToggle();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in Note Action: Check");
        }
    }
    //
    // Summary:
    //     Update the menu with buttons of the note.
    //
    //     It will be checked if update is necessary - thus Maybe.
    //
    // Parameters:
    //   gt:
    //     GameTime
    //   key:
    //     Note key
    public void NoteMenuDetailsUpdate(GameTime gt, String key)
    {
        try
        {
            Notes[key].MaybeUpdateMenu(gt);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception NoteMenuDetailsUpdate");
        }
    }
    //
    // Summary:
    //     Get the menu with buttons. Thats the menu for not existing notes
    //
    // Returns:
    //     Texture of menu
    public Texture2D NoteMenuBlankResult()
    {
        return _NoteMenuBlank.Result;
    }
    //
    // Summary:
    //     Test the menu with buttons of a requested note
    //     Thats the menu for not existing notes
    // Parameters:
    //   key:
    //     Note key
    //   v:
    //     Position on the menu(relative to upper left corner) to be tested.
    //
    // Return Values:
    //   Item id of this click position
    public casId NoteMenuBlankTest(Vector2 v)
    {
        return _NoteMenuBlank.Test(v);
    }
    //
    // Summary:
    //     Update the menu for not existing notes
    // Parameters:
    //   gt:
    //     Gametime
    public void NoteMenuBlankUpdate(GameTime gt)
    {
        _NoteMenuBlank.Update(gt);
    }
    //
    // Summary:
    //     Update the click regions scale for the menu of existing notes
    //     Used when the menu scale changes.
    // Parameters:
    //   scale:
    //     new scale for the click regions.
    public void ClickRegionsUpdateMenuDetails(float scale)
    {
        if (scale == _ClickRegionsNoteMenuScale)
        {
            return;
        }

        _ClickRegionsNoteMenuScale = scale;

        Vector2 posRectangle = new Vector2(223, 9);

        posRectangle = posRectangle * scale;

        Boundary des = new Boundary(posRectangle, (int)((280 - 223) * scale), (int)((56 - 9) * scale));

        _NoteMenuExistsClickable.Add(casId.markPartDelete, des);
        _NoteMenuExistsClickable.Add(casId.markPartCheckbox, des);
    }
    //
    // Summary:
    //   Update Note
    // Parameters:
    //   gt:
    //     GameTime
    //   checkVariant:
    //     calculate the texturevariant for the current used game scenario(like show original or greyscale textures) 
    //   Notekey:
    //     Note key
    public void NoteUpdate(GameTime gt, bool checkVariant, String Notekey)
    {
        CwaNote Note = Notes[Notekey];

        if (checkVariant == false)
        {
            Note.Update(gt, Note.tv);

            return;
        }

        // get the configured/the target texture variant forced by the current scenario
        casTextureVariant tvnew = _tvAllNotes.Value;

        if (tvnew == casTextureVariant.Undef)
        {
            // use the Note specific configuration
            if (_Scenario == Scenario.Original)
            {
                tvnew = Note.cfgTextureVariantOfScene_Original; // use specific of note
                Debug.WriteLine("Overridden tv from note original");
            }
            else
            {
                tvnew = Note.cfgTextureVariantOfScene_Greymix; // use specific of note
                Debug.WriteLine("Overridden tv from note greymix");
            }
        }

        if (tvnew != _tvAllNotes.Value)
        {
            _tvAllNotes.Value = tvnew;
            Debug.WriteLine("Noteupdate new variant");
        }

        // DO NOT CHANGE THE VARIANT AFTERWARDS !

        if (tvnew == casTextureVariant.Hidden)
        {
            // ScrollingBackground has to be informed that the previous texture 
            // is not clickable and visible anymore
            //
            // Remove from ScrollingBackground
            EventBulk_RemoveOuterNoteComplete(Note.dtUnified.ToString(), Note.idBucket);
        }

        Note.Update(gt, tvnew);
    }
}
}