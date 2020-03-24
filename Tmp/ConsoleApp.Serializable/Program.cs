using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;



namespace ConsoleApp.Serializable
{
    public static class UtilityLibrary
    {
        public static bool IsSerializable(this object obj)
        {
            if (obj == null)
                return false;

            Type t = obj.GetType();
            return t.IsSerializable;
        }

        public static int CountTuples(this object obj)
        {
            var t = obj.GetType();
            System.Reflection.PropertyInfo[] p = t.GetProperties();
            var s = obj.GetType().GetProperties().Select(property => property.GetValue(obj));
            int n = p.Count();
            return 2;
        }
    }

    [Serializable]
    public class MyObject
    {
        public int n1 = 0;
        public int n2 = 0;
        public String str = null;
    }

    class Program
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/serialization/basic-serialization

        static void Main(string[] args)
        {
            MyObject obj = new MyObject();
            obj.n1 = 1;
            obj.n2 = 24;
            obj.str = "Some String";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("MyFile.bin", 
                                           FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();

            Console.WriteLine("n1: {0}", obj.n1);
            Console.WriteLine("n2: {0}", obj.n2);
            Console.WriteLine("str: {0}", obj.str);

            ValueTuple<string, DateTime, decimal, int> value = 
                ValueTuple.Create("03244562", DateTime.Now, 13452.50m, 45);

            value.CountTuples();

            value.IsSerializable();
            Console.ReadKey();
        }
    }
}
