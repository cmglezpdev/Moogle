namespace MoogleEngine;
using System.Text.Json;

public class WorkSynonyms {

    private class Synonyms {
        private List<string[]> synonyms { get; set; }

        public Synonyms(){}

        public int Length {
            get{return this.synonyms.Count;}
        }
    }


    private Synonyms db;

    public WorkSynonyms( string pathdb ) {
        string dbString = File.ReadAllText(pathdb);
       this.db = JsonSerializer.Deserialize<Synonyms>(dbString);
    }

    public int GetLengthDB() {
        return this.db.Length;
    }

}