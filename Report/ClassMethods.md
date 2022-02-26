# Metodo de las Clases

## Clase SearchItems

Contiene la información de la busqueda de un documento que va a ser renderizado.

```cs
// Propiedades de la clase
public string Title { get; private set; }
public string Snippet { get; private set; }
public float Score { get; private set; }
public string MissingWords { get; private set; }
public string LinkFile {get; private set;}

// Operadores de comparación definidos 
public static bool operator < (SearchItem left, SearchItem right) 
public static bool operator > (SearchItem left, SearchItem right) 
public static bool operator >= (SearchItem left, SearchItem right) 
public static bool operator <= (SearchItem left, SearchItem right) 

// Metodo statico para ordenar un array de elementos(Algoritmo de MergeSort)
public static void Sort(SearchItem[] items)
```

## Clase SearchResults

Contiene una lista de `SearchItems` y una sugerencia de consulta para mostrar en los resultados.

## Clase Moogle

En esta clase aparece un metodo Query, la cual tendra todo el proceso(apoyándose de las demás clases) desde que se recibe la query hasta que se mandan los resultados para renderizarlos.

#### Metodos:
``` cs
public static SearchResult Query(string query);
```

Metodo principal que devuelvea instancia de la clase SearchResult con los resultados de la busqueda.

```cs
private static Dictionary<string, Tuple<int, float>> GetFreqWordsInQuery( string query );
```
Devuelve un diccionario donde a cada palabra de la query le corresponde un par de valores: la frecuencia de la palabra en la query y el score de esa palabra. Este metodo solo calcula la frecuencia de la palabra, el peso se calcula con otro metodo

```cs
public static void GetWeigthOfQuery( Dictionary<string, Tuple<int, float>> FreqWordsQuery)
```

Calcula auxiliandose de la formula del **TFIDF** el peso de cada palabra en el documento.

```cs
private static void UpdateFreqForOperatorRelevance(Dictionary<string, Tuple<int, float>> Freq, string query);
```

Si el usuario introdujo en la consulta el operador de relevancia, entonces aplicamos los cambios correspondientes a este operador sumando una aparición de la palabra afectada por cada asterisco.

```cs
public static Tuple<float, int>[] GetSimBetweenQueryDocs( Dictionary<string, Tuple<int, float>> FreqAndWeigthWordsQuery )
```

Devuelve un arreglo de pares. Calcula la similitud de la query con cada documento de la colección.

```cs
private static (string, string) GetNewQueryAndSuggestion(string query, List<Tuple<string, int>> SynomymsToModif)
```

Devulve dos strings, una nueva query y una sugerencia en caso de que halla alguna palabra mal escrita en la query. Este metodo lo que hace es tomar las palabras que conforman la query original ycomprobar si estan o no en los documentos.
Si la palabra esta en los documentos entoces es una palabra valida para la nueva query, comprobamos la cantidad de documentos en las que aparece, y si son muy pocos nos auxiliamos del diccionario de sinónimos y buscamos el sinónimo que mas apaprezca entre los documentos de nuestra colección.
En cado de que la palabra no aparezca entre los documentos, entoces es que la palabra no existe en el dominio de los documentos o es que está mal escrita. Si la palabra contiene sinonimos en el diccionario entonces hacemos lo mismo que cuando la palabra aparece, en caso contrario, usando el algoritmo de Levenshtein buscamos la palabra dentro del domininio de nuestra colección que tenga un minimo de diferencia con la de nosotros mal escrita(basicamente es buscar la palabra que menos transformaciones halla que hacerle a la nuestra para transformarla en esa) y que mas aparezca entre todos los documentos.
Si la palabra esta mal escrita, entonces la añadimos al string sugerencia y la mostramos al usuario.
Buscar un sinónimo se hace con el objetivo de que si la palabra devuelve muy pocos resultados, al buscar un sinónimo encontremos mas información.

```cs
    private static SearchItem[] BuildResult( Tuple<float, int>[] sim, Dictionary<string, Tuple<int, float>> FreqWordsQuery, string query)
```

Este metodo lo que hace es que, a partir de toda la información buscada y creada con los otros metodos, crea una lista de SearchItem con la informacion de cada documento.
Para el snippet lo que hice fue buscar en el documento cual es el lugar donde mas cerca estaban las palabras que conforman la query que aparecen en ese documento. Tomando a partir de ese punto un radio de **x** palabras a la derecha y a la izquierda se forma el snippet.

<!-- 
- [`SaveData:`](#SaveData) Es una clase en la que se guardarán todos los datos extraídos de los documentos, o los datos que son necesarios para realizar las consultas.
- [`WordInfo:`](#WordInfo) Es una clase que guarda la información de cada palabra del documento, dígase las apariciones de la palbra en un determinado documento y el peso(score) de la palabra proporcionado por el **TF-IDF**.
- [`FilesMethods:`](#FilesMethods) Conjunto de metodos relacionados con el procesamiento de los documentos.
- [`AuxiliarMethods:`](#AuxiliarMethods) Conjunto de metodos auxiliares que tienen un uso global y que no tienen una clasificación especifica entre las demas clases.
- [`WorkingOperators:`](#WorkingOperators) Conjunto de Metodos para el procesamiento de los operadores que aparezcan en la consulta del usuario.
-[`WorkingSynonyms:`](#WorkingSynonyms) Clase dedicada al procesamiento de una base de datos de sinónimos en formato JSON de las palabras en español.
-[`PorterAlogrithm:`](#PorterAlogrithm) Clase con el Stemmer para calcular las raices de las palarbas del español.
## Clase SearchItems -->
