using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace gson
{
    class JsonWriter : IDisposable
    {
        private enum ContainerType
        {
            ROOT,
            OBJECT,
            ARRAY,
        }

        private enum ExpectedType
        {
            NAME,
            VALUE,
            NODE,
            NOTHING
        }

        private const string INDENT = "    ";

        private readonly StreamWriter writer;

        private bool prettyPrint = true;
        private ExpectedType expectedType = ExpectedType.NODE;
        private readonly LinkedList<ContainerType> containers = new();
        private int indentCount = 0;
        private bool needSeparator;

        public JsonWriter(string path)
        {
            writer = new StreamWriter(path);
            containers.AddLast(ContainerType.ROOT);
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public JsonWriter PrettyPrint(bool pretty)
        {
            prettyPrint = pretty;
            return this;
        }

        public JsonWriter BeginObject()
        {
            BeginNode(ContainerType.OBJECT);
            expectedType = ExpectedType.NAME;
            return this;
        }

        public JsonWriter BeginArray()
        {
            BeginNode(ContainerType.ARRAY);
            expectedType = ExpectedType.VALUE;
            return this;
        }

        public JsonWriter EndObject()
        {
            if (expectedType != ExpectedType.NAME)
                throw new InvalidOperationException("unexpected end of object");
            EndNode(ContainerType.OBJECT);
            return this;
        }

        public JsonWriter EndArray()
        {
            if (expectedType != ExpectedType.VALUE)
                throw new InvalidOperationException("unexpected end of array");
            EndNode(ContainerType.ARRAY);

            //expectedType = 
            return this;
        }

        public JsonWriter Name(string name)
        {
            if (name.Length == 0)
                throw new Exception("Name can not be empty");

            if (expectedType != ExpectedType.NAME)
                throw new InvalidOperationException("unexpected name, expecting " + expectedType);

            Separator();
            writer.Write(Quote(name));
            writer.Write(prettyPrint ? " : " : ":");

            expectedType = ExpectedType.VALUE;
            return this;
        }


        public JsonWriter Value(object value)
        {
            CheckNothing();
            if (expectedType == ExpectedType.NAME)
                throw new InvalidOperationException("expecting name");

            ContainerType containerType = containers.Last();
            if (containerType == ContainerType.ARRAY)
                Separator();

            if (value is string s)
            {
                writer.Write(Quote(s));
            }
            else if (isNumeric(value))
            {
                writer.Write(value.ToString());
            }
            else if (value is bool b)
            {
                writer.Write(b ? "true" : "false");
            }
            else
            {
                throw new ArgumentException("invalid value type " + value.GetType() + ", must be numeric primitive, bool or string");
            }

            needSeparator = true;
            if (containerType == ContainerType.OBJECT)
                expectedType = ExpectedType.NAME;
            else if (containerType == ContainerType.ARRAY)
                expectedType = ExpectedType.VALUE;
            else
                expectedType = ExpectedType.NOTHING;

            return this;
        }

        private void BeginNode(ContainerType type)
        {
            CheckNothing();
            if (expectedType == ExpectedType.NAME)
                throw new InvalidOperationException("expecting a property name");
            if (expectedType == ExpectedType.NODE)
                Separator();
            char symbol = type == ContainerType.ARRAY ? '[' : '{';
            if (prettyPrint)
            {
                writer.WriteLine(symbol);
                indentCount++;
            }
            else
            {
                writer.Write(symbol);
            }
            containers.AddLast(type);
            needSeparator = false;
        }

        private void EndNode(ContainerType type)
        {
            CheckNothing();
            if (containers.Last() != type)
                throw new InvalidOperationException("unexpected end of " + type);

            char symbol = type == ContainerType.ARRAY ? ']' : '}';
            if (prettyPrint)
            {
                indentCount--;
                NewLine();
                writer.Write(symbol);
            }
            else
            {
                writer.Write(symbol);
            }
            containers.RemoveLast();

            ContainerType containerType = containers.Last();
            if (containerType == ContainerType.OBJECT)
                expectedType = ExpectedType.NAME;
            else if (containerType == ContainerType.ARRAY)
                expectedType = ExpectedType.VALUE;
            else
                expectedType = ExpectedType.NOTHING;
            needSeparator = true;
        }

        private void Separator()
        {
            if (prettyPrint)
            {
                if (needSeparator)
                {
                    writer.WriteLine(",");
                }
                Indent();
            }
            else if (needSeparator)
            {
                writer.Write(",");
            }
            needSeparator = false;
        }

        private void NewLine()
        {
            writer.WriteLine();
            Indent();
        }

        private void Indent()
        {
            for (int i = 0; i < indentCount; i++)
            {
                writer.Write(INDENT);
            }
        }

        private void CheckNothing()
        {
            if (expectedType == ExpectedType.NOTHING)
                throw new InvalidOperationException("No more values expected");
        }

        // copy paste from https://stackoverflow.com/questions/17862436/is-there-a-c-sharp-equivalent-to-javas-number-class
        private static bool isNumeric(object o)
        {
            return o is sbyte
                    || o is byte
                    || o is short
                    || o is ushort
                    || o is int
                    || o is uint
                    || o is long
                    || o is ulong
                    || o is float
                    || o is double
                    || o is decimal
                    || o is BigInteger;
        }

        private static string Quote(string s)
        {
            return '"' + s + '"';
        }
    }
}
