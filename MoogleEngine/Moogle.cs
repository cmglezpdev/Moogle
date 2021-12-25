namespace MoogleEngine;
using System.Diagnostics;

public class BuildDictionary {

    private Dictionary<string, WordInfo> DocsInfos = new Dictionary<string, WordInfo>();
    public BuildDictionary(bool build) {
        if(!build) return;
        string[] files = FilesMethods.ReadFolder();
        // Sacar la informacion de cada documento
        for(int i = 0; i < files.Length; i ++) 
            FilesMethods.ReadContentFile(files[i], i, ref this.DocsInfos);
    }

    public BuildDictionary() {}

    public Dictionary<string, WordInfo> Infos{
        get{return this.DocsInfos;}
    }

}



public static class Moogle
{
    public static SearchResult Query(string query) {

        Stopwatch crono = new Stopwatch();
        crono.Start();
        // Ficheros de la forma (la carpeta en la que estan, nombre del fichero
        string[] files = FilesMethods.ReadFolder();
        int TotalFiles = FilesMethods.GetTotalFiles();
        // Todas las palabras de query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);
    





       // Diccionario para guardar la info de todas las palabras de todos los documentos
        Dictionary<string, WordInfo> DocsInfos = new Dictionary<string, WordInfo>();
        // Sacar la informacion de cada documento
        for(int i = 0; i < files.Length; i ++) {
            FilesMethods.ReadContentFile(files[i], i, ref DocsInfos);
        }
        System.Console.WriteLine("Finished");





        List<SearchItem> AllItems = new List<SearchItem>();
        
        foreach(string w in WordsQuery) {
            if(!DocsInfos.ContainsKey(w)) continue; // Si la palabra no aparece en ningun documento

            for(int i = 0; i < TotalFiles; i ++) {
                List<WordInfo.info> info = DocsInfos[w].InfoWordInDoc(i);
                if(info.Count == 0) continue; // no Existe la palabra en el documento
                
                // Agregamos la palabra a nuestros resultados
                float score = DocsInfos[w].IFIDF(i); // score de la palabra en el documento
                
                string nameFile = FilesMethods.GetNameFile(files[i]); // Nombre del archivo

                // Mostramos cualquier pedazo de oracion en donde aparezca la palabra
                string snippet = DocsInfos[w].GetContext(i, 5);
                
                AllItems.Add(new SearchItem(nameFile, snippet, score));
            }
        }

        // Implementar la ordenacion por el score
        IEnumerable<SearchItem> AuxItems = from item in AllItems orderby item.Score descending select item; 

        List<SearchItem> items = new List<SearchItem>();
        foreach(SearchItem item in AuxItems) {
            items.Add(item);
            System.Console.WriteLine(item.Score);
            if(items.Count == 10) break;
        }
        // End Sort


        crono.Stop();
        System.Console.WriteLine(crono.ElapsedMilliseconds / 1000.00);

         return new SearchResult(items.ToArray(), query);
    }
}


