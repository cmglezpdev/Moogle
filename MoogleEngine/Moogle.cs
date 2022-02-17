namespace MoogleEngine;
using System.Text.Json;

public static class Moogle
{

    public static SearchResult Query(string query)
    {

        // ! Calcular el suggestion por las palabras que no aparecen en el documento
        string suggestion = GetSuggestion(query);

        //! Frecuencia de las palabras de la query
        Dictionary<string, int> FreqWordsQuery = GetFreqWordsInQuery( query );

        //! Matriz peso del query
        float[] wQuery = GetWeigthOfQuery( ref FreqWordsQuery );

        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = GetSimBetweenQueryDocs(wQuery, Data.wDocs);

        // !Modificar el peso de los documentos en base a cada operador del query
        List< Tuple<string, string> > operators = WorkingOperators.GetOperators(query);


        // Guardar los cambios que se le hacen a los pesos de los documentos para despues volverlos al valor inicial
        Dictionary< Tuple<int, int>, float > MemoryChange = new Dictionary<Tuple<int, int>, float>();
        // Realizar los cambios correspondientes a cada operador
        WorkingOperators.ChangeForOperators( operators, MemoryChange, sim);


        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);

        // !Construir el resultado
        SearchItem[] items = BuildResult( sim, FreqWordsQuery, Data.wDocs);

        //! Devolver a los valores originales a los scores que ya fueron modificados 
        NormalizeData(MemoryChange);

        // //! Si no ubieron palabras mal escritas entonces no hay que mostrar sugerencia
        // if(suggestion == query) suggestion = ""; 

        return new SearchResult(items, suggestion);
    }




    #region Methods
    
    private static Dictionary<string, int> GetFreqWordsInQuery( string query ) {
        //! Buscar la frecuencia de las palabras de la query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);

        // Calculamos la frecuencia de las palabras en la query
        Dictionary<string, int> FreqWordsQuery = new Dictionary<string, int>();
        foreach(string w in WordsQuery) {
            string lower = Lemmatization.Stemmer( w.ToLower() );
            if(!FreqWordsQuery.ContainsKey( lower ))
                FreqWordsQuery[ lower ] = 0;
            FreqWordsQuery[ lower ] ++;
        }
    
        return FreqWordsQuery;
    } 
    private static float[] GetWeigthOfQuery(ref Dictionary<string, int> FreqWordsQuery) {
            
        int MaxFreq = 0;
        foreach(KeyValuePair<string, int> PairWordFreq in FreqWordsQuery)
            MaxFreq = Math.Max(MaxFreq, PairWordFreq.Value);


        //! Crear la matriz peso de la query
        float[] wQuery = new float[Data.TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<string, int> wq in FreqWordsQuery) {
            // Si la no existe en las de los documentos
            if(!Data.IdxWords.ContainsKey( wq.Key )) 
                continue;

            wQuery[ Data.IdxWords[wq.Key] ] = info.TFIDF(Data.IdxWords[wq.Key], MaxFreq, wq.Value, ref Data.PosInDocs);
        }

        return wQuery;
    }
    private static Tuple<float, int>[] GetSimBetweenQueryDocs( float[] wQuery, float[,] wDocs){

        Tuple<float, int>[] sim = new Tuple<float, int>[Data.TotalFiles];
        for(int doc = 0; doc < Data.TotalFiles; doc ++) {
            float[] iWDoc = new float[Data.TotalWords];
            for(int w = 0; w < Data.TotalWords; w ++) 
                iWDoc[w] = wDocs[doc, w];
                
            float score = FilesMethods.GetScore( iWDoc, wQuery);
            sim[doc] = new Tuple<float, int>(score, doc);
        }
        
        return sim;
    }
    private static string GetSuggestion(string query) {
        string suggestion = "";

        for(int i = 0; i < query.Length; i ++) {
            // Si es un caracter que no forme una palabra entoces la anadimos a la sugerencia
            if(AuxiliarMethods.Ignore(query[i])) {
                suggestion += query[i];
                continue;
            }

            string w = AuxiliarMethods.GetWordStartIn(query, i);
            string lemman = Lemmatization.Stemmer(w);

            // Si la palabra no esta en el documento entonces no hay que modificarla
            if(Data.IdxWords.ContainsKey(lemman)) {
                suggestion += w;
                i += w.Length - 1;
                continue;
            }

            // En caso de que no este
            // !Antes de usar Levenshtein vemos si tiene sinonimos en la BD -----------------------------------------------------------------------------
            // string[] synonyms = GetSynonyms(w);
            // "../SynonymsDB/synonyms_db.json"



            string newW = "";
            int steps = 100000;
            foreach(KeyValuePair<string, int> wd in Data.IdxWords) {
                int cost = AuxiliarMethods.LevenshteinDistance(w, wd.Key);
                if(cost <= steps) {
                    steps = cost;
                    newW = wd.Key;
                }
            }
            suggestion += newW;
            i += w.Length - 1;
        }


        return suggestion;
    }
    private static SearchItem[] BuildResult( Tuple<float, int>[] sim, Dictionary<string, int> FreqWordsQuery, float[,] wDocs) {
        List<SearchItem> items = new List<SearchItem>();

        for(int i = 0; i < Data.TotalFiles; i ++) {
           // Si ninguna de las palabras estan en el documento     
           if(sim[i].Item1 == 0.00f) continue;

            float score = 0.00f;
            string word = "";
            int doc = sim[i].Item2;

            // Sacar la palabra con mayor score de la query
            foreach(KeyValuePair<string, int> wq in FreqWordsQuery) {
                // Si la palabra no esta entre los documentos
                if(!Data.IdxWords.ContainsKey(wq.Key)) continue;
                // Si la palabra no aparece en ese documento
                if(Data.PosInDocs[doc][ Data.IdxWords[wq.Key] ].AmountAppareance == 0) continue;

                // Sacar la palabra de mayor score
                if(score < wDocs[doc, Data.IdxWords[ wq.Key ]] ) {
                    word = wq.Key;
                    score = wDocs[doc, Data.IdxWords[word]];
                }
            }




            info PosOfWord = Data.PosInDocs[doc][ Data.IdxWords[word] ];

            Random r = new Random();
            int nl = 0, nw = 0;


            (nl, nw) = PosOfWord.nthAppareance( r.Next() % PosOfWord.AmountAppareance );

            string title = FilesMethods.GetNameFile(Data.files[doc]);
            string prev_snippet = FilesMethods.GetContext(doc, nl, nw, 10);

            //* Modificar aqui todas las palabras que aparecen en la query con negrita 
            //* Renderizar en snippet en html para la negrita
            string snippet = "";
            for(int c = 0; c < prev_snippet.Length; c ++) {
                if( AuxiliarMethods.Ignore( prev_snippet[c] ) ) {
                    snippet += prev_snippet[c];
                    continue;
                } 
                string aux = AuxiliarMethods.GetWordStartIn(prev_snippet, c);
                if( FreqWordsQuery.ContainsKey( Lemmatization.Stemmer(aux) ) ) {
                    snippet += ( "<strong>" + aux + "</strong>" );
                } else {
                    snippet += aux;
                }
                c += ( aux.Length - 1 );
            } 


            items.Add(new SearchItem(title, snippet, score));
        }

        return items.ToArray();
    }
    private static void NormalizeData( Dictionary< Tuple<int, int>, float > MemoryChange) {
        foreach(KeyValuePair< Tuple<int, int>, float > mc in MemoryChange) 
            Data.wDocs[ mc.Key.Item1, mc.Key.Item2 ] = mc.Value;
    }

    #endregion
}