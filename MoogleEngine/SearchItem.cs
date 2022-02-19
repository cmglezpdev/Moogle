namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet, float score, string missingWords)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.MissingWords = missingWords;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public float Score { get; private set; }

    public string MissingWords { get; private set; }
}
