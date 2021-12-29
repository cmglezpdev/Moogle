namespace MoogleEngine;
using System.Diagnostics;

public static class Moogle
{
    public static SearchResult Query(string query)
    {

        string[] files = FilesMethods.ReadFolder();
        int TotalFiles = files.Length;

        // Matrix con las repeticiones de las palabras en cada documento
        List<List<info>> PosInDocs = new List<List<info>>();
        // Palabras con su indice en la lista
        Dictionary<int, int> IdxWords = new Dictionary<int, int>();



        //! Redimencionar la matrix de las apariciones en la cantidad de documentos que son
        for(int doc = 0; doc < TotalFiles; doc ++)
            PosInDocs.Add(new List<info> ());

        //! Guardar todas las palabras de todos los documentos en la matrix
        for(int doc = 0; doc < TotalFiles; doc ++)
            FilesMethods.ReadContentFile(files[doc], doc, ref IdxWords, ref PosInDocs);
        
        

        // //! Redimencionar la lista de palabras de todos los documentos al maximo posible
        int TotalWords = IdxWords.Count;
        for(int doc = 0; doc < TotalFiles; doc ++) {
            int n = PosInDocs[doc].Count;
            AuxiliarMethods.Resize(ref PosInDocs, doc, TotalWords - n);
        }

        int MaxFreq = 0;
        // ! Ahora creamos la matris peso de los documentos
        float[,] wDocs = new float[TotalFiles, TotalWords];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            // Maximo de frecuencia entre todas las palabras del documento
            for(int i = 0; i < TotalWords; i ++) 
                MaxFreq = Math.Max(MaxFreq, PosInDocs[doc][i].AmountAppareance);
        
            // Calcular el peso de cada palabra en el documento
            for(int w = 0; w < TotalWords; w ++)
                wDocs[doc, w] = info.TFIDF(doc, w, MaxFreq, ref PosInDocs);
        }


        //! Creamos la matriz peso de la query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);
        MaxFreq = 0;
        // Calculamos la frecuencia de las palabras en la query
        Dictionary<int, int> FreqWordsQuery = new Dictionary<int, int>();
        foreach(string w in WordsQuery) {
            int hash = AuxiliarMethods.GetHashCode(w.ToLower());
            if(!FreqWordsQuery.ContainsKey( hash ))
                FreqWordsQuery[ hash ] = 0;
            FreqWordsQuery[ hash ] ++;
        
            MaxFreq = Math.Max(MaxFreq, FreqWordsQuery[hash]);
        }



        // Crear la matriz query
        float[] wQuery = new float[TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            // Si la palabra existe en el conjunto de palabras que forman el documento
            if(!IdxWords.ContainsKey( wq.Key )) {
                // Continuo ya q no lo necesito
                continue;
            }
            wQuery[ IdxWords[wq.Key] ] = info.TFIDF2(wq.Value, MaxFreq);
        }



        // ! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        float[] scores = new float[TotalFiles];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            float[] iWDoc = new float[TotalWords];
            for(int w = 0; w < TotalWords; w ++) iWDoc[w] = wDocs[doc, w];

            scores[doc] = info.Sim(ref iWDoc, ref wQuery);
        }


        

        // ?Ordenar los scores por scores
        // * Implementacion
        // * End Implementation


        // Lista de palabras de la query(sin repeticiones)
        int[] WQuery = new int[FreqWordsQuery.Count];
        int t = 0;
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            WQuery[t ++] = wq.Key;
        }



        // ! Construir el resultado
        List<SearchItem> items = new List<SearchItem>();
        
        for(int doc = 0; doc < TotalFiles; doc ++) {
            System.Console.WriteLine(scores[doc]);
            if(scores[doc]  == 0.00f) continue; // En el documento no existe ninguna palabra de la query
            
            string snippet = "";

            Random r = new Random();

            // Vamos a seleccionar una de las palabras que aparecen en el documento
            int iWord = 0;

            bool[] isOk = new bool[t];
            bool ok = false;

            // while ( !ok ) {
            //     // System.Console.WriteLine($"Doc #{doc}");
            //      iWord = r.Next() % t;
            //     // Si el documento tiene esa palabra
            //     info Word =  PosInDocs[doc][ IdxWords[ WQuery[ iWord ] ] ];
            //     if( Word.AmountAppareance != 0 ) {
            //         // Seleccionamos al azar una de sus apariciones en el doc
            //         Random r2 = new Random();
            //         int kth = r2.Next() % Word.AmountAppareance;
            //         int nl, nw;
            //         (nl, nw) = Word.nthAppareance(kth);
            //         snippet = FilesMethods.GetContext(doc, nl, nw, 5);

            //         // Anadir la respuesta a la lista
            //         items.Add(new SearchItem(FilesMethods.GetNameFile(files[doc]), snippet, scores[doc]));
                   
            //         // Comprobar si ya todas las posiciones fueron revisadas
                   
            //         ok = true;
            //     }
            // }
        }


        return new SearchResult(items.ToArray(), query);
    }
}


