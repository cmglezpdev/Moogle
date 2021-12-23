using System.IO;
namespace MoogleEngine;


public class FilesMethods {
    public static string getNameFile(string file) {
        int StartName = file.Length - 1;
        for( ; file[StartName] != '/' && StartName >= 0; StartName --); StartName ++;

        return file.Substring(StartName, file.Length - StartName - 4);
    }
    public static string getPathFile(string file) {
        string NameFile = getNameFile(file);
        return file.Substring(0, file.Length - NameFile.Length - 4);
    }
    public static string[] ReadFolder() {
        // Leer todos los archivos .txt de la carpeta Content
        string[] files = Directory.GetFiles(@"../Content", "*.txt", SearchOption.AllDirectories);
        return files;
    }

}