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
        string[] files = Directory.GetFiles(@"Content/", "*.txt", SearchOption.AllDirectories);
        return files;
    }

    public static int GetTotalFiles() {
        return ReadFolder().Length;
    }

    private static int[] GetLeftContext(int numLine, int numWord, ref List<int[]> content) { 
        List<int> context = new List<int>();
        
        // SI es la primera palabra no hay contexto derecho
        if(numLine == numWord && numWord == 0) return context.ToArray();

        // Tomar las palabras de esa linea primero para mas comodidad
        for(int i = numWord - 1; i >= 0 && context.Count < 50; i --)
            context.Add(content[numLine][i]);

        // Si es la primera linea del documento ya no hay mas lineas por encima
        if(numLine == 0) return context.ToArray();

        // Tomar el resto de palabras del contexto derecho
        while(--numLine >= 0) {
            int n = content[numLine].Length;
            for(int i = n - 1; i >= 0 && context.Count < 50; i --)
                context.Add(content[numLine][i]);
        }

        context.Reverse();
        return context.ToArray();
    }

    private static int[] GetRightContext(int numLine, int numWord, ref List<int[]> content) { 
        List<int> context = new List<int>();
        
        // SI es la ultima palabra no hay contexto izquierdo
        if(numLine == content.Count - 1 && numWord == content[ content.Count - 1 ].Length - 1) return context.ToArray();

        // Tomar las palabras de esa linea primero para mas comodidad
        for(int i = numWord + 1; i < content[numLine].Length && context.Count < 50; i ++)
            context.Add(content[numLine][i]);

        // Si es la ultima linea del documento ya no hay mas lineas por debajo
        if(numLine == content.Count - 1) return context.ToArray();

        // Tomar el resto de palabras del contexto izquierdo
        while(++ numLine < content.Count) {
            int n = content[numLine].Length;
            for(int i = 0; i < n && context.Count < 50; i ++)
                context.Add(content[numLine][i]);
        }
        
        return context.ToArray();
    }

    public static void ReadContentFile(string file, int idFile, ref Dictionary<string, WordInfo> DocsInfos ) {
        
        // Leer el fichero y sacar la informacion del contenido
        StreamReader archive = new StreamReader(file);
        // Extraer todo el contenido del documento(todas las lineas) y guardarlas como
        //  el conjunto de hashes de las palabras de esa linea
        List<int[]> content = new List<int[]>();
        List<string[]> strContent = new List<string[]>();

        for(string line = ""; line != null; line = archive.ReadLine()) {
            if(AuxiliarMethods.IsLineWhite(line)) continue;
            
            string[] words = AuxiliarMethods.getWordsOfSentence(line);
            strContent.Add(words);
            int[] hashes = new int[words.Length];
            for(int i = 0; i < words.Length; i ++) 
                hashes[i] = AuxiliarMethods.GetHash(words[i]); 

            content.Add(hashes);
        }


    //    TODO: mejorar el metodo de hallar el contexto de una palabra
        // Recorrer todas las lineas del documento  
        for(int i = 0; i < content.Count; i++) { 
            for(int j = 0; j < content[i].Length; j ++) {
                // i, j representan el indice de la linea y el indice de la palabra en la linea respectivamente
                int[] LeftContext = FilesMethods.GetLeftContext(i, j, ref content);
                int[] RightContext = FilesMethods.GetRightContext(i, j, ref content);

                // Comprobar si la palabra esta ya en el diccionario
                if(DocsInfos[ strContent[i][j] ] != null) {
                    DocsInfos[ strContent[i][j] ].AddAppearance(idFile, i, j, LeftContext, RightContext);
                    continue;
                }

                // Si la palabra no esta en el diccionario la agrego por primera vez
                 WordInfo current = new WordInfo(strContent[i][j]);
                 current.AddAppearance(idFile, i, j, LeftContext, RightContext);
                 DocsInfos[ strContent[i][j] ].Clone(current);
            }
        }

    } 




}