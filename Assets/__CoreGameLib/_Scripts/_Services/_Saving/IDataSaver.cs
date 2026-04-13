using System.Collections;
using System.Collections.Generic;
using _Services._Saving;

namespace Core._Services._Saving {
    public interface IDataSaver {
        IEnumerator LoadData(IKeysStorage keysStorage);
        void SetData(string key, string value);
        void SetData(Dictionary<string, object> map);

        float GetDataFloat(string key);
        int GetDataInt(string key);
        long GetDataLong(string key);
        string GetDataString(string key);
        bool GetDataBool(string key);

        public void DeleteAll();
        public void Delete(string key);

        public IKeysStorage GetKeysStorage();
        void RegisterSaveKey(Dictionary<string, object> defaultValues);
        void RegisterSaveKey(string key, object defaultValue);
    }
}