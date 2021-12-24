using MoogleEngine;

class Test{

    static void Main() {

        string[] files = FilesMethods.ReadFolder();
        // TestgetNameFile();
        // TestgetPathFile();
        // TestgetWordsOfSentence();

        Random a = new Random();
        System.Console.WriteLine(a.Next());
        System.Console.WriteLine(a.Next());
        System.Console.WriteLine(a.Next());
        System.Console.WriteLine(a.Next());


        //? Print if all is OK
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("It's OK!😁");
    }


    static void Assert(bool condition, string message) {
        if(!condition)
            throw new System.Exception(message);
    }

} 
