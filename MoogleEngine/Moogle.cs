namespace MoogleEngine;
using System.Text.Json;
using System.Text;

public static class Moogle
{

    public static SearchResult Query(string query)
    {
        //! Formatear la query 
        string formatQuery = AuxiliarMethods.FormatQuery(query);

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
        
        //! Realizar los cambios correspondientes a cada operador
        WorkingOperators.ChangeForOperators( operators, MemoryChange, sim);


        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);

        //! Construir el resultado
        SearchItem[] items = BuildResult( sim, FreqWordsQuery, Data.wDocs, query);

        //! Devolver a los valores originales a los scores que ya fueron modificados 
        NormalizeData(MemoryChange);

        // //! Si no ubieron palabras mal escritas entonces no hay que mostrar sugerencia
        if(suggestion == query) suggestion = ""; 

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

        string[] words = AuxiliarMethods.GetWordsOfSentence(query);
        List< Tuple<string, string> > operators = WorkingOperators.GetOperators(query);
        

        for(int i = 0; i < query.Length; i ++) {
       
            // Si es un caracter que no forme una palabra entoces la anadimos a la sugerencia
            if(AuxiliarMethods.Ignore(query[i])) {
                suggestion += query[i];
                continue;
            }

            string w = AuxiliarMethods.GetWordStartIn(query, i);
            string lemman = Lemmatization.Stemmer(w);

            //* Si la palabra esta en el documento entonces no hay que modificarla
            if(Data.IdxWords.ContainsKey(lemman)) {
                suggestion += w;
                i += w.Length - 1;
                continue;
            }

            //* En caso de que no este busco todos los sinonimos de esa palabra en la base de datos
            string[] Syn = Data.Synonyms.GetSynonymsOf(w);

            float bestScore = (float)int.MinValue;
            float secondBestScore = (float)int.MinValue;
            string bestWord = "";
            string secondBestWord = "";

            // Quedarme con el que mayor score tiene entre todos los documentos
            foreach(string sin in Syn) {
                string lem = Lemmatization.Stemmer(sin);

                // Si no esta en ningun documento lo omito
                if( !Data.IdxWords.ContainsKey(lem) ) continue;

                // Calculo el score promedio de esa palabra entre todos los documentos
                int idx = Data.IdxWords[ lem ];
                float ScorePromedio = 0.00f;

                for(int doc = 0; doc < Data.TotalFiles; doc ++) 
                    ScorePromedio += Data.wDocs[doc, idx];
                ScorePromedio /= Data.TotalWords;

                if(bestScore < ScorePromedio) {
                    bestScore = ScorePromedio;
                    bestWord = sin;
                }
            }

            // Si encontre un sinonimo para sustituir la palabra que no esta
            if(bestWord != "") {
                System.Console.WriteLine(bestWord);
                suggestion += bestWord;
                i += w.Length - 1;
                continue;
            }


            // * Si tampoco esta en la base de datos entonces veo que esta mas escrita
            foreach(KeyValuePair<string, int> wd in Data.IdxWords) {
                int cost = AuxiliarMethods.LevenshteinDistance(w, wd.Key);

                float umbralChange = (4.00f / 10.00f) * (float)wd.Key.Length;    // 40% de la palabra es el tope maximo a cambiar

                if(cost < secondBestScore) {
                    secondBestScore = cost;
                    secondBestWord = wd.Key;
                }

                if( (float)cost >= umbralChange ) continue;

                if(cost < bestScore) {
                    bestScore = cost;
                    bestWord = wd.Key;
                }
            }

            System.Console.WriteLine(bestWord);
            suggestion += (bestWord != "") ? bestWord : secondBestWord;
             i += w.Length - 1;
       }


        return suggestion;
    }
    private static SearchItem[] BuildResult( Tuple<float, int>[] sim, Dictionary<string, int> FreqWordsQuery, float[,] wDocs, string query) {
        List<SearchItem> items = new List<SearchItem>();
        string[] wordsOfQuery = AuxiliarMethods.GetWordsOfSentence(query);

        for(int i = 0; i < Data.TotalFiles; i ++) {
           // Si ninguna de las palabras estan en el documento     
           if(sim[i].Item1 == 0.00f) continue;

            float score = 0.00f;
            int doc = sim[i].Item2;

            List<string> wordsForCloseness = new List<string>();

            // Sacar la palabra con mayor score de la query
            foreach(KeyValuePair<string, int> wq in FreqWordsQuery) {
                // Si la palabra no esta entre los documentos
                if(!Data.IdxWords.ContainsKey(wq.Key)) continue;
                // Si la palabra no aparece en ese documento
                if(Data.PosInDocs[doc][ Data.IdxWords[wq.Key] ].AmountAppareance == 0) continue;

                // Anadir la palabra para ver donde estan mas cercas
                wordsForCloseness.Add(wq.Key);
            }


            (_, Tuple<int, int>[] positions) = WorkingOperators.ProcessCloseness(wordsForCloseness.ToArray(), doc);
            int LengthSnippet = 100;
            Array.Sort(positions);


            int solCount = 0;
            Tuple<int, int> solPosition = new Tuple<int, int>(positions[0].Item1, positions[0].Item2);

    
            // Ver cual es el subarray que contiene la mayor cantidad de palabras en un diametro equivalente al tamano del snippet
            for(int iword = 0; iword < positions.Length; iword ++) {
                
                int count = 1;
                // Count to left
                int idx = iword - 1;
                while(idx >= 0 && WorkingOperators.DistanceBetweenWords(doc, positions[iword].Item1, positions[iword].Item2, positions[idx].Item1, positions[idx].Item2) <= LengthSnippet/2 ) {
                    idx --;
                    count ++;
                }

                // Count to right
                idx = iword + 1;
                while(idx < positions.Length && WorkingOperators.DistanceBetweenWords(doc, positions[iword].Item1, positions[iword].Item2, positions[idx].Item1, positions[idx].Item2) <= LengthSnippet/2 ) {
                    idx ++;
                    count ++;
                }

                // Quedarme con el snippet que contenga mas palabas
                if(solCount < count) {
                    solCount = count;
                    solPosition = new Tuple<int, int>(positions[iword].Item1, positions[iword].Item2);
                }

            }


            string title = FilesMethods.GetNameFile(Data.files[doc]);
            string prev_snippet = FilesMethods.GetContext(doc, solPosition.Item1, solPosition.Item2, LengthSnippet);

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

            // Comprobar cuales son las palabras de la query que faltan en el documento
            StringBuilder missingWords = new StringBuilder();
            foreach(string w in wordsOfQuery) {
                if(!Data.IdxWords.ContainsKey( Lemmatization.Stemmer( w ) )) {
                    missingWords.Append(w + ", ");
                    continue;
                }
                if(Data.wDocs[ doc, Data.IdxWords[ Lemmatization.Stemmer( w ) ] ] == 0.00f)
                    missingWords.Append(w + ", ");
            }

            if(missingWords.Length != 0) {
                missingWords.Remove( missingWords.Length - 2, 2 );
                missingWords.Insert(0, "<del><i>");
                missingWords.Append("</i></del>");
            }


            items.Add(new SearchItem(title, snippet, score, missingWords.ToString()) );
        }

        return items.ToArray();
    }
    private static void NormalizeData( Dictionary< Tuple<int, int>, float > MemoryChange) {
        foreach(KeyValuePair< Tuple<int, int>, float > mc in MemoryChange) 
            Data.wDocs[ mc.Key.Item1, mc.Key.Item2 ] = mc.Value;
    }

    #endregion
}