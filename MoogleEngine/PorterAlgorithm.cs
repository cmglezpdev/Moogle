// !Mas adelante llevar esto al trabajo con hashing

namespace MoogleEngine;

public static class Lemmatization {

    public static string Stemmer(string w) {
        string word = w.ToLower();

        int r1, r2, rv;
        (r1, r2, rv) = Get_R1_R2_RV(word);
        int n = word.Length;

        string word1 = Step0(word, n - r1, n - r2, n - rv);
        if(word == word1) word1 = Step1(word, n - r1, n - r2, n - rv);
        if(word == word1) word1 = Step2a(word, n - r1, n - r2, n - rv);
        if(word == word1) word1 = Step2b(word, n - r1, n - r2, n - rv);



        word1 = Step3(word1, n - r1, n - r2, n - rv);

        return AuxiliarMethods.NormalizeWord( word1 ); 
    }


    private static (int, int, int) Get_R1_R2_RV(string word) {
        int r1, r2, rv;
        int n = word.Length;
        r1 = r2 = rv = n;

            // Calcular R1: El substring que esta despues de la primera no-vocal precedida por una vocal
        for(int i  = 1; i < n; i ++) 
            if( !IsVocal(word[i])  && IsVocal(word[i - 1])) {
                r1 = n - (i + 1); // tamano 
                break;
            }
        // Calcular R2: Lo mismo que R1 pero aplicado al propio R1
        for(int i = n - r1; i < n; i ++) {
            if( !IsVocal(word[i])  && i - 1 >= 0 && IsVocal(word[i - 1])) {
                r2 = n - (i + 1); // tamano 
                break;
            }     
        }
        
        // Calcular RV: 
        if(word.Length >=3) {

            if( IsConsonant(word[1]) ) {
                int i = 2;
                while(i < n && !IsVocal(word[i])) i ++;
                i ++;
                if(i < n) rv = n - i;
            }
            else 
            if( IsVocal(word[0]) && IsVocal(word[1]) ) {
                int i = 2;
                while(i < n && !IsConsonant(word[i])) i ++;
                i ++;
                if(i < n) rv = n - i;
            }
            else
            if( IsConsonant(word[0]) && IsVocal(word[1]) ) {
                rv = n - 3;
            }
        }


        return (r1, r2, rv);
    }


    // ? Step 0: Attached pronoun
    private static string Step0(string word, int r1, int r2, int rv) {
        int n = word.Length;
        string newWord = "";
        int index = -1;

        for(int i = 4; i >= 2; i --) {
            if(i > n || n - i < rv) continue; // Si es mas grande que la palabra o es mas grande que rv
            if( !step0.Contains( word.Substring(n - i, i) ) ) continue;    

            index = n - i;
            break;
        }


        if(index == -1) return word;

        // Si contiene uno de esos sufijos entonces lo eliminamos si esta precedido por otro
        n = index;
        for(int i = 5; i >= 2; i --) {
            if(i > n ||  n - i < rv) continue;
            string suffix = word.Substring(n - i, i);
            if( !stepAfter0.Contains( suffix ) ) continue;

            if(suffix != "yendo")
                 newWord = word.Substring(0, n - i);
            else // Quiere decir que el prefijo es yendo 
             if(n - i - 1 >= 0 && word[n - i - 1] == 'u')
                newWord = word.Substring(0, n - i);
            break;
        }
            
        return newWord; 
    }

    // ? Step 1: Standard suffix removal
    private static string Step1(string word, int r1, int r2, int rv) {
        int n = word.Length;
        string suff = "";

        for(int i = 8; i >= 2; i --) {

            if(n - i < 0) continue; // El sufijo es mas grande que la palabra
            string suffix = word.Substring(n - i, i);
            if(step1_1.Contains( suffix ) || step1_2.Contains( suffix ) || step1_3.Contains( suffix ) ||
               step1_4.Contains( suffix ) || step1_5.Contains( suffix ) || step1_6.Contains( suffix ) ||
               step1_7.Contains( suffix ) || step1_8.Contains( suffix ) || step1_9.Contains( suffix ) ) {
                   suff = suffix;
                   break; 
            }
        }   
        // Si contiene un suffijo de estos lo borro
        if(suff == "") return word;

        int k = suff.Length;
        if(step1_1.Contains(suff)) {
            if(n - k >= r2)  return word.Substring(0, n - k);
        } else

        if(step1_2.Contains(suff)) {
            if(n - k >= r2) {
                if( n - k - 2 >= r2 && word.Substring(n - k - 2, 2) == "ic" )
                    return word.Substring(0, n - k - 2);
                return word.Substring(0, n - k);
            }  
        } else

        if(step1_3.Contains(suff)) {
            if(n - k >= r2) 
                return (word.Substring(0, n - k) + "log");
        } else

        if(step1_4.Contains(suff)) {
            if(n - k >= r2)
                 return (word.Substring(0, n - k) + "u");
        } else

        if(step1_5.Contains(suff)) {
            if(n - k >= r2) 
                return (word.Substring(0, n - k) + "ente");
        } else

        if(step1_6.Contains(suff)) {
            if(n - k >= r1 && (n - k - 2 >= 0 && word.Substring(n - k - 2, 2) == "iv"))
                return word.Substring(0, n - k);

            if(n - k >= r2) {
                if(n - k - 2 >= r2 && word.Substring(n - k - 2, 2) == "at")
                    return word.Substring(0, n - k - 2);
                return word.Substring(0, n - k);
            }

            if(n - k - 2 >= r2) {
                string s = word.Substring(n - k - 2, 2);
                 if(s == "os" || s == "ad" || s == "ic")
                    return word.Substring(0, n - k - 2);
            }

        } else

        if(step1_7.Contains(suff)) {
            if(n - k >= r2) {
                if(n - k - 4 >= r2){
                    string s = word.Substring(n - k - 4, 4);
                    if(s == "ante" || s == "able" || s == "ible")
                        return word.Substring(0, n - k - 4);
                }
                return word.Substring(0, n - k);
            }  
              
        } else

        if(step1_8.Contains(suff)) {
            if(n - k >= r2) {

                if(n - k - 4 >= r2 && word.Substring(n - k - 4, 4) == "abil")
                    return word.Substring(0, n - k - 4);
                if(n - k - 2 >= r2 && (word.Substring(n - k - 2, 2) == "ic" || word.Substring(n - k - 2, 2) == "iv"))
                    return word.Substring(0, n - k - 2); 

                return word.Substring(0, n - k);
            } 
               
        } else
        if(step1_8.Contains(suff)) {
            if(n - k >= r2) {
                if(n - k - 2 >= r2 && word.Substring(n - k - 2, 2) == "at")
                    return word.Substring(0, n - k - 2);

                return word.Substring(0, n - k);
            }
        }
        

        return word;
    }



    // ? Step 2a: Verb suffixes beginning y
    private static string Step2a(string word, int r1, int r2, int rv) {
        int n = word.Length;

        for(int i = 5; i >= 2; i --) {
            if(n - i < rv) continue; 
            string suffix = word.Substring(n - i, i);
            if( !step2a.Contains(suffix) ) continue;
            if(n - suffix.Length - 1 >= 0 && word[n - suffix.Length - 1] == 'u')
                return word.Substring(0, n - i);
        }
        return word;
    }
    // ? Step 2b: Other verb suffixes
    private static string Step2b(string word, int r1, int r2, int rv) {
        int n = word.Length;

        for(int i = 4; i >= 2; i --) {
            if(n - i < rv) continue; 
            string suffix = word.Substring(n - i, i);
            if( !step2b1.Contains(suffix) ) continue;

            if(n - i - 2 >= 0 && word.Substring(n - i - 2, 2) == "gu")
                return word.Substring(0, n - i - 1);
        }

        for(int i = 7; i >= 2; i --) {
            if(n - i < rv) continue;
            string suffix = word.Substring(n - i, i);
            if( !step2b2.Contains(suffix) ) continue;
            
            return  word.Substring(0, n - i);
        }



        return word;
    }
    // ? Step 3: Residual Suffix
    private static string Step3(string word, int r1, int r2, int rv) {

        int n = word.Length;
        string newWord = "";

        if(n - 2 >= rv && step3a.Contains( word.Substring(n - 2, 2) ))
            return word.Substring(0, n - 2);
        if(n - 1 >=  rv && step3a.Contains( word.Substring(n - 1, 1) ))
            return word.Substring(0, n - 1);
        
        if(n - 1 >=  rv && step3b.Contains( word.Substring(n - 1, 1) )) {
             newWord = word.Substring(0, n - 1);   n = newWord.Length;   
            if(n - 1 >= rv && ( n - 2 >= 0 && word.Substring(n - 2, 2) == "gu" ))
                return newWord.Substring(0, n - 1);    
            return newWord;
        }
        
        return word;
    }







    private static bool IsVocal(char x) {
        return vowels.Contains( x );
    }
    private static bool IsConsonant(char x) {
        return  !IsVocal( x );
    }



#region DataForPorterAlgoritm
    
    private static char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'á', 'é', 'í', 'ó', 'ú', 'ü' };

    private static string[] step0 = { "me", "se", "sela", "selo", "selas", "selos", "la", "le", "lo", "las", "les", "los", "nos" }; 
    private static string[] stepAfter0 = { "iéndo", "ándo", "ár", "ér", "ír", "ando", "iendo", "ar", "er", "ir", "yendo" };
   
    private static string[] step1_1 = { "anza", "anzas", "ico", "ica", "icos", "icas", "ismo", "ismos", "able", "ables", "ible", "ibles",
                                       "ista", "istas", "oso", "osa", "osos", "osas", "amiento", "amientos", "imiento", "imientos" };
    private static string[] step1_2 = { "adora", "ador", "ación", "adoras", "adores", "aciones", "ante", "antes", "ancia", "ancias" };
    private static string[] step1_3 = { "logía", "logías" };
    private static string[] step1_4 = { "ución", "uciones" };
    private static string[] step1_5 = { "encia", "encias" };
    private static string[] step1_6 = { "amente" };
    private static string[] step1_7 = { "mente" };
    private static string[] step1_8 = { "idad", "idades" };
    private static string[] step1_9 = { "iva", "ivo", "ivas", "ivos" };

    private static string[] step2a = { "ya", "ye", "yan", "yen", "yeron", "yendo", "yo", "yó", "yas", "yes", "yais", "yamos" };
    private static string[] step2b1 = {"en", "es", "éis", "emos"};
    private static string[] step2b2 = { "arían", "arías", "arán", "arás", "aríais", "aría", "aréis", "aríamos", "aremos", "ará", "aré",
                                       "erían", "erías", "erán", "erás", "eríais", "ería", "eréis", "eríamos", "eremos", "erá", "eré",
                                       "irían", "irías", "irán", "irás", "iríais", "iría", "iréis", "iríamos", "iremos", "irá", "iré",
                                       "aba", "ada", "ida", "ía", "ara", "iera", "ad", "ed", "id", "ase", "iese", "aste", "iste", "an",
                                       "aban", "ían", "aran", "ieran", "asen", "iesen", "aron", "ieron", "ado", "ido", "ando", "iendo",
                                       "ió", "ar", "er", "ir", "as", "abas", "adas", "idas", "ías", "aras", "ieras", "ases", "ieses", "ís",
                                       "áis", "abais", "íais", "arais", "ierais", "aseis", "ieseis", "asteis", "isteis", "ados", "idos", "amos",
                                       "ábamos", "íamos", "imos", "áramos", "iéramos", "iésemos", "ásemos"};

    private static string[] step3a = {"os", "a", "o", "á", "í", "ó"};
    private static string[] step3b = {"e", "é"};

#endregion



}