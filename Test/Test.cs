using MoogleEngine;

class Test{

    static void Main() {

        string[] files = FilesMethods.ReadFolder();
        // TestgetNameFile();
        // TestgetPathFile();
        // TestgetWordsOfSentence();




        //? Print if all is OK
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("It's OK!😁");
    }


    static void Assert(bool condition, string message) {
        if(!condition)
            throw new System.Exception(message);
    }

    static void TestgetNameFile() {
        string directory = "../Content/Metodos de c#/MergeSort with Recursion.txt";
        Assert(FilesMethods.getNameFile(directory) == "MergeSort with Recursion", "The file's name is wrong!");
    }
    static void TestgetPathFile() {
        string directory = "../Content/Metodos de c#/MergeSort with Recursion.txt";
        Assert(FilesMethods.getPathFile(directory) == "../Content/Metodos de c#/", "The file's path is wrong!");
    }

    static void TestgetWordsOfSentence() {
        string[] words = {"Hasta", "el", "momento", "hemos", "logrado", "implementar", "gran", "parte", "de", "la", "interfaz", "gráfica"};
        string sentence = "A Hasta el momento hemos logrado implementar gran parte de la interfaz gráfica.";
        Assert(AuxiliarMethods.getWordsOfSentence(sentence) == words, "The words of sentence is wrong");
    }

} 
