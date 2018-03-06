using System.IO;
using System.Xml.Serialization;

namespace renstech.NET.SupernovaDispatcher.Utils
{
    public class XmlFileSerializer<T>
    {
        public XmlFileSerializer(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    
        public T Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));

            using (Stream s = File.OpenRead(Path))
            {
                return (T)xs.Deserialize(s);
            }
        }

        public void Save(T data)
        {
            using (Stream stream = File.Open(Path, FileMode.Create))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(stream, data);
            }
        }
    }
}
