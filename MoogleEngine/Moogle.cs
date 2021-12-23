namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {

        // Ficheros de la forma (la carpeta en la que estan, nombre del fichero
        string[] files = FilesMethods.ReadFolder();
        // Todas las palabras de query
        string[] WordsQuery = AuxiliarMethods.getWordsOfSentence(query);
        






        



 





        SearchItem[] items = new SearchItem[3] {
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.9f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.5f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.1f),
        };

        return new SearchResult(items, query);
    }
}
