namespace MoogleEngine;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        Stopwatch crono = new Stopwatch();
        crono.Start();

        //! Formatear la query 
        string formatQuery = AuxiliarMethods.FormatQuery( query );

        //! Calcular el suggestion por las palabras que no aparecen en el documento
        List< Tuple<string, int> > SynonymsToModif = new List<Tuple<string, int>> ();
        (query, string suggestion) = GetNewQueryAndSuggestion(formatQuery, SynonymsToModif);

        //! Frecuencia de las palabras de la query
        Dictionary<string, int> FreqWordsQuery = GetFreqWordsInQuery( query );
        //! Metodo que lo unico que hace es aumentar la cantidad de apariciones de la palabra por cada operador * que aparezca
        UpdateFreqForOperatorRelevance(FreqWordsQuery, query); 

        //! Matriz peso del query
        float[] wQuery = GetWeigthOfQuery( ref FreqWordsQuery );
        //! Modificar el peso de las palabras que fueron anadiddas por los sinonimos
        foreach(var v in SynonymsToModif) {
            System.Console.WriteLine("here!!!");
            wQuery[  Data.IdxWords[v.Item1] ] -= (v.Item2 / 100f * wQuery[ Data.IdxWords[v.Item1] ]);
        }

        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = GetSimBetweenQueryDocs(wQuery, Data.wDocs);

        // !Modificar el peso de los documentos en base a cada operador del query
        List< Tuple<string, string> > operators = WorkingOperators.GetOperators(query);

        //! Realizar los cambios correspondientes a cada operador
        WorkingOperators.ChangeForOperators( operators, sim);

        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);

        //! Construir el resultado
        SearchItem[] items = BuildResult( sim, FreqWordsQuery, Data.wDocs, query);

        System.Console.WriteLine(crono.ElapsedMilliseconds);
        crono.Stop();
        
        return new SearchResult(items, suggestion);
    }




    #region Methods
    //* Devuleve la Frequencia de las palabras de la query 
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
    // * Actualiza la frecuencia de las palabras de la query que son afectadas por el operador *
    private static void UpdateFreqForOperatorRelevance(Dictionary<string, int> Freq, string query) {
        string[] partsOfQuery = query.Split(' ');
        for(int i = 0; i < partsOfQuery.Length; i ++) {
            string v = partsOfQuery[i];
            if(!AuxiliarMethods.IsLineOperators(v)) continue;

            int count = 0;
            foreach(char c in v) if(c == '*') count ++;
            if(i + 1 < partsOfQuery.Length) {
                Freq[ Lemmatization.Stemmer(partsOfQuery[i + 1]) ] += count;
            }
        }
    }
    //* Devuleve el vector con los pesos(score) de las palabras del documento
    public static float[] GetWeigthOfQuery(ref Dictionary<string, int> FreqWordsQuery) {
            
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
    //* Devuelve la similitud del vector peso de la query con el de los documentos
    public static Tuple<float, int>[] GetSimBetweenQueryDocs( float[] wQuery, float[,] wDocs){

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
    //* Devuelve una query y una sugerencia nueva apoyandoce de los sinonimos y si la palabra esta mal escrita
    private static (string, string) GetNewQueryAndSuggestion(string query, List<Tuple<string, int>> SynomymsToModif) {
        string suggestion = "";
        string newQuery  = "";

        string[] words = query.Split(' ');
        // Se vuelve true si hay que devolver alguna sugerencia
        bool foundSuggestion = false;

        for(int i = 0; i < words.Length; i ++) {
            if(AuxiliarMethods.IsLineOperators(words[i])) {
                suggestion += (words[i] + ' ');
                newQuery += (words[i] + ' ');
                continue;
            }
            string lemman = Lemmatization.Stemmer(words[i]);

            if(Data.IdxWords.ContainsKey(lemman)) {
                suggestion += (words[i] + ' ');
                newQuery += (words[i] + ' ');
            
                // Si tiene pocas apariciones busco un sinonimo
                double TwentyPercent = (double)((2.00/10.00f) * (double)Data.TotalFiles);
                if( AuxiliarMethods.AmountAppareanceOfWordBetweenAllFiles(lemman) <= TwentyPercent ) {
                    // Si tiene el operador ! o el operador ~ no necesito el sinonimo
                    if( i - 1 >= 0 && (words[i - 1] == "!" || words[i - 1][0] == '~') ) 
                        continue;
                   
                    string[] Syn = Data.Synonyms.GetSynonymsOf(words[i]);
                    if(Syn.Length == 0 ) continue;
        
                    string bestSyn = "";
                    int bestAppar = 0;
                    foreach(string s in Syn) {
                        string lem = Lemmatization.Stemmer(s);
                        if( !Data.IdxWords.ContainsKey(lem) ) continue;
                        int aa = AuxiliarMethods.AmountAppareanceOfWordBetweenAllFiles(lem);
                        if(aa >= bestAppar) {
                            bestAppar = aa;
                            bestSyn = s;
                        }
                    }

                    if(i - 1 >= 0 && AuxiliarMethods.IsLineOperators(words[i - 1]))
                        newQuery += (words[i - 1] + " ");
                    newQuery += (bestSyn = " ");
                    // Anado la palabra a la lista para bajarle el score mas adelante
                    SynomymsToModif.Add(new Tuple<string, int>(bestSyn, 20));
                }
            }
            else {
                // Si no aparece compruebo si tiene algun sinonimo
                if( i - 1 >= 0 && (words[i - 1] == "!" || words[i - 1][0] == '~') ) 
                     continue;
                string[] Syn = Data.Synonyms.GetSynonymsOf(words[i]);
                if(Syn.Length > 0 ) {
                    // Existe sinonimo, asi que busco uno
                    string bestSyn = words[i];
                    int bestAppar = 0;
                    foreach(string s in Syn) {
                        string lem = Lemmatization.Stemmer(s);
                        if( !Data.IdxWords.ContainsKey(lem) ) continue;
                        int aa = AuxiliarMethods.AmountAppareanceOfWordBetweenAllFiles(lem);
                        if(aa >= bestAppar) {
                            bestAppar = aa;
                            bestSyn = s;
                        }
                    }
                    
                    if( bestSyn != words[i] && i - 1 >= 0 && AuxiliarMethods.IsLineOperators(words[i - 1]))
                        newQuery += (words[i - 1] + " ");
                    newQuery += (bestSyn + ' ');
                    // Anado la palabra a la lista para bajarle el score mas adelante
                    SynomymsToModif.Add(new Tuple<string, int>(bestSyn, 50));
                }
                else {
                    if( i - 1 >= 0 && (words[i - 1] == "!" || words[i - 1][0] == '~') ) 
                        continue;
                    // Si no tiene sinonimos entonces veo si esta mal escrita
                    foundSuggestion = true;
                    // System.Console.WriteLine("Here!!");
                    newQuery += (words[i] + " ");
                    // Comprobar si esta mal escrita
                    int bestCost = int.MaxValue;
                    string SugWord = words[i];
                    int AmountAppareance = 0;

                    int umbralChange = 2;
                    foreach(var wd in Data.OriginalWordsDocs) {
                        int cost = AuxiliarMethods.LevenshteinDistance(words[i], wd);
                        if(cost > umbralChange) continue;

                        if(cost < bestCost) {
                            // Quedarme tambien con la de mayor apariciones
                            int aa = AuxiliarMethods.AmountAppareanceOfWordBetweenAllFiles(lemman);
                            if(aa >= AmountAppareance) {
                                bestCost = cost;
                                SugWord = wd;
                            }
                        }
                    }

                    suggestion += SugWord + " ";
                }
            }
        }
        System.Console.WriteLine(foundSuggestion);

        if( foundSuggestion ) {
             if(suggestion[ suggestion.Length - 1 ] == ' ') suggestion = suggestion.Substring(0, suggestion.Length - 1);
        }
        else suggestion = "";
        
        if(newQuery[ newQuery.Length - 1 ] == ' ') newQuery = newQuery.Substring(0, newQuery.Length - 1);

        return (newQuery, suggestion);
    }





    //* Agrupa los resultados de las busquedas en un array
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

            items.Add(new SearchItem(title, snippet, score, missingWords.ToString(), Data.files[doc]) );
        }

        return items.ToArray();
    }

    #endregion
}