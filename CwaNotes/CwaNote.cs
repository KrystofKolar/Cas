using Cas;
using CasBase;
using Cwa.Cas;
using CwaCommon;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Cwa
{
    // Summary:
    //     Note class searches for linked files(isostore and medialibrary), 
    //     loading these files, creates new ones and stores all changes.
    //
    //     Note class also creates a menu to check, change texturevariant, delete note
    //
    //     Uses SpriteBatch
    //
    [DataContract]
    public class CwaNote : IDisposable
    {
        //     Indicate if Note was disposed
        [IgnoreDataMember]
        protected bool _disposed;

        // Summary:
        //     Implement IDisposable interface. 
        //     Immediately releases the unmanaged resources and marks managed ones as unused
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Summary:
        //     Helper for the Dipose interface
        //     If its called by the GC, the parameter is false and just the 
        //     unmanaged resources are disposed
        //     If its called manually by calling Dipose(), the parameter is true
        //     and managed and unmanaged resourcs are either released(unmanaged resources)
        //     or marked for the GC to be released.
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
                // managed resources
                if (_TextureTint != null) 
                    _TextureTint.Dispose();

                if (_TextureTintPrev != null) 
                    _TextureTintPrev.Dispose();

                if (Menu != null) 
                    Menu.Dispose();

                if (rt != null) 
                    rt.Dispose();

                if (_texDelete != null) 
                    _texDelete.Dispose();

                if (_texCheck != null) 
                    _texCheck.Dispose();

                if (_texScene != null) 
                    _texScene.Dispose();
            }

            // unmanaged resources
            _disposed = true;
        }

        [IgnoreDataMember]
        DateTime _dtUnified;

        // Summary:
        //     Unified Datetime of the Note. The Bucket is also set here.
        [DataMember]
        public DateTime dtUnified
        {
            get
            {
                return _dtUnified;
            }

            set
            {
                _dtUnified = value;
                idBucket = Cwa.CwaNote.GetBucket(_dtUnified);
            }
        }

        // Summary:
        //     Bucket 48 buckets a day. 15, 45 min each hour
        //     bucket00, 0:15
        //     bucket01, 0:45
        //     bucket02, 1:15
        //     bucket03, 1:45
        //     ..
        //     bucket46, 23:15
        //     bucket47, 23:45
        [DataMember]
        public casId idBucket;

        // Summary:
        //     Position of the bucket (of this note) in the farseer world.
        //     [simulation units]
        [DataMember]
        public Vector2 posSuBucket;

        // Summary:
        //     filenames in the isostore linked to this note
        //     Currently only the 1st file is used
        [DataMember]
        public Dictionary<DateTime, string> Isonames = new Dictionary<DateTime, string>();

        // Summary:
        //     textures loaded from the isostore
        //     the count of Isonames and Textures can differ
        //     just depends if the user has linked an image to the note
        [IgnoreDataMember]
        public Dictionary<DateTime, Texture2D> TexturesIsoLoaded; 

        [DataMember]
        casTextureVariant _tv;

        // Summary:
        //     active texturevariant
        [DataMember]
        public casTextureVariant tv
        {
            get 
            { 
                return _tv; 
            }

            set
            {
                    _tvp = _tv;
                    _tv = value;
            }
        }

        // Summary:
        //     previous texturevariant
        [DataMember]
        casTextureVariant _tvp;

        // Summary:
        //     texture loaded from isostore, then texturevariant
        //     sets the colors, tint etc.
        [IgnoreDataMember]
        Texture2D _TextureTint;

        // Summary:
        //     previous drawn texture
        //     Used for blend effect if texture changes
        [IgnoreDataMember]
        Texture2D _TextureTintPrev;

        // Summary:
        //     texture used to make parts of the note transparent
        //     used to cut out parts
        [IgnoreDataMember]
        Texture2D _texNoteAlpha;

        // Summary:
        //     an extra texture for the note. the pin
        [IgnoreDataMember]
        Texture2D _texNotePin;

        // Summary:
        //     In case the program scene "original" is used, then use this texturevariant
        [DataMember]
        public casTextureVariant cfgTextureVariantOfScene_Original = casTextureVariant.Original;

        // Summary:
        //     In case the program scene "grey" is used, then use this texturevariant
        [DataMember]
        public casTextureVariant cfgTextureVariantOfScene_Greymix = casTextureVariant.Original;

        // Summary:
        //     Fade class to create some fading effect between changes of the image.
        [IgnoreDataMember]
        public Fade FadeScene;

        // Summary:
        //     the rendered note. the resulting texture
        [IgnoreDataMember]
        public RenderTarget2D rt;

        // todo some magic values, to have a nice position of the note
        static float deg = MathHelper.ToRadians(- 7);
        static Vector2 v = new Vector2(9, 40);

        // Summary:
        //    the rendered note menu with buttons to delete, check, change variant..
        [IgnoreDataMember]
        public RenderTarget2D Menu;

        // Summary:
        //    clickable regions in the note menu
        [IgnoreDataMember]
        public Regions<casId> MenuRegions;

        // Summary:
        //    note menu, button delete
        [IgnoreDataMember]
        Texture2D _texDelete;

        // Summary:
        //    note menu, button delete boundary
        [IgnoreDataMember]
        Boundary _rDelete;

        // Summary:
        //    note menu, button check
        [IgnoreDataMember]
        Texture2D _texCheck;

        // Summary:
        //    note menu, button check boundary
        [IgnoreDataMember]
        Boundary _rCheck;

        // Summary:
        //    note menu, button scene
        [IgnoreDataMember]
        Texture2D _texScene;

        // Summary:
        //    note menu, button scene boundary
        [IgnoreDataMember]
        Boundary _rScene;

        // Summary:
        //    set to trigger an update of the note's rendertarget
        [IgnoreDataMember]
        bool _update; 

        // Summary:
        //    set to trigger an update because the picture was loaded/changed.
        [IgnoreDataMember]
        bool _updateCauseLoadPicture;

        // Summary:
        //    set to trigger an update of the note's menu rendertarget
        [IgnoreDataMember]
        bool _updateMenu;

        // Summary:
        //    ref spritebatch
        [IgnoreDataMember]
        SpriteBatch _sb;

        // Summary:
        //    ref graphicsdevice
        [IgnoreDataMember]
        GraphicsDevice _gd;

        // Summary:
        //    ref contentmanager
        [IgnoreDataMember]
        ContentManager _cm;

        // Summary:
        //     Note has no texture (no isostorage files linked), thus its empty.
        //     This member holds a default texture(with its texture variations)
        [IgnoreDataMember]
        Texture2D[] _texNoteIsotexEmpty;

        // Summary:
        //     Its possible to show the unchecked status on the note directly
        //     not only on the note menu
        [IgnoreDataMember]
        Texture2D[] _texNoteIsotexUnchecked;

        // Summary:
        //     Its possible to show the check status on the note directly
        //     not only on the note menu
        [IgnoreDataMember]
        Texture2D[] _texNoteIsotexChecked;

        // Summary:
        //     The menu of the active note.
        [IgnoreDataMember]
        Texture2D[] _texMenu;

        // Summary:
        //     Indicate check status
        [DataMember]
        public bool check;

        // Summary:
        //
        public CwaNote()
        {
        }

        // Summary:
        //     Load content, all textures are loaded here. For the note and the menu.
        //     clickregions are calculated
        //
        //     Values are not checked. Caller has to take care(performance, avoid double check)
        //
        // Parameters:
        //   gd:
        //     GraphicsDevice
        //   sb:
        //     Spritebatch
        //   cm:
        //     ContentManger
        //   texaNoteIsotexEmpty:
        //     default note
        //   texNoteAlpha:
        //     The alpha value for all Notes, the loaded textures and blank notes.
        //   texaNoteIsotexChecked:
        //     check texture on the note
        //   texaNoteIsotexUnChecked:
        //     uncheck texture on the note
        //   texMenu:
        //     note menu
        //   texMenu_ScenarioIter:
        //     note menu, button for scenario changes
        //   texMenu_DeleteNote:
        //     note menu, button for delete a note
        //   texMenu_CheckNote:
        //     note menu, button for check note
        //   texPin:
        //     note has an extra pin drawn above the note                
        //   ptClickableCheckNote:
        //     note menu, clickable region to check a note
        //   ptClickableDeleteNote:
        //     note menu, clickable region to delete a note
        //   ptClickableScenarioIter:
        //     note menu, clickable region to change the scenario
        public virtual void LoadContent(GraphicsDevice gd,
                                        SpriteBatch sb,
                                        ContentManager cm,

                                        Texture2D[] texaNoteIsotexEmpty,
                                        Texture2D texNoteAlpha,

                                        Texture2D[] texaNoteIsotexChecked,
                                        Texture2D[] texaNoteIsotexUnChecked,

                                        Texture2D[] texMenu,
                                        Texture2D texMenu_ScenarioIter,
                                        Texture2D texMenu_DeleteNote,
                                        Texture2D texMenu_CheckNote,

                                        Texture2D texPin,
                                
                                        Point ptClickableCheckNote,
                                        Point ptClickableDeleteNote,
                                        Point ptClickableScenarioIter)

        {
            _gd = gd;
            _sb = sb;
            _cm = cm;

            _texNoteIsotexEmpty = texaNoteIsotexEmpty;
            _texNoteAlpha = texNoteAlpha;

            _texNoteIsotexChecked = texaNoteIsotexChecked;
            _texNoteIsotexUnchecked = texaNoteIsotexUnChecked;

            _texMenu = texMenu;
            _texScene = texMenu_ScenarioIter;
            _texDelete = texMenu_DeleteNote;
            _texCheck = texMenu_CheckNote;

            _texNotePin = texPin;

            FadeScene = new Fade(new Vector2(1F, 1F) * .52F,
                                 true,
                                 1,
                                 new Vector2(0F, 1F),
                                 false,
                                 Vector2.Zero,
                                 Vector2.Zero,
                                 false);

            _update = true;
            _updateMenu = true;
            _updateCauseLoadPicture = false;

            _rCheck = new Boundary(ptClickableCheckNote, texMenu_CheckNote.Width, texMenu_CheckNote.Height);

            _rDelete = new Boundary(ptClickableDeleteNote, texMenu_DeleteNote.Width, texMenu_DeleteNote.Height);

            _rScene = new Boundary(ptClickableScenarioIter, texMenu_ScenarioIter.Width, texMenu_ScenarioIter.Height);

            MenuRegions = new Regions<casId>();

            MenuRegions.Add(casId.markDetail_Clickable_Delete, _rDelete);
            MenuRegions.Add(casId.markDetail_Clickable_IterScenario, _rScene);
            MenuRegions.Add(casId.markDetail_Clickable_Check, _rCheck);

            TexturesIsoLoaded = new Dictionary<DateTime, Texture2D>();

            rt = new RenderTarget2D(_gd,
                                         (int)(_texNoteIsotexEmpty[(int)casTextureVariant.Original].Width),
                                         (int)(_texNoteIsotexEmpty[(int)casTextureVariant.Original].Height));
        }

        // Summary:
        //     Save a filename to a dictionary of filenames.
        //     Not a file is saved just the name.
        //     Currently only one file is used for one date.
        //
        // Parameters:
        //   dt:
        //     Datetime
        //
        //   isoname:
        //     filename
        public void IsonamesAdd(DateTime dt, String isoname)
        {
            Isonames[dt] = isoname;

            SaveIso();
        }

        // Summary:
        //     Load a texture (with the name in "Isonames")
        //     from the isolated storage
        //     to the texture dictionary (TexturesIsoLoaded)
        //
        // Parameters:
        //   request:
        //     Datetime is the key in dictionary of filenames in isolated storage
        //
        // Return Values:
        //     true if file was loaded into "TextureFromIso" dictionary, else false
        public bool LoadPicture(DateTime request)
        {
            try
            {
                string isoname = Isonames[request];
                Rectangle rec = new Rectangle(0, 0, CwaTexNotesManager.TEXWIDTH, CwaTexNotesManager.TEXHEIGHT);

                Texture2D tex = CwaIsolatedStorage.IsolatedStorageHelper.LoadTextureFromIso(_gd, isoname, rec);

                Debug.WriteLine("Loaded picture {0} from iso", isoname);

                TexturesIsoLoaded[request] = tex;
                _updateCauseLoadPicture = true;

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("LoadPicture error: {0}", e.Message.ToString());
                return false;
            }
        }

        // Summary:
        //     Get the bucket id for a date
        //
        //     A day is split into 48 parts. Each part represents
        //     a "bucket"(timespan 30 minutes) and has an id.
        // Parameters:
        //   dt:
        //     date
        //
        // Return Values:
        //     bucket id
        //
        public static casId GetBucket(DateTime dt)
        {
            short bucket = CwaCommon.CwaMathHelper.Get48Bucket(dt);

            return (casId)(casId.markBucket0 + bucket);
        }

        // Summary:
        //     Get the bucket id for a datetime
        //
        //    e.g.: dt=12:55 -> 12:45
        //          dt=01:25 -> 01:15
        // Parameters:
        //   dt:
        //     date
        //
        // Return Values:
        //     bucket id
        //
        public static DateTime GetDateTimeUnified(DateTime dt)
        {
            return CwaCommon.TimeFormatting.GetDateTime_Resolution30MinsCentered(dt);
        }

        // Summary:
        //     Save note with all properties, variables, .. marked with [DataMember] to isostore
        public void SaveIso()
        {
            String fname = CwaTexNotesManager.GetFilenameCwaNote(dtUnified);

            CwaIsolatedStorage.IsolatedStorageProperty<CwaNote> Note = new CwaIsolatedStorage.IsolatedStorageProperty<CwaNote>(fname, new CwaNote());
            Note.Value = this;

            Debug.WriteLine("Saved note {0} to iso", Note.Value.dtUnified);
        }

        // Summary:
        //     Toggle the check status
        //     The change is saved to isostore. Rendertarget is marked as invalid/needs an update
        //
        // Parameters:
        //
        // Return Values:
        //   current check status
        public bool CheckToggle()
        {
            check = !check;

            _update = true;
            _updateMenu = true;

            SaveIso();

            return check;
        }

        // Summary:
        //     Update the rendertarget of the note.
        //     Checks if update is necessary.
        //     Fades in, out when changing. 
        //     Calculates greyscale, show checked status.
        //
        // Parameters:
        //   gameTime:
        //     used to update fade effects
        //   texVariant:
        //      Texture Variant for rendering
        public void Update(GameTime gameTime, casTextureVariant variant)
        {
            if (_update == false &&
                _updateCauseLoadPicture == false &&
                variant == tv &&
                FadeScene.IsStop() && FadeScene.FadeValue >= FadeScene.FadeClamp.Y) // fully faded in
            {
                return; // no update needed
            }

            Texture2D tex = null;
            
            if (TexturesIsoLoaded.Count() > 0)
            {
                // note has a texture, currently 1 is supported only
                tex = TexturesIsoLoaded.First().Value;
            }

            // change of texture variant || new picture
            if (variant != tv || _updateCauseLoadPicture)
            {
                _updateCauseLoadPicture = false;

                FadeScene.StopAndPresetFadeIn();

                #region get the texBaked by creating a texture variant from texOriginal
                if (tex != null)
                {
                    // first time after CHANGE of scenario, you end up here

                    // make a copy of previous texture variant
                    CwaCommon.Texture.Copy(_gd, _TextureTint, out _TextureTintPrev);

                    switch (variant)
                    {
                        case casTextureVariant.Hidden:
                        {
                            _TextureTint = null; //todo better make the alpha value 0
                        }
                        break;

                        case casTextureVariant.Grey:
                        {
                            CwaCommon.Texture.CreateVariationsGreyscaleExt(_gd,
                                                                            tex,
                                                                            out _TextureTint,
                                                                            new Color(0, 0, 20),
                                                                            new Color(0, 0, 0),
                                                                            1F,
                                                                            0F);
                        }
                        break;

                        default:
                        {
                            _TextureTint = tex;
                        }
                        break;
                    }
                    // Common.Texture.ApplyAlpha(texBaked, _texNoteAlpha);
                }
                #endregion

                tv = variant;

            } // tv changed

            if (FadeScene.IsStop() && FadeScene.FadeValue < FadeScene.FadeClamp.Y)
            {
                // lower border reached and stopped - make it fade in
                // todo when occurs this case?
                FadeScene.Direction_UpAndGo(); 
            }
            else
            {
                FadeScene.Update(gameTime);
            }

            Color clr = Color.White * FadeScene.FadeValue;
            Color clrPrev = Color.White * FadeScene.FadeValueInv;

            _gd.SetRenderTarget(rt);
            _gd.Clear(Color.Transparent);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // its null when previous is blank todo check
            if (tex != null)
            {
                if (_TextureTintPrev != null)
                {
                    _sb.Draw(_TextureTintPrev, v, null, clrPrev, deg, Vector2.Zero, 
                        1f, SpriteEffects.None, 0f);
                }

                if (_TextureTint != null)
                {
                    _sb.Draw(_TextureTint, v, null, clr, deg, Vector2.Zero,
                         1f, SpriteEffects.None, 0f);
                }

                _sb.Draw(_texNotePin, new Vector2(92, 0), Color.White);
            }
            // tex == null, means no isostore textures exist. just use default image.
            else
            {
                // draw previous and current note and let it fade in.

                if (_tvp == casTextureVariant.Original ||
                    _tvp == casTextureVariant.Grey ||
                    _tvp == casTextureVariant.White)
                {
                    _sb.Draw(_texNoteIsotexEmpty[(int)_tvp], Vector2.Zero, null, clrPrev);
                }

                if (tv == casTextureVariant.Original ||
                    tv == casTextureVariant.Grey ||
                    tv == casTextureVariant.White)
                {
                    _sb.Draw(_texNoteIsotexEmpty[(int)tv], Vector2.Zero, null, clr);
                }
            }

            _sb.End();
            _gd.SetRenderTarget(null);

            _update = false;
        }

        // Summary:
        //     Update the Menu for the note if necessary.
        //     Menu contains check button, change the note colors or delete it
        //
        // Parameters:
        //   gameTime:
        //     not used
        public void MaybeUpdateMenu(GameTime gameTime)
        {
            if (_updateMenu == false)
            {
                return;
            }

            Color clrTint = Color.White;

            if (Menu == null)  //first time after "hidden" baking, we end up here
            {
                Rectangle rc = _texMenu[(int)casTextureVariant.Original].Bounds;
                Menu = new RenderTarget2D(_gd, rc.Width, rc.Height);
            }

            _gd.SetRenderTarget(Menu);
            _gd.Clear(Color.Transparent);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            _sb.Draw(_texMenu[(int)casTextureVariant.Original],
                    Vector2.Zero,
                    null,
                    Color.White);

            _sb.Draw(check ? _texCheck : _texCheck,
                     _rCheck.Vector2,
                     check ? Color.Red : clrTint);

            _sb.Draw(_texScene, _rScene.Vector2, clrTint);

            _sb.Draw(_texDelete, _rDelete.Vector2, clrTint);

            _sb.End();
            _gd.SetRenderTarget(null);

            _updateMenu = false;
        }
    }
}
