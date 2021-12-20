using System.IO;
namespace MoogleEngine;

public class Files{
    private string path;
    private string name;
    public Files(string name, string path) {
        this.path = path;
        this.name = name;
    }
    public string Name{
        get{ return this.name; }
        private set{}
    }
    public string Path{
        get{ return this.path; }
        private set{}
    }
}



public class FilesMethods {
    public static string getNameFile(string file) {
        int StartName = file.Length - 1;
        for( ; file[StartName] != '/' && StartName >= 0; StartName --); StartName ++;

        return file.Substring(StartName, file.Length - StartName - 4);
    }
    public static string getPathFile(string file) {
        string NameFile = getNameFile(file);
        return file.Substring(11, Math.Max(0, file.Length - NameFile.Length - 16));
    }
    public static Files[] ReadFolder() {
        // Leer todos los archivos .txt de la carpeta Content
        // TODO: cambiar el aux por otro nombre
        string[] aux = Directory.GetFiles(@"../Content", "*.txt", SearchOption.AllDirectories);
        Files[] files = new Files[aux.Length];

        for(int i = 0; i < aux.Length; i ++) {
            string name = FilesMethods.getNameFile(aux[i]);
            string Path = FilesMethods.getPathFile(aux[i]);
            
            files[i] = new Files(name, Path);
        }
        return files;
    }


    
    public static void Sort(ref Files[] files, ref string[] words){
        // Si noy hay terminos en la busqueda entonces se devuleve los archivos en el orden en que estan
        if(words.Length == 0) return;
        // Ordenar los ficheros por el metodo de mergeSort
        AuxiliarMethods.MergeSort(ref files, ref words);
    }


}