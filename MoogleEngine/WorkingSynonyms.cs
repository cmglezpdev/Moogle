namespace MoogleEngine;
using System.Text.Json;

public class WorkingSynonyms {

    private class synonyms {
        public List<string[]>? sinonimos { get; set; }
    }


    private List<string[]> Synonyms;
    public WorkingSynonyms( string pathdb ) { // Constructor
        
        // Leer el Json y guardarlo en un string
        string dbString = File.ReadAllText(pathdb);

        // Convertir el string con el json a una structura de datos
        synonyms db = JsonSerializer.Deserialize<synonyms>(dbString)!;
        
        // Guardar el contenido de los sinonimos
        this.Synonyms = db.sinonimos!;
    }


    public int LengthDB {
        get{return this.Synonyms.Count;}
    }

    public string[] GetSynonymsOf( string word ) {
        
        List<string> sinonimos = new List<string>();


        int n = this.LengthDB;

        // Ir por cada array de conjuntos de sinonimos
        for(int i = 0; i < n; i ++) {

            bool found = false;

            // Ir por cada palabra de ese conjunto y ver si es la palabra que me interesa
            foreach(string w in this.Synonyms[i]) {
                if( Lemmatization.Stemmer( w ) != word ) continue;
                found = true;
                break;
            }

            if(!found) continue;
            foreach(string w in this.Synonyms[i]) {
                if( Lemmatization.Stemmer( w ) == word ) continue;
                sinonimos.Add(w);
            }
        }

        // Se devuelve un arreglo vacio xq no se encontro nada
        return sinonimos.ToArray();
    }



}