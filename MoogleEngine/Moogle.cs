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


        System.Console.WriteLine("Etapa #1");


        int MaxFreq = 0;
        // ! Ahora creamos la matris peso de los documentos
        float[,] wDocs = new float[TotalFiles, TotalWords];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            // Maximo de frecuencia entre todas las palabras del documento
            MaxFreq = 0;
            for(int i = 0; i < TotalWords; i ++) 
                MaxFreq = Math.Max(MaxFreq, PosInDocs[doc][i].AmountAppareance);
        
            // Calcular el peso de cada palabra en el documento
            for(int w = 0; w < TotalWords; w ++)
                wDocs[doc, w] = info.TFIDF(doc, w, MaxFreq, ref PosInDocs);
        }



        //! Buscar la frecuencia de las palabras de la query
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

        //! Crear la matriz query
        float[] wQuery = new float[TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            // Si la no existe en las de los documentos
            if(!IdxWords.ContainsKey( wq.Key )) 
                continue;
            wQuery[ IdxWords[wq.Key] ] = info.TFIDF(wq.Value, MaxFreq);

        }

        System.Console.WriteLine("Estapa #2");


        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = new Tuple<float, int>[TotalFiles];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            float[] iWDoc = new float[TotalWords];
            for(int w = 0; w < TotalWords; w ++) 
                iWDoc[w] = wDocs[doc, w];
                
            float score = info.Sim(ref iWDoc, ref wQuery);
            sim[doc] = new Tuple<float, int>(score, doc);
        }






        // //? Ordenamineto de las respuestas por el score








        //! Reconstruir el score despues de ordenado
        float[] scores = new float[TotalFiles];
        foreach(Tuple<float, int> iSim in sim) 
            scores[ iSim.Item2 ] = iSim.Item1;


        // ! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        for(int doc = 0; doc < TotalFiles; doc ++) {
            float[] iWDoc = new float[TotalWords];
            for(int w = 0; w < TotalWords; w ++) 
                iWDoc[w] = wDocs[doc, w];

            scores[doc] = info.Sim(ref iWDoc, ref wQuery);
        }


        // StreamWriter a = new StreamWriter("archive.txt");
        // for(int i = 0; i < TotalFiles; i ++) {  
        //     string s = "";
        //     for(int j = 0; j < TotalWords; j ++)
        //         s += $"{wDocs[i, j]} -- ";
        //     a.WriteLine(s);
        //     a.WriteLine("\n\n\n\n");
        // }   
        // a.Close();

        // ?Ordenar los scores por scores
        // * Implementacion
        // * End Implementation


        // Lista de palabras de la query(sin repeticiones)
        int[] WQuery = new int[FreqWordsQuery.Count];
        int t = 0;
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            WQuery[t ++] = wq.Key;
        }


        System.Console.WriteLine("Estapa #3");


        // ! Construir el resultado
        List<SearchItem> items = new List<SearchItem>();
        
        for(int doc = 0; doc < TotalFiles; doc ++) {
            if(scores[doc]  == 0.00f) continue; // En el documento no existe ninguna palabra de la query
            string snippet = "";

            Random r = new Random();
            // Vamos a seleccionar una de las palabras que aparecen en el documento
            int iWord = 0;

            bool[] isOk = new bool[t];
            bool ok = false;

            while ( !ok ) {
                iWord = r.Next() % t;
                isOk[iWord] = true;
                // Si la palabra no esta entre las de los documentos
                if(!IdxWords.ContainsKey(WQuery[ iWord ]))  continue;

                // Si el documento tiene esa palabra
                info Word =  PosInDocs[doc][ IdxWords[ WQuery[ iWord ] ] ];
                if( Word.AmountAppareance != 0 ) {
                    // Seleccionamos al azar una de sus apariciones en el doc
                    Random r2 = new Random();
                    int kth = r2.Next() % Word.AmountAppareance;
                    int nl, nw;
                    (nl, nw) = Word.nthAppareance(kth);
                    snippet = FilesMethods.GetContext(doc, nl, nw, 5);

                    // Anadir la respuesta a la lista
                    items.Add(new SearchItem(FilesMethods.GetNameFile(files[doc]), snippet, scores[doc]));   
                    ok = true;
                    continue;
                }   
                
                ok = true;
                foreach(bool i in isOk)
                    if(!i) {
                        ok = false;
                        break;
                    }
            }
        }

        System.Console.WriteLine("Estapa #4");




        // List<SearchItem> items = new List<SearchItem>();

        return new SearchResult(items.ToArray(), query);
    }
}


