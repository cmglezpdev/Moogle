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
    // Matrix con las repeticiones de las palabras en cada documento
    public static List<List<info>> PosInDocs = new List<List<info>>();
    public static Dictionary<string, int> IdxWords = new Dictionary<string, int>();     // Palabras con su indice en la lista
    public static List<string> OriginalWordsDocs = new List<string>();
    public static string[] files = new string[0];
    public static int TotalFiles = 0;
    public static int TotalWords = 0;
    public static float[,] wDocs = new float[0,0];
    public static List< List<int> > CntWordsForLines = new List<List<int>> ();
    public static WorkingSynonyms Synonyms = new WorkingSynonyms();
    #endregion
```