namespace MoogleEngine;

static public class Data {


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





    public static void DataProcessing() {
        files = FilesMethods.ReadFolder();
        TotalFiles = files.Length;
        
        //! Redimencionar la matrix a la cantidad de documentos que son
        for(int doc = 0; doc < TotalFiles; doc ++) {
            CntWordsForLines.Add( new List<int>() );
        }

        //! Guardar todas las palabras de todos los documentos
        Dictionary<string, bool> Aux = new Dictionary<string, bool>();
        for(int doc = 0; doc < TotalFiles; doc ++)
            FilesMethods.ReadContentFile(files[doc], doc, Aux);
        OriginalWordsDocs.Sort();

        //!  Calcular peso de los documentos
       GetWeigthOfDocs();

        //! Cargar la base de datos de sinonimos
        Synonyms = new WorkingSynonyms("../SynonymsDB/synonyms_db.json");

    }
    public static void GetWeigthOfDocs() {

        for(int doc = 0; doc < TotalFiles; doc ++) {
            // Frecuencia maxima en el documento
            int MaxFreq = 0;
            foreach(var i in PosInDocs[doc]) 
                MaxFreq = Math.Max(MaxFreq, PosInDocs[doc][i.Key].AmountAppareance);
            
            // Calcular el peso de cada palabra en el documento
            foreach(var i in PosInDocs[doc])
                PosInDocs[doc][i.Key].WeigthWord = WordInfo.TFIDF(i.Key, MaxFreq, i.Value.AmountAppareance);
        }

    }

}