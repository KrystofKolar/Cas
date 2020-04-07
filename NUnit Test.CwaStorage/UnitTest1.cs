using CasStorage;
using Cwa;
using CwaIsolatedStorage;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;
using System.IO;

namespace NUnit_Test.CwaStorage
{
    public static class Common
    {
        public static readonly string Filename = "CAS.json";
        public static readonly string DirectoryTest = @"\CwaNunit";
    }

    // [TestFixture] attribute marks the class that this class contains test methods.
    [TestFixture]
    public class TestCAS
    {
        public TestCAS()
        {
            // Create Subdirectory and default "Package"
            CwaSystemIO.IO.isf += Common.DirectoryTest;

            if (Directory.Exists(CwaSystemIO.IO.isf))
                Directory.Delete(CwaSystemIO.IO.isf, true);

            Directory.CreateDirectory(CwaSystemIO.IO.isf);
            IsfPropertyBase.props = new IsfPropertiesPackage(Common.Filename);
        }

        [SetUp]
        public void Setup()
        {

        }

        [TestCase]
        public void When_JustSettings_FileExists()
        {
            Assert.IsTrue(File.Exists(Path.Combine(CwaSystemIO.IO.isf, Common.Filename)));
        }

        [TestCase]
        public void When_PropertyInteger_ValuesSaved()
        {
            IsfProperty<int>
            Property_Integer = new IsfProperty<int>(
                "KeyInteger",
                111);

            Assert.IsTrue(Property_Integer.Value == 111);

            Property_Integer.Value = 12345;

            Assert.IsTrue(Property_Integer.Value == 12345);
        }



        [TestCase]
        public void When_PropertyEnum_ValuesSaved()
        {
            IsfProperty<CasBase.Scenario>
            Property_Scenario = new IsfProperty<CasBase.Scenario>(
                "KeyEnumScenario",
                CasBase.Scenario.GreyMix);
            Assert.IsTrue(Property_Scenario.Value == CasBase.Scenario.GreyMix);

            Property_Scenario.Value = CasBase.Scenario.GreyDepth;

            Assert.IsTrue(Property_Scenario.Value == CasBase.Scenario.GreyDepth);
        }

        [TestCase]
        public void When_Note_Created()
        {
            DateTime dt = new DateTime(2020, 12, 11, 10, 9, 8);
            string filename = CwaNotesBase.Root.GetFilenameCwaNote(dt);

            IsfProperty<CwaNote> Property_Note = new CwaIsolatedStorage.IsfProperty<CwaNote>(
                filename,
                new CwaNote());

            Assert.IsTrue(CwaSystemIO.IO.FileExists(filename));

            CwaNote note = (CwaNote)IsfPropertyBase.props.Get(filename);

            note.posSuBucket = new Microsoft.Xna.Framework.Vector2(123, 456);
            note.Isonames.Add(DateTime.Now, "file0");
            //note.Isonames.Add(DateTime.Now.AddHours(1), "file1");
            //note.Isonames.Add(DateTime.Now.AddHours(2), "file2");
            //note.Isonames.Add(DateTime.Now.AddHours(3), "file3");
            //note.Isonames.Add(DateTime.Now.AddHours(4), "file4");
            //note.Isonames.Add(DateTime.Now.AddHours(5), "file5");
            Property_Note.Value = note;

            Assert.IsTrue(Property_Note.Value.Isonames.Count == note.Isonames.Count);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void When_Note_Compare_Isonames_posSuBucket_Values(int count)
        {
            CwaNote note = new CwaNote();
            note.posSuBucket = new Vector2(1, 11) * count;

            for (int i = 0; i < count; ++i)
            {
                note.Isonames.Add(DateTime.Now.AddDays(i), $"file{i}");
            }

            IsfProperty<CwaNote> Property_Note = new IsfProperty<CwaNote>(
                CwaNotesBase.Root.GetFilenameCwaNote(DateTime.Now),
                new CwaNote());
            Property_Note.Value = note;

            Assert.IsTrue(Property_Note.Value.posSuBucket == new Vector2(1, 11) * count);
            Assert.IsTrue(Property_Note.Value.Isonames.Count == count);

        }
    }
}