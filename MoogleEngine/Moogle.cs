namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {

        // Ficheros de la forma (la carpeta en la que estan, nombre del ficher)
        Files[] files = FilesMethods.ReadFolder();
        // Todas las palabras de query menos las que solo son de una letra
        string[] WordsQuery = AuxiliarMethods.getWordsOfSentence(query);
        // Ordenar los ficheros priorizando los ficheros que tengan algun termino de busqueda en su nombre
        FilesMethods.Sort(ref files, ref WordsQuery);






 





        SearchItem[] items = new SearchItem[3] {
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.9f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.5f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.1f),
        };

        return new SearchResult(items, query);
    }
}
