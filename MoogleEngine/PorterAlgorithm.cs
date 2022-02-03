// !Mas adelante llevar esto al trabajo con hashing

namespace MoogleEngine;

public static class Lemmatization {

    public static string Stemmer(string word) {
        
        int r1, r2, rv;
        (r1, r2, rv) = Get_R1_R2_RV(word);
            
        string word1 = Step0(word, r1, r2, rv);
        // if(word == word1) word1 = Step1(word, r1, r2, rv);



        return word1; 
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
        for(int i = n - r1; i < n; i ++) 
                if( !IsVocal(word[i])  && IsVocal(word[i - 1])) {
                r2 = n - (i + 1); // tamano 
                break;
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



    private static string Step0(string word, int r1, int r2, int rv) {
        int n = word.Length;
        string newWord = "";
        int index = -1;

        for(int i = 4; i >= 2; i --) {
            if(i > n) continue;
            if( !step0.Contains( word.Substring(n - i, i) ) ) continue;    
            // if(n - i < rv) continue;

            index = n - i;
            break;
        }

        System.Console.WriteLine(index);

        if(index == -1) return word;

        // Si contiene uno de esos sufijos entonces lo eliminamos si esta precedido por otro
        n = index;
        for(int i = 5; i >= 2; i --) {
            if(i > n) continue;
            if( !stepAfter0.Contains( word.Substring(n - i, i) ) ) continue;
            // if(n - i < rv) continue;

            if(word.Substring(n - i, i) == "yendo" && ( n - i - 1 >= 0 && word[n - i - 1] != 'u')) continue;
            newWord = word.Substring(0, n);
            break;
        }
            
        return newWord;
    }


    // private static string Step1(string word, int r1, int r2, int rv) {
    //     int n = word.Length;
    //     string suff = "";

    //     for(int i = word.Length; i >= 1; i --) {

    //         string suffix = word.Substring(n - i, i);
    //         if(step1_1.Contains( suffix ) || step1_2.Contains( suffix ) || step1_3.Contains( suffix ) ||
    //            step1_4.Contains( suffix ) || step1_5.Contains( suffix ) || step1_6.Contains( suffix ) ||
    //            step1_7.Contains( suffix ) || step1_8.Contains( suffix ) || step1_9.Contains( suffix ) ) {
    //             if(suff.Length < suffix.Length)
    //                 suff = suffix;
    //         }
    //     }   
    //     // Si contiene un suffijo de estos lo borro
    //     if(suff == "") return word;
    //     int k = suff.Length;

    //     if(step1_1.Contains(suff)) {
    //         if(k <= r2)
    //             return word.Substring(0, n - k);
    //     } else
    //     if(step1_2.Contains(suff)) {
    //         if(k <= r2){
    //             if(n - k - 2 >= 0 && word.Substring(n - k - 2, 2) == "ic") 
    //                 return (word.Substring(0, n - k));
    //             else
    //                 return word.Substring(0, n - k);
    //         }
    //     } else
    //     if(step1_3.Contains(suff)) {
    //         if(k <= r2) 
    //             return (word.Substring(0, n - k) + "log");
    //     } else
    //     if(step1_4.Contains(suff)) {
    //         if(k <= r2)
    //              return (word.Substring(0, n - k) + "u");
    //     } else
    //     if(step1_5.Contains(suff)) {
    //         if(k <= r2) 
    //             return (word.Substring(0, n - k) + "entre");
    //     } else
    //     if(step1_6.Contains(suff)) {
    //         if(k <= r1 ) {
    //             if( n - k - 2 >= 0 && word.Substring(n - k - 2, 2) == "iv" )
    //                 return word.Substring(0, n - k - 2);
    //             return word.Substring(0, n - k);
    //         } else
    //         if(k <= r2){
    //             if(n - k - 2 >= 0 && (word.Substring(n - k - 2, 2) == "at" || word.Substring(n - k - 2, 2) == "os" || word.Substring(n - k - 2, 2) == "ic" || word.Substring(n - k - 2, 2) == "ad")) {
    //                 if(n - k - 2 <= r2) 
    //                     return word.Substring(0, n - k - 2);
    //                 return word.Substring(0, n - k);
    //              }  
    //         }
    //     } else
    //     if(step1_7.Contains(suff)) {
    //         if(k <= r2)  
    //             if(n - k - 4 >= 0 && (word.Substring(n - k - 4, 4)  == "ante" || word.Substring(n - k - 4, 4) == "able" || word.Substring(n - k - 4, 4) == "ible")) {
    //                 if(n - k - 4 <= r2)
    //                     return word.Substring(0, n - k - 4);
    //                 return (word.Substring(0, n - k));    
    //             }
    //     } else
    //     if(step1_8.Contains(suff)) {
    //         if(k <= r2)  
    //             if(n - k - 4 >= 0 && word.Substring(n - k - 4, 4) == "abil")
    //                 if(n - k - 4 >= r2) return word.Substring(0, n - k - 4);
    //                 else return word.Substring(0, n - k);
    //             else 
    //             if(n - k - 2 && (word.Substring(n - k - 2, 2) == "ic" || word.Substring(n - k - 2, 2) == "iv"))
    //                 if(n - k - 2 >= r2) return word.Substring(0, n - k - 2);
    //                 else return word.Substring(0, n - k);
    //     } else
    //     if(step1_8.Contains(suff)) {
    //         if(k > r2) continue;
    //         if(n - k - 2 >= 0 && word.Substring(n - k - 2, 2) == "at")
    //             if(n - k - 2 <= r2) return word.Substring(0, n - k - 2);
    //         return word.Substring(0, n - k);
    //     }
        

    //     return word;
    // }



// ? This is OK!!
    private static string Step2a(string word, int r1, int r2, int rv) {
        int n = word.Length;

        for(int i = 5; i >= 2; i --) {
            if(i > rv) continue; 
            string suffix = word.Substring(n - i, i);
            if( !step2a.Contains(suffix) ) continue;
            if(n - suffix.Length - 1 >= 0 && word[n - suffix.Length - 1] == 'u')
                return word.Substring(0, n - i);
        }
        return word;
    }


    private static string Step2b(string word, int r1, int r2, int rv) {
        int n = word.Length;

        for(int i = 4; i >= 2; i --) {
            if(i > rv) continue; 
            string suffix = word.Substring(n - i, i);
            if( !step2b1.Contains(suffix) ) contiene;

            if(n - i - 2 >= 0 && word.Substring(n - i - 2, 2) == "gu")
                return word.Substring(0, n - i - 1);
        }
        return word;
    }






    private static bool IsVocal(char x) {
        return vowels.Contains( x );
    }
    private static bool IsConsonant(char x) {
        return  x >= 'a' && x <= 'z' && 
              ( x != 'a' && x != 'e' &&  x != 'i' &&  x != 'o' &&  x != 'u');
    }



#region DataForPorterAlgoritm
    
    private static char[] vowels = { "a", "e", "i", "o", "u", "á", "é", "í", "ó", "ú", "ü" };

    public static string[] step0 = { "me", "se", "sela", "selo", "selas", "selos", "la", "le", "lo", "las", "les", "los", "nos" }; 
    public static string[] stepAfter0 = { "iéndo", "ándo", "ár", "ér", "ír", "ando", "iendo", "ar", "er", "ir", "yendo" };
   
    public static string[] step1_1 = { "anza", "anzas", "ico", "ica", "icos", "icas", "ismo", "ismos", "able", "ables", "ible", "ibles",
                                       "ista", "istas", "oso", "osa", "osos", "osas", "amiento", "amientos", "imiento", "imientos" };
    public static string[] step1_2 = { "adora", "ador", "ación", "adoras", "adores", "aciones", "ante", "antes", "ancia", "ancias" };
    public static string[] step1_3 = { "logía", "logías" };
    public static string[] step1_4 = { "ución", "uciones" };
    public static string[] step1_5 = { "encia", "encias" };
    public static string[] step1_6 = { "amente" };
    public static string[] step1_7 = { "mente" };
    public static string[] step1_8 = { "idad", "idades" };
    public static string[] step1_9 = { "iva", "ivo", "ivas", "ivos" };

    public static string[] step2a = { "ya", "ye", "yan", "yen", "yeron", "yendo", "yo", "yó", "yas", "yes", "yais", "yamos" };
    public static string[] step2b1 = {"en", "es", "éis", "emos"};
    public static string[] step2b2 = { "arían", "arías", "arán", "arás", "aríais", "aría", "aréis", "aríamos", "aremos", "ará", "aré",
                                       "erían", "erías", "erán", "erás", "eríais", "ería", "eréis", "eríamos", "eremos", "erá", "eré",
                                       "irían", "irías", "irán", "irás", "iríais", "iría", "iréis", "iríamos", "iremos", "irá", "iré",
                                       "aba", "ada", "ida", "ía", "ara", "iera", "ad", "ed", "id", "ase", "iese", "aste", "iste", "an",
                                       "aban", "ían", "aran", "ieran", "asen", "iesen", "aron", "ieron", "ado", "ido", "ando", "iendo",
                                       "ió", "ar", "er", "ir", "as", "abas", "adas", "idas", "ías", "aras", "ieras", "ases", "ieses", "ís",
                                       "áis", "abais", "íais", "arais", "ierais", "aseis", "ieseis", "asteis", "isteis", "ados", "idos", "amos",
                                       "ábamos", "íamos", "imos", "áramos", "iéramos", "iésemos", "ásemos"};

    public static string[] step3a = {"os", "a", "o", "á", "í", "ó"};
    public static string[] step3b = {"e", "é"};

#endregion



}