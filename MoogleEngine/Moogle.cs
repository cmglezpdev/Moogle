﻿namespace MoogleEngine;
public static class Moogle
{

#region Variables
    public static List<List<info>> PosInDocs = new List<List<info>>();         // Matrix con las repeticiones de las palabras en cada documento
    public static Dictionary<int, int> IdxWords = new Dictionary<int, int>();     // Palabras con su indice en la lista
    public static string[] files = new string[0];
    public static int TotalFiles = 0;
    public static int TotalWords = 0;
    public static float[,] wDocs = new float[0,0];

    #endregion




    public static SearchResult Query(string query)
    {

        // for(int i = 0; i < TotalFiles; i ++){
        //     int s = 0;
        //     for(int j = 0; j < TotalWords; j ++) {
        //         if(wDocs[i,j] != 0f)
        //             s ++;
        //     }
        //     System.Console.WriteLine(s);
        // }

        // !Frecuencia de las palabras de la query
        Dictionary<int, int> FreqWordsQuery = GetFreqWordsInQuery(query);

        //!  Matriz peso del query
        float[] wQuery = GetWeigthOfQuery( ref FreqWordsQuery );


        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = GetSimBetweenQueryDocs(ref wQuery, ref wDocs);



      //  // !Modificar el peso de los documentos en base a cada operador del query
        // Tuple<string, string>[] operators = FilesMethods.GetOperators(query);


        // for(int doc = 0; doc < TotalFiles; doc ++){

        //     foreach(Tuple<string, string> PairOperWords in operators) {
        //         string opers = PairOperWords.Item1;
        //         string word = PairOperWords.Item2;
        //         int Hash = AuxiliarMethods.GetHashCode(word);

        //         // System.Console.WriteLine( wDocs[doc, IdxWords[Hash]] );


        //         // Si es una palabra que no esta en ningun documento
        //         if(!IdxWords.ContainsKey(Hash)) continue;
                
        //         // recorrer los operadores e ir aplicando uno por uno
        //         foreach(char op in opers) {
                    
        //             switch( op ) {
        //                 //?  La palabra no puede aparecer en ningun documento que sea devuelto 
        //                 case '!':
        //                     // Si la palabra esta en el documento entonces igualamos score a cero para que ese documento no salga
        //                     if( wDocs[doc, IdxWords[Hash]] != 0.00f ) 
        //                         sim[doc] = new Tuple<float, int> (0.00f, doc);
        //                     break;

        //                 //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
        //                 case '^': 
        //                     // Si la palabra no esta en el doc entonces igualamos el score a cero para que ese documento no salga
        //                     if(wDocs[doc, IdxWords[Hash]] == 0.00f)
        //                         sim[doc] = new Tuple<float, int> (0.00f, doc);
        //                     break;

        //                 //?  Aumentar la relevancia de la palabra en el documento
        //                 case '*':
        //                     // Si la palabra aparece en el doc entonces aumentamos un 20% su socre
        //                     if(wDocs[doc, IdxWords[Hash]] != 0.00f)
        //                         wDocs[doc, IdxWords[Hash]] += wDocs[doc, IdxWords[Hash]] * 1f/5f;
        //                     break;
                        
        //                 default: break;
        //             }
        //         }
        //     }
        // }


        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);


        // !Construir el resultado
        SearchItem[] items = BuildResult(ref sim, ref FreqWordsQuery, ref wDocs);
        



        return new SearchResult(items, query);
    }







    #region Methods
    public static void DatesProcessing() {
        files = FilesMethods.ReadFolder();
        TotalFiles = files.Length;
        
        //! Redimencionar la matrix de las apariciones en la cantidad de documentos que son
        for(int doc = 0; doc < TotalFiles; doc ++)
            PosInDocs.Add(new List<info> ());


        //! Guardar todas las palabras de todos los documentos en la matrix
        for(int doc = 0; doc < TotalFiles; doc ++)
            FilesMethods.ReadContentFile(files[doc], doc, ref IdxWords, ref PosInDocs);

        // //! Redimencionar la lista de palabras de todos los documentos al maximo posible
        TotalWords = IdxWords.Count;
        for(int doc = 0; doc < TotalFiles; doc ++) {
            int n = PosInDocs[doc].Count;
            AuxiliarMethods.Resize(ref PosInDocs, doc, TotalWords - n);
        }

       //!  Matriz peso de los documentos
       wDocs = GetWeigthOfDocs();
    }


    private static float[,] GetWeigthOfDocs() {

        float[,] wDocs = new float[TotalFiles, TotalWords];
        
        for(int doc = 0; doc < TotalFiles; doc ++) {
            // Maximo de frecuencia entre todas las palabras del documento
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

    private static Dictionary<int, int> GetFreqWordsInQuery( string query ) {
        //! Buscar la frecuencia de las palabras de la query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);

        // Calculamos la frecuencia de las palabras en la query
        Dictionary<int, int> FreqWordsQuery = new Dictionary<int, int>();
        foreach(string w in WordsQuery) {
            int hash = AuxiliarMethods.GetHashCode(w.ToLower());
            if(!FreqWordsQuery.ContainsKey( hash ))
                FreqWordsQuery[ hash ] = 0;
            FreqWordsQuery[ hash ] ++;
        }
    
        return FreqWordsQuery;
    } 


    private static float[] GetWeigthOfQuery(ref Dictionary<int, int> FreqWordsQuery) {
            
        int MaxFreq = 0;
        foreach(KeyValuePair<int, int> PairWordFreq in FreqWordsQuery)
            MaxFreq = Math.Max(MaxFreq, PairWordFreq.Value);


        //! Crear la matriz peso de la query
        float[] wQuery = new float[TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            // Si la no existe en las de los documentos
            if(!IdxWords.ContainsKey( wq.Key )) 
                continue;

            wQuery[ IdxWords[wq.Key] ] = info.TFIDF(IdxWords[wq.Key], MaxFreq, wq.Value, ref PosInDocs);
        }

        return wQuery;
    }


    private static Tuple<float, int>[] GetSimBetweenQueryDocs(ref float[] wQuery, ref float[,] wDocs){

        Tuple<float, int>[] sim = new Tuple<float, int>[TotalFiles];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            float[] iWDoc = new float[TotalWords];
            for(int w = 0; w < TotalWords; w ++) 
                iWDoc[w] = wDocs[doc, w];
                
            float score = FilesMethods.GetScore(ref iWDoc, ref wQuery);
            sim[doc] = new Tuple<float, int>(score, doc);
        }

        return sim;
    }


    private static SearchItem[] BuildResult(ref Tuple<float, int>[] sim, ref Dictionary<int, int> FreqWordsQuery, ref float[,] wDocs) {
        List<SearchItem> items = new List<SearchItem>();

        for(int i = 0; i < TotalFiles; i ++) {
           // Si ninguna de las palabras estan en el documento     
           if(sim[i].Item1 == 0.00f) continue;

            float score = 0.00f;
            int hash = 0, doc = sim[i].Item2;

            foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
                // Si la palabra no esta entre los documentos
                if(!IdxWords.ContainsKey(wq.Key)) continue;
                // Si la palabra no aparece en ese documento
                if(PosInDocs[doc][ IdxWords[wq.Key] ].AmountAppareance == 0) continue;

                // Sacar la palabra de mayor score
                if(score < PosInDocs[doc][ IdxWords[wq.Key] ].AmountAppareance) {
                    hash = wq.Key;
                    score = wDocs[doc, IdxWords[hash]];
                }
            }
            // Si ninguna de las palabras esta en el documento
            if(hash == 0) continue;
            info word = PosInDocs[doc][ IdxWords[hash] ];

            Random r = new Random();
            int nl = 0, nw = 0;
            (nl, nw) = word.nthAppareance( r.Next() % word.AmountAppareance );

            string title = FilesMethods.GetNameFile(files[doc]);
            string snippet = FilesMethods.GetContext(doc, nl, nw, 20);

            items.Add(new SearchItem(title, snippet, score));
        }

        return items.ToArray();
    }


    #endregion




}


