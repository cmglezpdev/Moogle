namespace MoogleEngine;
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
        // ! Ahora creamos la matriz peso de los documentos
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

        //! Crear la matriz peso de la query
        float[] wQuery = new float[TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            // Si la no existe en las de los documentos
            if(!IdxWords.ContainsKey( wq.Key )) 
                continue;
            wQuery[ IdxWords[wq.Key] ] = info.TFIDF(wq.Value, MaxFreq);
        }



        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = new Tuple<float, int>[TotalFiles];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            float[] iWDoc = new float[TotalWords];
            for(int w = 0; w < TotalWords; w ++) 
                iWDoc[w] = wDocs[doc, w];
                
            float score = info.Sim(ref iWDoc, ref wQuery);
            sim[doc] = new Tuple<float, int>(score, doc);
        }

        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);        

        // ! Construir el resultado
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
            string snippet = FilesMethods.GetContext(doc, nl, nw, 5);

            items.Add(new SearchItem(title, snippet, score));
        }



        return new SearchResult(items.ToArray(), query);
    }
}


