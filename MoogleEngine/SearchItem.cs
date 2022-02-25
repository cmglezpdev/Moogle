namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet, float score, string missingWords, string LinkFile)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.MissingWords = missingWords;
        this.LinkFile = LinkFile;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public float Score { get; private set; }

    public string MissingWords { get; private set; }

    public string LinkFile {get; private set;}

    public static bool operator < (SearchItem left, SearchItem right) {
        if(left.Score < right.Score) 
            return true;
        return false;
    }
    public static bool operator > (SearchItem left, SearchItem right) {
        if(left.Score > right.Score) 
            return true;
        return false;
    }

    public static bool operator >= (SearchItem left, SearchItem right) {
            if(left.Score >= right.Score)
                return true;
            return false;
    }
    public static bool operator <= (SearchItem left, SearchItem right) {
            if(left.Score <= right.Score)
                return true;
            return false;
    }


    public static void Sort(SearchItem[] items) {
        SearchItem[] aux = new SearchItem[items.Length];
        Sort(items, aux, 0, items.Length - 1);
        Array.Copy(items, aux, aux.Length);
    }

    private static void Merge( SearchItem[] items, SearchItem[] aux, int l, int piv, int r ) {

        int izq = l;
        int der = piv + 1;
        int idx = l;


        while(izq <= piv && der <= r) {
            if(items[izq] >= items[der])
                aux[idx++] = items[izq++];
            else
                aux[idx++] = items[der++];
        }   

        if(izq <= piv) 
            Array.Copy(items, izq, aux, idx, piv - izq + 1);
        if(der <= r)
            Array.Copy(items, der, aux, idx, r - der + 1);
        Array.Copy(aux, l, items, l, r - l + 1);
    }
    private static void Sort( SearchItem[] items, SearchItem[] aux, int l, int r ) {

        if(l >= r) return;

        int piv = (l + r) / 2;
        
        Sort(items, aux, l, piv);
        Sort(items, aux, piv + 1, r);
    
        Merge(items, aux, l, piv, r);
    }




}
