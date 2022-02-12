namespace MoogleEngine;
using System.Text.Json;

public class WorkingSynonyms {

    private class Synonyms {
        private List<string[]> synonyms { get; set; }
        public Synonyms(){}
        public int Length {
            get{return this.synonyms.Count;}
        }
    }

    private Synonyms db;

    public WorkingSynonyms( string pathdb ) {
        string dbString = File.ReadAllText(pathdb);
        this.db = JsonSerializer.Deserialize<Synonyms>(dbString);
    }



    public int GetLengthDB() {
        return this.db.Length;
    }

}