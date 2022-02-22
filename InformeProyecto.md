# Moogle!
##### Documentación del Codigo Solución

![](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.
>
>Dev: Carlos Manuel Gonzalez Peña C11

Moogle! es un buscador desarrollado con .Net Core 6.0 usando Blazor como Framework de interfás gráfica.
El obetivo de este buscador es poder realizar consultas de tipo texto en un conjunto de documentos.
Para realizar las consultas se usa el Modelo de Espacio Vectorial (Vectorial Space Model), organizando y mostrando los resultados inteligentemente.

La aplicacion esta dividida en dos grandes Componentes:
     -`MoogleServer:` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.
    - `MoogleEngine:` contiene toda la logica que usa la aplicación para realizar las busquedas.
  
## Implementación MoogleServer

.
.
.

## Implentación MoogleEngine

Este componente cuenta con 10 clases que contienten los algoritmos necesarios para realizar las consultas:
-`SearchItems:` Contiene la informacion de cada documento que se quiere mostrar en los resultados de una búsqueda.
-`SearchResult:` Contiene una lista de `SearchItems` y una sugerencia de consulta para mostrar en los resultados .
-`Moogle:` En ella aparece un metodo Query, la cual tendra todo el proceso(apoyandoce de las demas clases) desde que se recibe la query hasta que se mandan los resultados para renderizarlos.
-`SaveData:` Es una clase en la que se guardaran todos los datos extraidos de los docuentos, o los datos que son necesarios para realizar las consultas.
-`WordInfo:` Es una estructura(```struct```) que guarda la informacion de cada palabra del documento.
-`FilesMethods:` Conjunto de metodos relacionados con el procesamiento de los documentos.
-`AuxiliarMethods:` Conjunto de metodos auxiliares que tienen un uso global.
-`WorkingOperators:` Conjunto de Metodos para el procesamiento de los operadores que aparezcan en la consulta del usuario.
-`WorkingSynonyms:` Clase dedicada al procesamiento de una base de datos de sinonimos de las palabras en español.
-`PorterAlogrithm:` Algoritmo para calcular las raices de las palarbas del español.

#### Procesamiento de Palabras del Corpus
Al arrancar el servidor se ejecuta el metodo `DataProcessing` el cual se encarga de leer todos los documentos y crear una matriz `PosInDocs` donde cada fila corresponde a un documento y las columanas indican las apariciones de esa palabra en el documento. Tambien crea otra matriz `wDocs` indicando por cada palabra en cada documento el peso(relevancia) de esa palabra en el propio documento.
Esta clase contine otro conjunto de datos que se consideran relevantes para el proceso de las consultas:
```cs
    #region Variables
    // Matriz con las apariciones de las palabras en cada documento
    public static List<List<info>> PosInDocs = new List<List<info>>();
    // Indice de las palabras en el orden que fueron apareciendo 
    public static Dictionary<string, int> IdxWords = new Dictionary<string, int>();
    // Todas las palabras entre todos los documentos 
    public static List<string> OriginalWordsDocs = new List<string>();
    // Direccion de todos los documentos de la carpeta Content
    public static string[] files = new string[0];
    // Cantidad total de Documentos
    public static int TotalFiles = 0;
    // Cantidad total de palabras
    public static int TotalWords = 0;
    // Matriz peso de cada palabra en todos los documentos
    public static float[,] wDocs = new float[0,0];
    // Cantidad de palabras por linea de cada documento
    public static List< List<int> > CntWordsForLines = new List<List<int>> ();
    // Estructura que guarda la base de datos de sinonimos
    public static WorkingSynonyms Synonyms = new WorkingSynonyms();
    #endregion
```
Para calcular la relevancia de las palabras en sus respectivos documentos se usa el la el metodo del `TF-IDF` que contiene el `struct` info del documento WordInfo.


#### Procesamiento de la Query
Una vez que el usuario realiza una consulta se ejecuta el metodo `Query` de la clase `Moogle`. Este metodo contiene todo el flujo que se sigue hasta enviar los resultados al server.
Lo primero que se hace es llamar al metodo `FormatQuery` de la clase `AuxiliarMethods`, el cual me "formatea" la query para que en su uso previo sea facil de extraer con solo dividir la query entre espacios en blanco; ademas de que mediante el metodo `ValidOperators`, si un conjuno de operadores esta mal escrito o no tiene sentido o tienes cosas de mas el te devuelve uno que "intenta" deducir lo que quisistes decir. De esta forma, si el usuario escribe `"Todos los !*ALGoritmos de ordenación~existentes"`, 