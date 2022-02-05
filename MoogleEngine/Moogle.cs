namespace MoogleEngine;
public static class Moogle
{

    #region Variables
    public static List<List<info>> PosInDocs = new List<List<info>>();         // Matrix con las repeticiones de las palabras en cada documento
    public static Dictionary<string, int> IdxWords = new Dictionary<string, int>();     // Palabras con su indice en la lista
    public static string[] files = new string[0];
    public static int TotalFiles = 0;
    public static int TotalWords = 0;
    public static float[,] wDocs = new float[0,0];

    #endregion




    public static SearchResult Query(string query)
    {

        //! Calcular el suggestion por las palabras que no aparecen en el documento
        string suggestion = GetSuggestion(query);

        //! Frecuencia de las palabras de la query
        Dictionary<string, int> FreqWordsQuery = GetFreqWordsInQuery( suggestion );

        //! Matriz peso del query
        float[] wQuery = GetWeigthOfQuery( ref FreqWordsQuery );

        //! Calcular el rank entre las paguinas midiendo la similitud de la query con el documento
        Tuple<float, int>[] sim = GetSimBetweenQueryDocs(ref wQuery, ref wDocs);

        // !Modificar el peso de los documentos en base a cada operador del query
        List< Tuple<string, string> > operators = FilesMethods.GetOperators(suggestion);
        // Guardar los cambios que se le hacen a los pesos de los documentos para despues volverlos al valor inicial
        Dictionary< Tuple<int, int>, float > MemoryChange = new Dictionary<Tuple<int, int>, float>();
        ChangeForOperators(ref operators, ref MemoryChange, ref sim);


        //! Ordenar los scores por scores
        Array.Sort(sim);
        Array.Reverse(sim);


        // !Construir el resultado
        SearchItem[] items = BuildResult(ref sim, ref FreqWordsQuery, ref wDocs);

        //! Devolver a los valores originales a los scores que ya fueron modificados 
        NormalizeData(ref MemoryChange);

        //! Si no ubieron palabras mal escritas entonces no hay que mostrar sugerencia
        if(suggestion == query) suggestion = ""; 

        return new SearchResult(items, suggestion);
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
    private static Dictionary<string, int> GetFreqWordsInQuery( string query ) {
        //! Buscar la frecuencia de las palabras de la query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);

        // Calculamos la frecuencia de las palabras en la query
        Dictionary<string, int> FreqWordsQuery = new Dictionary<string, int>();
        foreach(string w in WordsQuery) {
            string lower = AuxiliarMethods.NormalizeWord(w);
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
        float[] wQuery = new float[TotalWords];
        // Ir por todas las palabras de la query
        foreach(KeyValuePair<string, int> wq in FreqWordsQuery) {
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
    private static string GetSuggestion(string query) {
        string suggestion = "";

        for(int i = 0; i < query.Length; i ++) {
            // Si es un caracter que forme una palabra entoces la anadimos a la sugerencia
            if(AuxiliarMethods.Ignore(query[i])) {
                suggestion += query[i];
                continue;
            }

            string w = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWordStartIn(query, i));
            if(IdxWords.ContainsKey(w)) {
                suggestion += w;
                i += w.Length - 1;
                continue;
            }
        
            string newW = "";
            int steps = 100000;
            foreach(KeyValuePair<string, int> wd in IdxWords) {
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
    private static SearchItem[] BuildResult(ref Tuple<float, int>[] sim, ref Dictionary<string, int> FreqWordsQuery, ref float[,] wDocs) {
        List<SearchItem> items = new List<SearchItem>();

        for(int i = 0; i < TotalFiles; i ++) {
           // Si ninguna de las palabras estan en el documento     
        //    System.Console.WriteLine(sim[i].Item1);
           if(sim[i].Item1 == 0.00f) continue;

            float score = 0.00f;
            string word = "";
            int doc = sim[i].Item2;

            // Sacar la palabra con mayor score de la query
            foreach(KeyValuePair<string, int> wq in FreqWordsQuery) {
                // Si la palabra no esta entre los documentos
                if(!IdxWords.ContainsKey(wq.Key)) continue;
                // Si la palabra no aparece en ese documento
                if(PosInDocs[doc][ IdxWords[wq.Key] ].AmountAppareance == 0) continue;

                // Sacar la palabra de mayor score
                if(score < wDocs[doc, IdxWords[ wq.Key ]] ) {
                    word = wq.Key;
                    score = wDocs[doc, IdxWords[word]];
                }
            }


            info PosOfWord = PosInDocs[doc][ IdxWords[word] ];

            Random r = new Random();
            int nl = 0, nw = 0;
            (nl, nw) = PosOfWord.nthAppareance( r.Next() % PosOfWord.AmountAppareance );

            string title = FilesMethods.GetNameFile(files[doc]);
            string snippet = FilesMethods.GetContext(doc, nl, nw, 10);

            items.Add(new SearchItem(title, snippet, score));
        }

        return items.ToArray();
    }
    private static void NormalizeData(ref Dictionary< Tuple<int, int>, float > MemoryChange) {
        foreach(KeyValuePair< Tuple<int, int>, float > mc in MemoryChange) 
            wDocs[ mc.Key.Item1, mc.Key.Item2 ] = mc.Value;
    }
    private static void ChangeForOperators(ref List< Tuple<string, string> > operators, ref Dictionary< Tuple<int, int>, float > MemoryChange, ref Tuple<float, int>[] sim) {


        for(int doc = 0; doc < TotalFiles; doc ++){

            foreach(Tuple<string, string> PairOperWords in operators) {
                string opers = PairOperWords.Item1;
                string word = PairOperWords.Item2;

                // Si es una palabra que no esta en ningun documento..
                // El operador tiene que ser diferente al de cercania porque en ese operador se guarda
                // mas de una palabra y ese string nunca va a estar en el diccionario
                if(opers != "~" && !IdxWords.ContainsKey(word)) continue; 
                
                // recorrer los operadores e ir aplicando uno por uno   
                foreach(char op in opers) {
                    
                    switch( op ) {
                        //?  La palabra no puede aparecer en ningun documento que sea devuelto 
                        case '!':
                            ProcessOperators('!', word, doc, ref MemoryChange, ref sim);
                            break;
                        //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
                        case '^': 
                            ProcessOperators('^', word, doc, ref MemoryChange, ref sim);
                            break;
                        //?  Aumentar la relevancia del documento que tiene esa palabra
                        case '*':
                            ProcessOperators('*', word, doc, ref MemoryChange, ref sim);
                            break; 
                        
                        //? Aumentar la relevancia del documento mientras mas cercanas esten las palabras
                        case '~':

                            List<string> wordsForCloseness = new List<string>();
                            List< Tuple<string, string> > OpersAndWords = FilesMethods.GetOperators(word); 
                            string[] SubWords = AuxiliarMethods.GetWordsOfSentence(word);
                            int idx = 0; // Indice para recorrer las palabras con operadores


                            //? Poner en las palabras para la cercania las que no tengan operador
                            for(int wi = 0; wi < SubWords.Length; wi ++) {
                                
                                if(!IdxWords.ContainsKey(SubWords[wi])) continue;
                                
                                // Si no es una palabra de las que tienen operadores entonces la agrego a la lista si aparece en el documento
                                bool found = false;
                                foreach(Tuple<string, string> g in OpersAndWords) 
                                    if(g.Item2 == SubWords[wi]) {
                                        found = true;
                                        break;
                                    }
                                
                                if( found ) continue;
                                
                                if( PosInDocs[doc][ IdxWords[ SubWords[wi] ] ].AmountAppareance > 0 )
                                    wordsForCloseness.Add(SubWords[wi]);
                            }


                            //? Trabajar con las palabras con operadores y ponerlos en la  cercania
                            for(int i = 0; i < OpersAndWords.Count; i ++) {
                                string o = OpersAndWords[i].Item1;
                                string w = OpersAndWords[i].Item2;
                                
                                if(!IdxWords.ContainsKey(w)) continue;

                                // Si la palabra no esta en el documento entonces la omitimos para la cercania
                                if( PosInDocs[doc][ IdxWords[w] ].AmountAppareance == 0 )
                                    continue;

                                if(o == "!") {
                                    // Si la palabra aparece en el documento entonces el operador de cercania de todas las palabras queda invalidado
                                    if( PosInDocs[doc][ IdxWords[w] ].AmountAppareance > 0 ) {
                                         ProcessOperators('!', w, doc, ref MemoryChange, ref sim);
                                        break;
                                    }
                                     continue;                                    
                                 }

                                // Solo nos queda por aplicar los operadores ^ y * en caso de que los tenga
                                foreach(char x in o) 
                                    ProcessOperators(x, w, doc, ref MemoryChange, ref sim);

                                // anadimos las palabras a una lista para calcular la cercania
                                wordsForCloseness.Add(w);
                            }
                             System.Console.WriteLine(wordsForCloseness.Count);

                            if(wordsForCloseness.Count <= 1) // Si no hay al menos dos palabras para la cercania
                                continue;
                            // **** Calcular la cercania con un backtraking
                            double minDistance = CalcMinCloseness(doc, 0, wordsForCloseness, -1, -1, 0);

                            // Modificar el score en base a la cercania
                            minDistance = (double)( minDistance / (double)SubWords.Length );
                            sim[doc] = new Tuple<float, int>(sim[doc].Item1 + (float)minDistance ,doc);

                            

                        break;

                        default: break;
                    }
                }
            }
        }


    }
    private static double DistanceBetweenWords(int x1, int y1, int x2, int y2) {
        return Math.Sqrt( (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) );
    }
    private static double CalcMinCloseness( int doc, int idx, List<string> words, int prevx, int prevy, double distance ) {
        // Si se procesaron todas las palabras
        if(idx == words.Count) {
            return distance;
        }

        info Appareances = PosInDocs[doc][ IdxWords[ words[idx] ] ];
        int n = Appareances.AmountAppareance;

        System.Console.WriteLine("TO {0}", n);


        double minDistance = double.MaxValue;

        for(int app = 0; app < n; app ++) {
            int currx, curry;
            (currx, curry) = Appareances.nthAppareance(app);
            double currdist = (prevx == -1) ? 0 : DistanceBetweenWords( prevx, prevy, currx, curry );

            minDistance =  Math.Min( minDistance, CalcMinCloseness(doc, idx + 1, words, currx, curry, distance + currdist) );
        }    

        return minDistance;        
    }
    static private void ProcessOperators(char op, string word, int doc, ref Dictionary< Tuple<int, int>, float > MemoryChange, ref Tuple<float, int>[] sim) {
        switch( op ) {
            //?  La palabra no puede aparecer en ningun documento que sea devuelto
            case '!':
                // Si la palabra esta en el documento entonces igualamos score a cero para que ese documento no salga
                if( wDocs[doc, IdxWords[word]] > 0.00f )  
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
            case '^': 
                // Si la palabra no esta en el doc entonces igualamos el score a cero para que ese documento no salga
                if(wDocs[doc, IdxWords[word]] == 0.00f)
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            // //?  Aumentar la relevancia del documento que tiene esa palabra
            case '*':
                // Si la palabra aparece en el doc entonces aumentamos un 20% su socre
                if(wDocs[doc, IdxWords[word] ] > 0.00f) {
                    wDocs[doc, IdxWords[word]] += wDocs[doc, IdxWords[word]] * 1f/5f; // Actualizar el peso de la palabra especificamente
                    MemoryChange[ new Tuple<int, int>(doc, IdxWords[word]) ] = sim[doc].Item1;
                    sim[doc] = new Tuple<float, int>( sim[doc].Item1 + sim[doc].Item1 * 1f/5f, sim[doc].Item2 ); // Actualizar el peso del documento
                }
                break;   
            default: break;
        }
    }


    #endregion
}