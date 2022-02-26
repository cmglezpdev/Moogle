# Metodo de las Clases

## SearchItems

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

## SearchResults

Contiene una lista de `SearchItems` y una sugerencia de consulta para mostrar en los resultados.

## Moogle

En esta clase aparece un metodo Query, la cual tendra todo el proceso(apoyándose de las demás clases) desde que se recibe la query hasta que se mandan los resultados para renderizarlos.

### Metodos

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

## SaveData

Es una clase en la que se guardarán todos los datos extraídos de los documentos, o los datos que son necesarios para realizar las consultas.

```cs
#region Variables
// Matriz con las apariciones de las palabras en cada documento
public static Dictionary<int, Dictionary<string, WordInfo> > PosInDocs = new Dictionary<int, Dictionary<string, WordInfo>> ();
// Indice de las palabras en el orden que fueron apareciendo 
public static List<string> OriginalWordsDocs = new List<string>();
// Direccion de todos los documentos de la carpeta Content
public static string[] files = new string[0];
// Cantidad total de Documentos
public static int TotalFiles = 0;
// Cantidad total de palabras
public static int TotalWords = 0;
// Matriz peso de cada palabra en todos los documentos
public static List< List<int> > CntWordsForLines = new List<List<int>> ();
// Estructura que guarda la base de datos de sinonimos
public static WorkingSynonyms Synonyms = new WorkingSynonyms();
#endregion

// Indexacion y calculo de datos que no dependen de la query
public static void DataProcessing();
//! Calcular el peso del todas las palabras en cada documento
public static void GetWeigthOfDocs();
```

```cs
public static void DataProcessing()l
```

Metodo statico de la clase que es llamado mientras carga el servidor para indexar los documentos. Especificamente lo que hace es leer todo el contenido de cada uno de los documentos y construir el Diccionario `PosInDocs`. También calcula el peso de cada palabra en su respectivo documento y carga en la instancia de clase `Synonyms` de la clase `WorkingSynonyms` la base de datos de los sinonimos.

```cs
public static void GetWeigthOfDocs();
```

Recorre las palabras de cada documento calculando el peso de las mismas en sus documentos

## WordInfo

Es una clase que guarda la información de cada palabra del documento, dígase las apariciones de la palbra en un determinado documento y el peso(score) de la palabra proporcionado por el **TF-IDF**.

```cs
private List<int> numLine = new List<int> ();
private List<int> numWord = new List<int> ();
private float weight = 0;

public int AmountAppareance{
    get{return this.numLine.Count;}
}
public (int, int) nthAppareance(int n);
//! Calcular el TF-IDF de una palabra en un documento
public static float TFIDF(string word, int MaxFreq, int AmountAppareance);
//! RANKING DE LOS DOCUMENTOS
public static float Sim(int doc, Dictionary<string, Tuple<int, float>> wquery);
```

Una instancia de esta clase guarda la información de una palabra. La clase contiene 3 propiedades: El peso de la palabra y dos listas de numeros, las posiciones en las que aparece esta palabra dentro de un documento. Este par esta conformado por el numero identificativo de la linea en la que aparece y la cantiad de palabras que hay por delante de él en esa linea.

También hay 4 metodos: la primera lo que hace es devolver la cantidad de apariciones de esa palabra en el documento, la segunda te devuevle la n-esima aparición de la palabra en el documento y las otras dos son relacionadas al peso y la similitud de las palabras y los documentos respectivamente(el TFIDF para calcular el peso de la palabra y Sim para calcular la similitud entre dos vectores)

## Clase FilesMethods

Esta clase contiene un conjunto de metodos relacionados con el procesamiento de los documentos.

```cs
// Extrae el nombre de un fichero a partir de su ruta
public static string GetNameFile(string file);
// Lee la carpeta Content y retorna todos las rutas de los ficher .txt
public static string[] ReadFolder() ;
// Devuelve el path del documento por su ID
public static string GetFileByID(int idFile);
//! Cantidad de palabras de una oración 
public static int GetAmountWordsInSentence(string line);
```

```cs
public static void ReadContentFile(string file, int idFile, Dictionary<string, bool> OWords );
```

Este Metodo lo que hace es leer el contenido de un archivo, separar las palabras y guardarlas en el diccionario.

```cs
// Devuelve el contexto izquierdo
public static string GetLeftContext(int idFile, int numLine, int numWord, int length, bool addWord);
// Devuelve el conexto derecho 
public static string GetRightContext(int idFile, int numLine, int numWord, int length, bool addWord);
// Devuelve el contexto izquiero + el derecho(contexto total)
public static string GetContext(int idFile, int numLine, int numWord, int length);
```

Estos tres metodos lo que hacen es, dado el **length** del snippet(Contexto de la posición **<numLine, numWord>**), los primeros dos metodos van a esa posición en el documento y extraen **length / 2** palabras cada uno a la izquiera y a la derecha de la posición respectivamente. Luego el tercer metodo une estos dos contextos y retornan el snippet

## Auxiliar Methods

Esta clase contiene un conjunto de metodos auxiliares que tienen un uso global y que no presentan una clasificación especifica entre las demas clases.

```cs
// Extraer todas las palabras de una oracion
public static string[] GetWordsOfSentence(string sentence);
```

Este metodo extrae todas las palabras de una oracion, para eso divide la oración por los espacios en blanco y a cada pedasito le quita por la derecha y por la izquierda lo que no sea parte de una palabra(Palabra es aquel conjunto de caráctes que son letras o números).

```cs
// Formatear la Query convenientemente para trabajar mejor con ella
public static string FormatQuery(string q);
```

Este metodo lo que hace es re-estructurarme la query(sin cambiar el orden de las palabras)  de la siguiente manera:

1. Remplaza los signos de puntuación presentes por espacios en blanco.
2. Elimina todos los espacios en blanco que estan de mas.
3. Separa los operadores de sus palabras a esactamente un espacio en blanco por el medio si estan pegados.
4. Valida el conjunto de operadores. Esto quiere decir que usando el metodo de la clase `ValidOperators` de la clase `WorkingOperators`, valida los operadores y los sustituye por uno que tenga logica(en caso de que no la tenga).
5. Lleva a minusculas todas las palabras

De esta manera si el usuario por ejemplo introduce una query asi: `Las *!TarjETas para entrar   al^ museo de BELLAS~ARTES`, el metodo me devolverá: `las ! tarjetas para entrar al ^ museo de bellas ~ artes`. Nótese que despues de formateada, con solo dividir el string por los espacios en blanco obtendremos las palabras y los operadores, donde si el pedaso[i] son operadores, entonces le corresponden al pedaso[i + 1].

```cs
// True si el string es una cadena de operadores
public static bool IsLineOperators(string l);
// True si el caracter no pertenece a una palabra
public static bool Ignore(char x);
// Comprobar que una linea del fichero es o no una linea en blanco
public static bool IsLineWhite(string line);
// Llevar una palabra a minusculas y sin tildes
public static string NormalizeWord(string word);
// retorna de un string la palabra que empieza en la posicion start
public static string GetWordStartIn(string sentence, int start)
// retorna de un string la palabra que termina en la posición end
public static string GetWordEndIn(string sentence, int end);
// La acantidad de documentos en el que aparece la palabra
public static int AmountAppareanceOfWordBetweenAllFiles(string word);
// True si la palabra esta entre los documentos
public static bool IsWordInDocs(string word);
// Devuelve la palabra que esta a la derecha o a la izquiera de esa posicion
public static string GetWord(string sentence, int pos, string direction);
//! Algoritmo de Levenshtein
public static int LevenshteinDistance(string a, string b) {
    
    int n = a.Length,
        m = b.Length;

    int[,] dp = new int[n + 1, m + 1];

    // El coste de convertir una palabra vacia a una de i caracteres tiene un coste de i pasos
    for(int i = 0; i <= n; i ++) dp[i, 0] = i;
    for(int i = 0; i <= m; i ++) dp[0, i] = i;


    for(int i = 1; i <= n; i ++) {
        for(int j = 1; j <= m; j ++) {
            // Si los caracteres son iguales entonces no hay coste
            int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
            dp[i, j] = Math.Min( dp[i - 1, j] + 1, // Eliminacion
                        Math.Min(dp[i, j - 1] + 1, // Insersion
                        dp[i - 1, j - 1] + cost) );  // Sustitucion
        }
    }

    return dp[n, m];
}
```

Este ultimo metodo, el algoritmo de Levenshtein se usa para calcular la cantidad de transformaciones que hay que hacerle a una palabra para transformarla en otra. Este metodo es un algoritmo recursivo(aunque aqui esta implementado con programación dinamica e iterativamente), que se basa en la idea de que a una palabra se le pueden hacer tres tipos de operaciones: Insertar un caracter en una posición, eliminar un caracter de una posición, o sustituir un caracter por otro.
Usando estas transformaciones podemos decir que d(i, j) es la distacia minima de transformar la palabra de taman i en la de tamaño j.
Luego si comparamos las dos palabras vamos de caracter en caracter preguntando:

- Si los dos caracteres son iguales, entonces no hay que transformar ese caracter, por lo tanto retorna (i - 1, j - 1).
- Si son diferentes entonces tenemos que realizar una transformacion(retorna 1). Luego nos quedarnos con el mínimo de:
  
  - Eliminar el elemento: (i - 1, j) + 1.
  - Insertar un elemento: (i, j - 1) + 1.
  - Sustituir el elemento: (i - 1, j - 1) + 1.

## WorkingOperators

Esta clase contiene un conjunto de Metodos para el procesamiento de los operadores que aparezcan en la consulta del usuario.

```cs
// Comprobar si un caracter es un operador
public static bool IsOperator(char o);
//* Devuelve los operadores que hay en un string en la posicion pos
public static string GetOperators(string sentence, int pos);
//* Devuelve vacio si no es valida, y en otro caso simplifica la expresion a una valida
public static string ValidOperators(string op);
```

```cs
//* Procesar todos los operadores de la query para cada documento
public static void ChangeForOperators( List< Tuple<string, string> > operators,  Tuple<float, int>[] sim);
```

Este metodo recibe el conjunto de pares formados por los operadores y la palabra q afecta. El objetivo del metodo es aplicar los respectivos cambios a los documentos por cada operador.

1. **Cambios del operador ! :** Como la palabra no puede aparecer en ningún documento que sea devuelto, lo que hacemos es que, si encontramos un documento en el que la palabra este, entonces igualamos su similitud a cero, para que no sea devuelto.
2. **Cambios del operador * :** En este momento ya hiciemos los cambios anteriormente, aumentado la frecuencia de la palabra afectada en la query.
3. **Cambios del operador ^ :** Como la palabra tiene que aparecer obligatoriamente en los documentos que sean devuletos, entonces buscamos los documentos en los que no aparezca e igualamos la similitud a cero, para que el documento no sea devuleto, quedando asi solo los que contienen esta palabra.
4. **Cambios del operador ~ :** Este operador afecta a dos o mas palabras, por lo que se guarda como una secuencia de palabras. Para este operador lo que hacemos es sacar todas las palabra que tienen que estar cerca simultaneamente y:
    - Se va por cada documento seleccionando las palabras que aparecen en este.
    - Si la palabra esta afectada por algún operador(distinto del de cercania claro) entoces realizamos los cambios de dicho operador.
    - Luego, si la cantidad de palabras que aparece en el documento es mayor o igual a dos(para poder establecer relación de cercanía entre las palabras), se busca en el documento la distancia minima entre las palabras, o sea, lo mas cerca que están en el documento una de otras.
    - Luego se le aplican los cambios pertinentes a la similitud de cada documento basándonos en lo siguente:
        - Si entre las palabras que deben aparecer cerca, hay un documento que tiene  menos palabras que otro(porque hay palabras que no aparecen en un documento y en otros si), es muy probable que el documento con menos palabras tenga una cercanía mas chiquita que el que tiene mas palabras. Como mientras mas palabras tenga la cercanía, mas relevante debe de ser el documento, lo que hacemos es dividir la cantidad de palabras originales que deben de estar cerca entre la cantidad de esas palabras que estan en el documento, y lo multiplicamos por el socre que ya teníamos.
        - Mientras mas cerca esten las palabras, mas relevante sera el documento. Para esto dividimos 100 entre la cercanía y se lo sumamos al score(similitud) que ya teníamos. Esto lo que hace es que mientras mas pequeño sea la cerania, mayor sera la división y mas relevante sera el documento.

```cs
//* Funcion Auxiliar paraCalcular la distancia entre dos palabras del documento
public static int DistanceBetweenWords(int doc, int x1, int y1, int x2, int y2);
//* Funcion Auxiliar que recibe un operador y realiza los cambios pertinentes
public static void ProcessOperator(char op, string word, int doc, Tuple<float, int>[] sim);
```

```cs
//* Funcion Auxiliar para Calcula a cercania del conjunto de palabras por cada documento
public static (float, Tuple<int, int>[]) ProcessCloseness( string[] wordsForCloseness, int doc );
```

Este metodo calcula la cercania entre un conjunto de palabras y devuelve la cercania minima y las posiciones que la conforman. La idea es ordenar los pares en orden no-decreciente y con ayuda de dos punteros **l** y **r**:

1. Se corre el puntero **r** a la derecha y se va guardando las posiciones en arreglo y contando las apariciones dentro de ese rango hasta que tengamos la cantidad de palabras necesarias para la cercania. Luego tenemos que la ultima palabra tiene esactamente una aparición. Ahora corremos el puntero **l** hacia la derecha y vamos restando apariciones de de las posicione hasta que llegemos a una que no se pueda eliminar porque se incompleta el arreglo. LLegado a este punto tenenemos un subarreglo de tamaño minimo donde estan todas las palabras.
2. Ahora calcularmos la distancia entre las apariciones(contando solo una aparición por palabra)
3. Comparamos el resultado con uno que ya tengamos y nos quedamos con el minimo y en caso de que el minimo sea el que acabamos de calcular, guardamos el array tambien.
4. Movemos el puntero **l** una posición a la derecha para descompletar el array y volvemos a realizar los pasos desde el primero una y otra vez hasta que se acaben las posiciones.

Luego solo tenemos que retornar la cercania mínima y la lista de las posiciones.

## WorkingSynonyms

Clase dedicada al procesamiento de una base de datos de sinónimos en formato JSON de las palabras en español.

```cs
private class synonyms {
    public List<string[]>? sinonimos { get; set; }
}
private List<string[]> Synonyms;
public WorkingSynonyms( string pathdb );

public WorkingSynonyms();

public int LengthDB {
    get{return this.Synonyms.Count;}
}

public string[] GetSynonymsOf( string word );
```

Se crea una clase synonyms con una propiedad de tipo `List<string[]>` para guardar la estructura del JSON de los Sinonimos. Luego tenemos dos constructores, una propiedad que es el tamaño del diccionario y un metodo que dado una palabra, devuelve una lista de sinonimos de la misma.

## PorterAlgorithm

Porter Algorithm es una metodo para calcular la raiz de una palabra, permitiendo asi poder crear familias de palabras que pueden estar relacionadas.

```cs
ublic static string Stemmer(string w);
```

Esta clase contiene un metodo llamado Stemmer que dado una palabra, ejecuta un conjunto de reglas(las reglas son diferentes en dependencia del idioma de stemmer) que reducen la palabra a su raíz.
