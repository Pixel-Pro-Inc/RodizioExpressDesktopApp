using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class SerializedObjectManager
    {
        string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BranchId"); //does this mean it only stores things into the branch id file?

        public void SaveData(object serializedData)
        {
            var save = serializedData;

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath))
            {
                binaryFormatter.Serialize(fileStream, save);
            }
        }
        public object RetrieveData()
        {
            object load = null;

            if (File.Exists(savePath))
            {
                var binaryFormatter = new BinaryFormatter();
                using (var fileStream = File.Open(savePath, FileMode.Open))
                {
                    load = (object)binaryFormatter.Deserialize(fileStream);

                    return load;
                }
            }

            return null;
        }
    }
}
