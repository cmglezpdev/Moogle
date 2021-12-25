namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {

        // Ficheros de la forma (la carpeta en la que estan, nombre del fichero
        string[] files = FilesMethods.ReadFolder();
        int TotalFiles = FilesMethods.GetTotalFiles();
        // Todas las palabras de query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);
    

        // // Diccionario para guardar la info de todas las palabras de todos los documentos
        Dictionary<string, WordInfo> DocsInfos = new Dictionary<string, WordInfo>();
        // Sacar la informacion de cada documento
        for(int i = 0; i < files.Length; i ++) {
            FilesMethods.ReadContentFile(files[i], i, ref DocsInfos);
        }


        List<SearchItem> items = new List<SearchItem>();


        foreach(string w in WordsQuery) {
            if(DocsInfos[w] == null) continue; // Si la palabra no aparece en ningun documento

            for(int i = 0; i < TotalFiles; i ++) {
                List<WordInfo.info> info = DocsInfos[w].InfoWordInDoc(i);
                if(info.Count == 0) continue; // no Existe la palabra en el documento
                
                // Agregamos la palabra a nuestros resultados
                float score = DocsInfos[w].IFIDF(i); // score de la palabra en el documento
                string nameFile = FilesMethods.GetNameFile(files[i]); // Nombre del archivo

                // Mostramos cualquier pedazo de oracion en donde aparezca la palabra
                string snippet = DocsInfos[w].GetContext(i, 5);
                
                items.Add(new SearchItem(nameFile, snippet, score));
            }

        }

        // Implementar la ordenacion por el score


        // SearchItem[] item = {new SearchItem("Hola", "Es todo lo que hay", 0.5f),
        //                     new SearchItem("Nombre", "Es todo lo que hay", 0.4f),
        //                     new SearchItem("Fereado", "Es todo lo que hay", 0.2f)};

         return new SearchResult(items.ToArray(), query);
        //  return new SearchResult(items.ToArray(), query);
    }
}


