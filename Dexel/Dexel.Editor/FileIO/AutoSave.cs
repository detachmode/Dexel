using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;
using Dexel.Model.DataTypes;
using Roslyn.Generators;

namespace Dexel.Editor.FileIO
{
    public static class AutoSave
    {

        private static Timer _autosaveTimer;

        public static void ExecuteAutoSave(MainModel model)
        {

            try
            {
                GetLongestFunctionUnitName(model, name => {
                    var filename = GetAutoSaveFullFilename(name);                   
                    Interactions.SaveToFile(filename, model);
                });
            }
            catch
            {
                // ignore autosave error
            }

        }


        private static void GetLongestFunctionUnitName(MainModel model, Action<string> onName)
        {
            var fu = model.FunctionUnits.OrderByDescending(f => f.Name.Length).FirstOrDefault();
            if (fu == null) return;

            var funame = Names.MethodName(fu);
            onName(funame);
        }


        public static int GetAutoSaveInterval()
        {
            var second = 15;
            return second * 1000;
        }


        private static string GetAutoSaveFullFilename(string functionunitname)
        {
            var maxfilecount = 30;
            var autosavedir = GetAutoSaveDir();          
            var count = GetNextCount(autosavedir, maxfilecount);          
            var path = MakeAutoSavePath(functionunitname, autosavedir.Name, count);
            MaybeDeleteOldestFile(autosavedir, maxfilecount);
            return path;
        }


        private static DirectoryInfo GetAutoSaveDir()
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var autosavedir = Directory.CreateDirectory(dir + @"\autosaves");
            return autosavedir;
        }


        private static string MakeAutoSavePath(string functionunitname, string dir, int count)
        {
            return dir + $"\\{functionunitname}_{count}.yaml";
        }


        private static int GetNextCount(DirectoryInfo autosavedir, int maxfilecount)
        {
            var lastFile = autosavedir.GetFiles()
                 .OrderByDescending(f => f.LastWriteTime)
                 .FirstOrDefault();

            int count = 0;
            if (lastFile == null) return count;

            var filename = Path.GetFileNameWithoutExtension(lastFile.Name);
            var filec = filename.Split('_').Last();
            int.TryParse(filec, out count);
            count = (++count)%maxfilecount;
            return count;
        }


        private static void MaybeDeleteOldestFile(DirectoryInfo autosavedir, int maxfilecount)
        {
            if (autosavedir.GetFiles().Length >= maxfilecount)
            {
                var oldestFile = autosavedir.GetFiles()
                    .OrderBy(f => f.LastWriteTime)
                    .FirstOrDefault();
                if (oldestFile != null) File.Delete(oldestFile.Name);
            }
        }


        public static void Start()
        {

            var timerinterval = GetAutoSaveInterval();
            _autosaveTimer?.Dispose();
            _autosaveTimer = new Timer(
                state => ExecuteAutoSave(null) // TODO: Fixme
                , null, 0, timerinterval);
        }
    }
}
