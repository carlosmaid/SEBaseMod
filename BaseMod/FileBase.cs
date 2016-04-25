using System.IO;
using Sandbox.ModAPI;

namespace BaseMod
{
    public abstract class FileBase
    {
        public string Name { get; protected set; }

        private string _format;

        protected FileBase(string format, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath);

            _format = format;
            Name = string.Format(format, fileName);
            Init();
        }

        protected FileBase() { }

        public void Save(string customSaveName = null)
        {
            string fileName;

            if (!string.IsNullOrEmpty(customSaveName))
                fileName = string.Format(_format, customSaveName);
            else
                fileName = Name;

            SaveData(fileName);
        }

        protected abstract void SaveData(string fileName);
        public abstract void Load();
        public abstract void Create();

        protected void Init()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(Name, typeof (FileBase)))
                Load();
            else
                Create();
        }
    }
}