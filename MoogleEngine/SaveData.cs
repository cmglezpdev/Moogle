namespace MoogleEngine;

static public class Data {


    #region Variables
    // Matriz con las apariciones de las palabras en cada documento
    public static Dictionary<int, Dictionary<string, info> > PosInDocs = new Dictionary<int, Dictionary<string, info>> ();
    // Indice de las palabras en el orden que fueron apareciendo 
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





    public static void DataProcessing() {
        files = FilesMethods.ReadFolder();
        TotalFiles = files.Length;
        
        //! Redimencionar la matrix de las apariciones en la cantidad de documentos que son
        for(int doc = 0; doc < TotalFiles; doc ++) {
            CntWordsForLines.Add( new List<int>() );
        }


        //! Guardar todas las palabras de todos los documentos en la matrix
        Dictionary<string, bool> Aux = new Dictionary<string, bool>();
        for(int doc = 0; doc < TotalFiles; doc ++)
            FilesMethods.ReadContentFile(files[doc], doc, Aux);
        OriginalWordsDocs.Sort();

        // //! Redimencionar la lista de palabras de todos los documentos al maximo posible
        TotalWords = IdxWords.Count;
        for(int doc = 0; doc < TotalFiles; doc ++) {
            int n = PosInDocs[doc].Count;
            AuxiliarMethods.Resize(PosInDocs, doc, TotalWords - n);
        }

        //!  Matriz peso de los documentos
        wDocs = GetWeigthOfDocs();

        //! Cargar la base de datos de sinonimos
        Synonyms = new WorkingSynonyms("../SynonymsDB/synonyms_db.json");

    }
    public static float[,] GetWeigthOfDocs() {

        float[,] wDocs = new float[TotalFiles, TotalWords];
        
        for(int doc = 0; doc < TotalFiles; doc ++) {
            // Frecuencia maxima en el documento
            int MaxFreq = 0;
            for(int i = 0; i < TotalWords; i ++) 
                MaxFreq = Math.Max(MaxFreq, PosInDocs[doc][i].AmountAppareance);

            // Calcular el peso de cada palabra en el documento
            for(int IdxW = 0; IdxW < TotalWords; IdxW ++) {
                wDocs[doc, IdxW] = info.TFIDF(IdxW, MaxFreq, PosInDocs[doc][IdxW].AmountAppareance, ref PosInDocs);
            } 
        }

        return wDocs;
    }

}