using System.Text;
namespace MoogleEngine;

public static class WorkingOperators {

    //* Comprobar si un caracter es un operador
    public static bool IsOperator(char o) {
        return (o == '!' || o == '^' || o == '~' || o == '*');
    }

    //* Devolver lista de pares en forma de <operadores, palabra>
    public static List< Tuple<string, string> > GetOperators(string query) {
        List<Tuple<string, string>> operators = new List<Tuple<string, string>> ();        
        string[] parts = query.Split(' ');
        int n = parts.Length;


        for(int i = 0; i < n; i ++) {
            if( !AuxiliarMethods.IsLineOperators(parts[i]) ) continue;

            string op = parts[i];
            
            // Si esos operadores son los últimos entonces no le corresponden a ninguna palabra
            if(i == n - 1)  continue;
            string next_word = Lemmatization.Stemmer( parts[i + 1] );

            // Si son los operadores menos el de cercania simplemente anadimos la palabra con sus operadores
            if( op[0] != '~' ) {
                operators.Add( new Tuple<string, string> (op, next_word) );
                continue;
            }

            //  Si es el de cercania
            // Si es la primera palabra entonces ignoramos la cercania y nos quedamos con los demas operadores que afecten a la palabra
            if(i == 0) {
                if( op.Length > 1 ) // Si tiene mas operadores los guardamos junto con la palabra 
                    operators.Add( new Tuple<string, string> (op.Substring(1, op.Length - 1), next_word) );
                continue;
            }

            // Sino tomamos las dos palabras de su alrededor que son afectadas por el operador
            string prev_word = Lemmatization.Stemmer( parts[i - 1] );

            // Si ya hemos guardando operadores vemos si lo ultimo que tenemos agregado es un operador de cercania
            Tuple<string, string> last = new Tuple<string, string>("", "");
            if(operators.Count != 0) last = operators.Last();

            // Si es el de cercania entonces tomamos el ultimo de la cadena
            string aux_word = (last.Item1 == "~") ?  Lemmatization.Stemmer( last.Item2.Split(' ').Last() ) : Lemmatization.Stemmer( last.Item2 ); // ultima palabra agregada al conjunto de <operadores, palabra>

            //  Si son palabras diferentes entonces anadimos un nuevo elemento a la lista de operadores <"~", prev_word + " " + operadores + " " + parts[i + 1]>
            string oper_aux = parts[i].Substring(1, parts[i].Length - 1);
            
            if( aux_word != prev_word ) {
               operators.Add(new Tuple<string, string> ("~", prev_word + ' ' + oper_aux + ((oper_aux.Length != 0) ? " " : "")  + Lemmatization.Stemmer( parts[i + 1] )));
            } 
            else {
                operators.RemoveAt(operators.Count - 1);
                // Si pertenece a un operador de cercania la anadimos directamente 
                if( last.Item1 == "~" ) {
                    operators.Add( new Tuple<string, string>( "~", last.Item2 + " " + oper_aux + oper_aux + 
                                                            ((oper_aux.Length != 0) ? " " : "")  + Lemmatization.Stemmer( parts[i + 1] ) ) );
                } 
                else {
                    operators.Add( new Tuple<string, string>( "~", last.Item1 + " " + last.Item2 + " " + oper_aux + 
                                                            ((oper_aux.Length != 0) ? " " : "") + Lemmatization.Stemmer( parts[i + 1] ) ) );
                }

            }

        }

        return operators;
    }
   //* Devuelve vacio si no es valida, y en otro caso simplifica la expresion a una valida
    public static string ValidOperators(string op) {
        if(op == "") return "";

        // Si son operadores simples
        if(op == "!") return op;
        if(op == "^") return op;
        if(op == "*") return op;
        if(op == "~") return op;

        int[] cnt = new int[257];
        int n = op.Length;

        foreach ( char c in op) cnt[ (int)c ] ++;

        // Si el signo de ~ esta entre los operadores pero no al inicio entonces los operadores no tienen logica
        if( op[0] != '~' && cnt[ (int)'~' ] != 0 ) return "";
        
        // Si estan los operadores ^ y ! entonces los operadores no tienen logica
        if( cnt[ (int)'^' ] != 0 && cnt[ (int)'!' ] != 0 ) return "";

        // Si aparece ! entonces no importa los otros operadores, el documento no debe de aparecer
        if(cnt[ (int)'!' ] != 0 ) return "!";
        

        // Ahora poner una aparicion de cada operador, excepto el * que se ponen todos
        cnt[ (int)'~' ] = cnt[ (int)'!' ] = cnt[ (int)'*' ] = cnt[ (int)'^' ] = 0;
        StringBuilder newOper = new StringBuilder();
        int fristPositionAsteriscos = -1;

        for(int i = 0; i < n; i ++) {
            int id = (int)op[i];
            cnt[id] ++;

            if(op[i] == '*') {
                if(fristPositionAsteriscos == - 1) {
                    newOper.Append( '*' );
                    fristPositionAsteriscos = newOper.Length - 1;
                    continue;
                }
                newOper.Insert(fristPositionAsteriscos, '*');
            }
            if(cnt[ id ] > 1) continue; 
            
            newOper.Append( op[i] );
        }
        
        return newOper.ToString();
    }

    //* Procesar todos los operadores de la query para cada documento
    public static void ChangeForOperators( List< Tuple<string, string> > operators,  Tuple<float, int>[] sim) {

        for(int doc = 0; doc < Data.TotalFiles; doc ++){

            foreach(Tuple<string, string> PairOperWords in operators) {
                string opers = PairOperWords.Item1;
                string word = PairOperWords.Item2;

                // Si es una palabra que no esta en ningun documento..
                // El operador tiene que ser diferente al de cercania porque en ese operador se guarda
                // mas de una palabra y ese string nunca va a estar en el diccionario
                if(opers != "~" && !AuxiliarMethods.IsWordInDocs(word)) continue; 
                
                // recorrer los operadores e ir aplicando uno por uno   
                foreach(char op in opers) {
                    
                    switch( op ) {
                        //?  La palabra no puede aparecer en ningun documento que sea devuelto 
                        case '!':
                            ProcessOperator('!', word, doc, sim);
                            break;
                        //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
                        case '^': 
                            ProcessOperator('^', word, doc, sim);
                            break;
                        //?  Aumentar la relevancia del documento que tiene esa palabra
                        case '*':
                            ProcessOperator('*', word, doc, sim);
                            break; 
                        
                        //? Aumentar la relevancia del documento mientras mas cercanas esten las palabras
                        case '~':

                            List<string> wordsForCloseness = new List<string>();
                            List< Tuple<string, string> > OpersAndWords = GetOperators(word); 
                            string[] SubWords = word.Split(' ');

                            //? Poner en las palabras para la cercania las que no tengan operador
                            for(int wi = 0; wi < SubWords.Length; wi ++) {
                                
                                if( AuxiliarMethods.IsLineOperators(SubWords[wi]) ) continue;
                                if( !AuxiliarMethods.IsWordInDocs(SubWords[wi]) ) continue;
                                
                                // Si no tiene operadores entonces la agrego a la lista si aparece en el documento
                                if(wi > 0 && AuxiliarMethods.IsLineOperators(SubWords[wi - 1]) ) continue;
                                
                                if( Data.PosInDocs[doc].ContainsKey(SubWords[wi]) )
                                    wordsForCloseness.Add(SubWords[wi]);
                            }


                            //? Trabajar con las palabras con operadores y ponerlos en la  cercania
                            for(int i = 0; i < OpersAndWords.Count; i ++) {
                                string o = OpersAndWords[i].Item1;
                                string w = OpersAndWords[i].Item2;
                                
                                if(!AuxiliarMethods.IsWordInDocs(w)) continue;

                                // Si la palabra no esta en el documento entonces la omitimos para la cercania
                                if( !Data.PosInDocs[doc].ContainsKey(w) )
                                    continue;

                                if(o == "!") {
                                    // Si la palabra aparece en el documento entonces el operador de cercania de todas las palabras queda invalidado
                                    if( Data.PosInDocs[doc].ContainsKey(w) ) {
                                         ProcessOperator('!', w, doc, sim);
                                        break;
                                    }
                                     continue;                                    
                                 }

                                // Solo nos queda por aplicar los operadores ^ y * en caso de que los tenga
                                foreach(char x in o) 
                                    ProcessOperator(x, w, doc, sim);

                                // anadimos las palabras a una lista para calcular la cercania
                                wordsForCloseness.Add(w);
                            }

                            if(wordsForCloseness.Count <= 1) // Si no hay al menos dos palabras para la cercania
                                continue;
                        

                            string aux = ""; // Anadir la cantidad de palabras originales de la cercania
                            foreach(string x in wordsForCloseness)
                                aux += x + " ";
                            aux += SubWords.Length.ToString();
                            ProcessOperator('~', aux, doc, sim);
                        break;

                        default: break;
                    }
                }
            }

        }
    }
    //* Funcion Auxiliar paraCalcular la distancia entre dos palabras del documento
    public static int DistanceBetweenWords(int doc, int x1, int y1, int x2, int y2) {

        // Ordenar el par
        Tuple<int, int>[] pairs = { new Tuple<int, int>(x1, y1), new Tuple<int, int>(x2, y2) };    
        Array.Sort(pairs);
        x1 = pairs[0].Item1;
        y1 = pairs[0].Item2;
        x2 = pairs[1].Item1;
        y2 = pairs[1].Item2;
        // End Order Par
        
        if(x1 == x2) 
            return Math.Abs(x1 - x2);

        int distance = 0;
        for(int line = x1 + 1; line <= x2 - 1; line ++) {
            distance += Data.CntWordsForLines[doc][line];
        }

        distance += (Data.CntWordsForLines[doc][x1] - y1);
        distance += x2;

        return distance;
    }
    //* Funcion Auxiliar para actualizar el score de los documentos para un operador
    public static void ProcessOperator(char op, string word, int doc, Tuple<float, int>[] sim) {
        switch( op ) {
            //?  La palabra no puede aparecer en ningun documento que sea devuelto
            case '!':
                // Si la palabra esta en el documento entonces igualamos score a cero para que ese documento no salga
                if(Data.PosInDocs[doc].ContainsKey(word))
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            //?  La palabra tiene que aparecer en cualquier documento que sea devuleto
            case '^': 
                // Si la palabra no esta en el doc entonces igualamos el score a cero para que ese documento no salga
                if(!Data.PosInDocs[doc].ContainsKey(word))
                    sim[doc] = new Tuple<float, int> (0.00f, doc);
                break;

            // //?  Aumentar la relevancia del documento que tiene esa palabra
            case '*':
                // No hacemos nada, ya que aumentamos la relevancia(cantidad de apariciones en la query) en pasos previos
                break;   

            //? Calcular la cercania 
            case '~':
                string[] aux = AuxiliarMethods.GetWordsOfSentence(word);
                int CntWordsOriginalsForCloseness = int.Parse(aux[ aux.Length - 1 ]);

                string[] wordsForCloseness = new String[ aux.Length - 1 ];
                for(int i = 0; i < aux.Length - 1; i ++)
                    wordsForCloseness[i] = aux[i];

                (float minDistance, _) = ProcessCloseness(wordsForCloseness, doc);
                float score = ((float)wordsForCloseness.Length / (float)CntWordsOriginalsForCloseness) / sim[doc].Item1; // Mientras mas palabras tengan mas score tendera
                sim[doc] = new Tuple<float, int> ( score + 100.00f / (float)minDistance, doc); // Minetras mas cercano mas score tendra

                break;

            default: break;
        }
    }
    //* Funcion Auxiliar para Calcula a cercania del conjunto de palabras por cada documento
    public static (float, Tuple<int, int>[]) ProcessCloseness( string[] wordsForCloseness, int doc ) {

        // Guardar todas las posiciones de las palabras en un array para ordenarlos 
        List< Tuple<int, int, string> > posiciones = new List< Tuple<int, int, string> > ();
        // Diccionario para registrar las apariciones durante la busqueda
        Dictionary< string, int > cnt = new Dictionary< string, int >();
        // Guardar los intervalos en donde estan todas las palabras
        List< Tuple<int, int> > Interv = new List< Tuple<int, int> > ();



        foreach(string words in wordsForCloseness) {
            int n = Data.PosInDocs[doc][ words ].AmountAppareance;
            for(int i = 0; i < n; i ++) {
                (int x, int y) = Data.PosInDocs[doc][ words ].nthAppareance(i);
                posiciones.Add( new Tuple<int, int, string>( x, y, words ) );
            }
        }
        
        // Ordenar las posiciones por posiciones de menor a mayor
        posiciones.Sort();

        int cantWords = wordsForCloseness.Length;

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

        int minDistance = int.MaxValue;
        // Lista de posiciones mas cercanas para el snippet
        List<Tuple<int, int>> positionsForSnippet = new List<Tuple<int, int>>();

        // Recorrer los intervalos en busca del mas cercano
        foreach( Tuple<int, int> i_interv in Interv ) {

            // Limpiar cnt para reutilizarlo con cada intervalo
           cnt.Clear();

            int li = i_interv.Item1;
            int ri = i_interv.Item2;

            int distance = 0;
            int prevx =  posiciones[li].Item1, prevy = posiciones[li].Item2;

            List<Tuple<int, int>> aux = new List<Tuple<int, int>>();

            // Ir por todas las apariciones de las palabras en el intervalo [li ... ri] y tomar una aparicion de cada palabra
            for(int i = li; i <= ri; i ++) {
                if( cnt.ContainsKey(posiciones[i].Item3) )
                    continue;
                cnt[ posiciones[i].Item3 ] = 1;
                
                aux.Add( new Tuple<int, int> ( posiciones[i].Item1, posiciones[i].Item2 ) );

                distance += DistanceBetweenWords(doc, prevx, prevy, posiciones[i].Item1, posiciones[i].Item2);
                prevx = posiciones[i].Item1;
                prevy = posiciones[i].Item2;
            }

            if(distance < minDistance) {
                minDistance = distance;
                positionsForSnippet.Clear();

                // Quedarme con las posiciones de las palabras que conforman la distancia mas corta
                for(int i = 0; i < aux.Count; i ++)
                    positionsForSnippet.Add(new Tuple<int, int>(aux[i].Item1, aux[i].Item2));
            }

            
        }

        return (minDistance, positionsForSnippet.ToArray());
    }

}