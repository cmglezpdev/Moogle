namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {

        // Ficheros de la forma (la carpeta en la que estan, nombre del fichero
        string[] files = FilesMethods.ReadFolder();
        // Todas las palabras de query
        string[] WordsQuery = AuxiliarMethods.getWordsOfSentence(query);
    

        // Diccionario para guardar la info de todas las palabras de todos los documentos
        Dictionary<string, WordInfo> DocsInfos = new Dictionary<string, WordInfo>();
        // Sacar la informacion de cada documento
        for(int i = 0; i < files.Length; i ++) {
            FilesMethods.ReadContentFile(files[i], i, ref DocsInfos);
        }

        








        SearchItem[] items = new SearchItem[3] {
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.9f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.5f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.1f),
        };

        return new SearchResult(items, query);
    }
}


