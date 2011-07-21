/*
 * Reuters XML Search
 * 
 * BinarySerializer.cs
 *
 * Binary Serializer and Deserializer to save objects to file system
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 * This is a new change added to the 'FirstBranch' branch
 */

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VectorModelIRS
{
    public class BinarySerializer
    {
        public static long SerializeObject<T>(string filePath, T objectToSerialize)
        {

            long length = 0;
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                length = stream.Length; 
                stream.Close();                
            }

            return length;

        }

        public static T DeSerializeObject<T>(string filePath)
        {
            T objectToSerialize;
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter bFormatter = new BinaryFormatter();
                objectToSerialize = (T)bFormatter.Deserialize(stream);
                stream.Close();
            }

            return objectToSerialize;
        }
    }
    
}
