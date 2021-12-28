namespace MoogleEngine;

public struct info {
    
    List<int> numLine = new List<int> ();
    List<int> numWord = new List<int> ();

    public info(int nl, int nw) {
        numLine.Add(nl);
        numWord.Add(nw);
    }

    public info() {}

    public void AddAppearance(int nl, int nw) {
        this.numLine.Add(nl);
        this.numWord.Add(nw);
    }

    public int AmountAppareance{
        get{return this.numLine.Count;}
    }

    public List<int> NumbersOfLine {
        get{return numLine;}
    }

    public List<int> NumbersOfWord {
        get{return numWord;}
    }



    public static float TFIDF(int doc, int w, ref List<List<info>> WordsOfDocs ) {
        float tfidf = 0.0f;

        int MaxFreq = 0;
        for(int i = 0; i < WordsOfDocs[doc].Count; i ++) {
            MaxFreq = Math.Max(MaxFreq, WordsOfDocs[doc][i].AmountAppareance);
        }

        float tf = (float)(WordsOfDocs[doc][w].AmountAppareance / MaxFreq);
        

        int ni = 0;
        for(int i = 0; i < WordsOfDocs.Count; i ++) {
            if(WordsOfDocs[i][w].AmountAppareance != 0) ni ++;
        }
        float idf = (float)Math.Log10(WordsOfDocs.Count / ni);

        float alfa = 0.5f;
        tfidf = (float)(alfa + (1.0f - alfa) * idf);

        return tfidf;
    }


}