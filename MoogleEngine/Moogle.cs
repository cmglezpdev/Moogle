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
        List< Tuple<string, string> > operators = FilesMethods.GetOperators(query);


        // Guardar los cambios que se le hacen a los pesos de los documentos para despues volverlos al valor inicial
        Dictionary< Tuple<int, int>, float > MemoryChange = new Dictionary<Tuple<int, int>, float>();
        // Realizar los cambios correspondientes a cada operador
        ChangeForOperators( operators, MemoryChange, sim);


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
            string lower = AuxiliarMethods.NormalizeWord(w);
                   lower = Lemmatization.Stemmer(lower);
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

            string w = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWordStartIn(query, i));
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
            string snippet = FilesMethods.GetContext(doc, nl, nw, 10);

            items.Add(new SearchItem(title, snippet, score));
        }

        return items.ToArray();
    }
    private static void NormalizeData( Dictionary< Tuple<int, int>, float > MemoryChange) {
        foreach(KeyValuePair< Tuple<int, int>, float > mc in MemoryChange) 
            Data.wDocs[ mc.Key.Item1, mc.Key.Item2 ] = mc.Value;
    }
    private static void ChangeForOperators( List< Tuple<string, string> > operators, Dictionary< Tuple<int, int>, float > MemoryChange,  Tuple<float, int>[] sim) {

        for(int doc = 0; doc < Data.TotalFiles; doc ++){

                          
            // Guardar todas las posiciones de las palabras en un array para ordenarlos 
            List< Tuple<int, int, string> > posiciones = new List< Tuple<int, int, string> > ();
            // Diccionario para registrar las apariciones durante la busqueda
            Dictionary< string, int > cnt = new Dictionary< string, int >();
            // Guardar los intervalos en donde estan todas las palabras
            List< Tuple<int, int> > Interv = new List< Tuple<int, int> > ();
            // Marcador para saber si durante el procesamiento de este documento habia algun operador de cercania
            // para poder agregar los cambios
            bool workWithClossenes = false;


            foreach(Tuple<string, string> PairOperWords in operators) {
                string opers = PairOperWords.Item1;
                string word = PairOperWords.Item2;

                // Si es una palabra que no esta en ningun documento..
                // El operador tiene que ser diferente al de cercania porque en ese operador se guarda
                // mas de una palabra y ese string nunca va a estar en el diccionario
                if(opers != "~" && !Data.IdxWords.ContainsKey(word)) continue; 
                
                // recorrer los operadores e ir aplicando uno por uno   
                foreach(char op in opers) {
                    
                    switch( op ) {
                        //?  La palabra no puede aparecer en ningun documento que sea devuelto 
                        case '!':
                            ProcessOperators('!', word, doc, MemoryChange, sim);
                            break;
                        //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
                        case '^': 
                            ProcessOperators('^', word, doc, MemoryChange, sim);
                            break;
                        //?  Aumentar la relevancia del documento que tiene esa palabra
                        case '*':
                            ProcessOperators('*', word, doc, MemoryChange, sim);
                            break; 
                        
                        //? Aumentar la relevancia del documento mientras mas cercanas esten las palabras
                        case '~':

                            List<string> wordsForCloseness = new List<string>();
                            List< Tuple<string, string> > OpersAndWords = FilesMethods.GetOperators(word); 
                            string[] SubWords = AuxiliarMethods.GetWordsOfSentence(word);

                            //? Poner en las palabras para la cercania las que no tengan operador
                            for(int wi = 0; wi < SubWords.Length; wi ++) {
                                
                                if(!Data.IdxWords.ContainsKey(SubWords[wi])) continue;
                                
                                // Si no es una palabra de las que tienen operadores entonces la agrego a la lista si aparece en el documento
                                bool found = false;
                                foreach(Tuple<string, string> g in OpersAndWords) 
                                    if(g.Item2 == SubWords[wi]) {
                                        found = true;
                                        break;
                                    }
                                
                                if( found ) continue;
                                
                                if( Data.PosInDocs[doc][ Data.IdxWords[ SubWords[wi] ] ].AmountAppareance > 0 )
                                    wordsForCloseness.Add(SubWords[wi]);
                            }


                            //? Trabajar con las palabras con operadores y ponerlos en la  cercania
                            for(int i = 0; i < OpersAndWords.Count; i ++) {
                                string o = OpersAndWords[i].Item1;
                                string w = OpersAndWords[i].Item2;
                                
                                if(!Data.IdxWords.ContainsKey(w)) continue;

                                // Si la palabra no esta en el documento entonces la omitimos para la cercania
                                if( Data.PosInDocs[doc][ Data.IdxWords[w] ].AmountAppareance == 0 )
                                    continue;

                                if(o == "!") {
                                    // Si la palabra aparece en el documento entonces el operador de cercania de todas las palabras queda invalidado
                                    if( Data.PosInDocs[doc][ Data.IdxWords[w] ].AmountAppareance > 0 ) {
                                         ProcessOperators('!', w, doc, MemoryChange, sim);
                                        break;
                                    }
                                     continue;                                    
                                 }

                                // Solo nos queda por aplicar los operadores ^ y * en caso de que los tenga
                                foreach(char x in o) 
                                    ProcessOperators(x, w, doc, MemoryChange, sim);

                                // anadimos las palabras a una lista para calcular la cercania
                                wordsForCloseness.Add(w);
                            }

                            if(wordsForCloseness.Count <= 1) // Si no hay al menos dos palabras para la cercania
                                continue;
                            
                            // *************************************************************************************
                            workWithClossenes = true; // Hay que realizar los cambios

                            foreach(string words in wordsForCloseness) {
                                int n = Data.PosInDocs[doc][ Data.IdxWords[words] ].AmountAppareance;
                                for(int i = 0; i < n; i ++) {
                                    (int x, int y) = Data.PosInDocs[doc][ Data.IdxWords[words] ].nthAppareance(i);
                                    posiciones.Add( new Tuple<int, int, string>( x, y, words ) );
                                }
                            }
                            
                            // Ordenar las posiciones por posiciones de menor a mayor
                            posiciones.Sort();

                            int cantWords = wordsForCloseness.Count;

                            int l = 0, r = 0;
                            while(true) {

                                // Anadir apariciones hasta que esten todas las palabras
                                while( cnt.Count < cantWords  ) {
                                    // Si estamos en el final de todas las posiciones
                                    if(r == posiciones.Count) break;
                                    
                                    // Aumentamos una aparicion
                                    if( !cnt.ContainsKey( posiciones[r].Item3 ) )
                                        cnt[ posiciones[r].Item3 ] = 0;
                                    cnt[ posiciones[r++].Item3 ] ++;
                                }
                                r --;

                                // Si no hay la cantidad de palabras esactas entonces es q r llego al final y no encontro otro conj
                                if(cnt.Count != cantWords )
                                    break;


                                // Eliminar apariciones de la izquierda hasta tener las minimas indispensables
                                while( l < r ) {
                                    // Si ya no se pueden eliminar mas palabras
                                    if( cnt[ posiciones[l].Item3 ] - 1 <= 0) {
                                        break;
                                    }
                                    // Si puedo seguir eliminando borro la aparicion esa
                                    cnt[ posiciones[l++].Item3 ] --;
                                } 

                                // Guardamos los intervalos en donde aparecen todas las palabras comparar el intervalo mas cercano
                                Interv.Add( new Tuple<int, int> (l, r) );

                                // Si r llego al final paramos la busqueda
                                if(r == posiciones.Count - 1) break;
                                // Eliminamos esa primera posicion del intervalo para que encuentre otro completo
                                cnt.Remove( posiciones[l ++].Item3 );
                                r ++;
                            }
                        break;

                        default: break;
                    }
                }
            }

            // Si no hay que hacerle cambios al documento que implique la cercania
            if( !workWithClossenes ) continue;


           // Limpiar cnt para reutilizarlo
           cnt.Clear();

            int minDistance = int.MaxValue;


            // Recorrer los intervalos en busca del mas cercano
            foreach( Tuple<int, int> i_interv in Interv ) {

                int l = i_interv.Item1;
                int r = i_interv.Item2;

                int distance = 0;
                int prevx =  posiciones[l].Item1, prevy = posiciones[l].Item2;

                for(int i = l; i <= r; i ++) {
                    if( cnt.ContainsKey(posiciones[i].Item3) )
                        continue;
                    cnt[ posiciones[i].Item3 ] = 1;
                    
                    distance += DistanceBetweenWords(prevx, prevy, posiciones[i].Item1, posiciones[i].Item2);
                    prevx = posiciones[i].Item1;
                    prevy = posiciones[i].Item2;
                }
                minDistance = Math.Min( minDistance, distance );
                
            }
            float score = sim[doc].Item1;
            sim[doc] = new Tuple<float, int> ( score + 1.00f / (float)minDistance, doc);

        }
        // *******************************************************************************************

    }
    private static int DistanceBetweenWords(int x1, int y1, int x2, int y2) {
        int distance =  Math.Abs(x1 - x2);
        // Si estan en la misma linea resto sus posiciones
        if(x1 == x2) return distance + Math.Abs(y1 - y2);
        
        // Con x1 <= x2 && y1 <= y2
        return x2 + Math.Max(0, 15 - x1); // 15 es un valor promedio de la cantidad de palabras por linea
    }
    static private void ProcessOperators(char op, string word, int doc, Dictionary< Tuple<int, int>, float > MemoryChange, Tuple<float, int>[] sim) {
        switch( op ) {
            //?  La palabra no puede aparecer en ningun documento que sea devuelto
            case '!':
                // Si la palabra esta en el documento entonces igualamos score a cero para que ese documento no salga
                if( Data.wDocs[doc, Data.IdxWords[word]] > 0.00f )  
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
            case '^': 
                // Si la palabra no esta en el doc entonces igualamos el score a cero para que ese documento no salga
                if(Data.wDocs[doc, Data.IdxWords[word]] == 0.00f)
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            // //?  Aumentar la relevancia del documento que tiene esa palabra
            case '*':
                // Si la palabra aparece en el doc entonces aumentamos un 20% su socre
                if(Data.wDocs[doc, Data.IdxWords[word] ] > 0.00f) {
                    Data.wDocs[doc, Data.IdxWords[word]] += Data.wDocs[doc, Data.IdxWords[word]] * 1f/5f; // Actualizar el peso de la palabra especificamente
                    MemoryChange[ new Tuple<int, int>(doc, Data.IdxWords[word]) ] = sim[doc].Item1;
                    sim[doc] = new Tuple<float, int>( sim[doc].Item1 + sim[doc].Item1 * 1f/5f, sim[doc].Item2 ); // Actualizar el peso del documento
                }
                break;   
            default: break;
        }
    }


    #endregion
}