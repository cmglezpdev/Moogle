namespace MoogleEngine;

public static class WorkingOperators {

    //* Comprobar si un caracter es un operador
    public static bool IsOperator(char o) {
        return (o == '!' || o == '^' || o == '~' || o == '*');
    }
    
    //* Devuelve los operadores que hay en un string en la posicion pos
    public static string GetOperators(string sentence, int pos) {
        string operators = "";
        int n = sentence.Length;

        for(int i = pos; i < n && (IsOperator(sentence[i]) || Char.IsWhiteSpace(sentence[i])); i ++)
            if(IsOperator(sentence[i]))
                    operators += sentence[i];

        return operators;
    }

    //* Devolver lista de pares en forma de <operadores, palabra>
    public static List< Tuple<string, string> > GetOperators(string query) {
        List<Tuple<string, string>> o = new List<Tuple<string, string>>();
        int n = query.Length;

        // Guardar las palabras que tienen el operador de cercania para despues juntarlos todos en una sola tupla
        List<string> aux = new List<string>(); 

        for(int i = 0; i < query.Length; i ++) {
            if(!IsOperator(query[i])) continue;

            // Sacar los operadores delante de la palabra en la posicion i
            string op = GetOperators(query, i);
            int j = i;
            while(++j < n && AuxiliarMethods.Ignore(query[j]));
            if(j >= n) break;
            // Sacar la palabra que esta a la derecha de la posicion j
            string wo = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWord(query, j, "right"));
            int auxLen = wo.Length;
                    wo = Lemmatization.Stemmer(wo);

            // Validar los operadores segun mi criterio
            string operators = AuxiliarMethods.ValidOperators(op);
            if(operators == "") continue; // Si los operadores no son validos
            
            // Si no es un operador de cercania puedo guardar la palabra con sus operadores
            if(operators[0] != '~') {
                 o.Add(new Tuple<string, string>(operators, wo));
                i = j + auxLen - 1;
                continue;
            }
            
            // Como es un operador de cercania entonces encontramos la palabra anterior a ella
            int k = i;
            while( --k >= 0 && AuxiliarMethods.Ignore(query[k]) );
            if(k < 0) continue;
            string prev_wo = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWord(query, k, "left"));
                   prev_wo = Lemmatization.Stemmer(prev_wo); 
            if(prev_wo == "") { // Si la palabra no existe el operador queda invalidado por lo que no la agregamos
                 i = j + auxLen - 1;
                 continue;
            }
            
            // Si es la misma que la ultima que pusimos entonces significa que tiene operador, por lo tanto 
            // la quitamos de la lista y la anadimos junto a la palara nueva
            if(o.Count > 0) { // Si hay palabras guardadas
                // Si la el operador de la palabra anterior es el de cercania, tenemos que cojer la segunda palabra de las que estan juntas con ese operador
                string x = ( o.Last().Item1 != "~" ) ? o.Last().Item2 : AuxiliarMethods.GetWordsOfSentence(o.Last().Item2).Last();
                
                if(prev_wo == x) { // Si la palabras coinciden las tomamos y las unimos
                    
                    // Si la palabra es simple
                    if(o.Last().Item1 != "~") prev_wo = o.Last().Item1 + o.Last().Item2;
                    else prev_wo = o.Last().Item2;
                   
                    o.RemoveAt(o.Count - 1);
                }
            }

            o.Add(new Tuple<string, string>( "~", prev_wo + " " + operators.Substring(1, operators.Length - 1) + wo));
            i = j + auxLen - 1;
        }

        return o;
    }

    //* Procesar todos los operadores de la query para cada documento
    public static void ChangeForOperators( List< Tuple<string, string> > operators, Dictionary< Tuple<int, int>, float > MemoryChange,  Tuple<float, int>[] sim) {

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
                            ProcessOperator('!', word, doc, MemoryChange, sim);
                            break;
                        //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
                        case '^': 
                            ProcessOperator('^', word, doc, MemoryChange, sim);
                            break;
                        //?  Aumentar la relevancia del documento que tiene esa palabra
                        case '*':
                            ProcessOperator('*', word, doc, MemoryChange, sim);
                            break; 
                        
                        //? Aumentar la relevancia del documento mientras mas cercanas esten las palabras
                        case '~':

                            List<string> wordsForCloseness = new List<string>();
                            List< Tuple<string, string> > OpersAndWords = GetOperators(word); 
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
                                         ProcessOperator('!', w, doc, MemoryChange, sim);
                                        break;
                                    }
                                     continue;                                    
                                 }

                                // Solo nos queda por aplicar los operadores ^ y * en caso de que los tenga
                                foreach(char x in o) 
                                    ProcessOperator(x, w, doc, MemoryChange, sim);

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

    //* Funcion Auxiliar paraCalcular la distancia entre dos palabras del documento
    public static int DistanceBetweenWords(int x1, int y1, int x2, int y2) {
        int distance =  Math.Abs(x1 - x2);
        // Si estan en la misma linea resto sus posiciones
        if(x1 == x2) return distance + Math.Abs(y1 - y2);
        
        // Con x1 <= x2 && y1 <= y2
        return x2 + Math.Max(0, 15 - x1); // 15 es un valor promedio de la cantidad de palabras por linea
    }

    //* Funcion Auxiliar para actualizar el score de los documentos para un operador
    public static void ProcessOperator(char op, string word, int doc, Dictionary< Tuple<int, int>, float > MemoryChange, Tuple<float, int>[] sim) {
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




}