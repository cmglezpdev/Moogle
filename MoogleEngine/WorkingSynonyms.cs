namespace MoogleEngine;
using System.Text.Json;

public class WorkingSynonyms {

    private class synonyms {
        public List<string[]> sinonimos { get; set; }
    }


    private List<string[]> Synonyms;
    public WorkingSynonyms( string pathdb ) { // Constructor
        
        // Leer el Json y guardarlo en un string
        string dbString = File.ReadAllText(pathdb);

        // Convertir el string con el json a una structura de datos
        synonyms db = JsonSerializer.Deserialize<synonyms>(dbString);
        
        // Guardar el contenido de los sinonimos
        this.Synonyms = db.sinonimos;
    }


    public int LengthDB {
        get{return this.Synonyms.Count;}
    }




}