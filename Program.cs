// See https://aka.ms/new-console-template for more information
using gson;

using (var writer = new JsonWriter("test.json"))
{
    writer.PrettyPrint(false);
    writer.BeginObject();
    writer.Name("intValue").Value(42);
    writer.Name("stringValue").Value("test value");
    writer.Name("boolValue").Value(false);
    writer.Name("arrayValue").BeginArray().Value(1).Value(2).Value(3).EndArray();
    writer.Name("nestedObject").BeginObject().Name("a").Value(3.14).Name("b").Value((ushort)63232).EndObject();
    writer.EndObject(); 
}
