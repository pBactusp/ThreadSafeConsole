using System.Collections.Concurrent;

public static class TsConsole // Thread safe console
{
    public static int SleepTime = 100;

    static ConcurrentQueue<TsConsoleActions.TsConsoleAction> _actions = new ConcurrentQueue<TsConsoleActions.TsConsoleAction>();

    static TsConsole()
    {
        Task.Run(() => Start());
    }

    static async void Start()
    {
        while (true)
        {
            if (_actions.Count > 0)
            {
                //_actions.Dequeue().Perform();

                if (_actions.TryDequeue(out TsConsoleActions.TsConsoleAction action))
                    action.Perform();
            }
            else
                await Task.Delay(SleepTime);
        }
    }

    public static void AddActions(TsConsoleActions.TsConsoleAction action)
    {
        _actions.Enqueue(action);
    }

    public static void Write(string message)
    {
        AddActions(new TsConsoleActions.Write(message));
    }
    public static void WriteLine(string message)
    {
        AddActions(new TsConsoleActions.Write(message + Environment.NewLine));
    }

    public static void WriteInColor(string message, ConsoleColor color)
    {
        ConsoleColor currentColor = Console.ForegroundColor;

        _actions.Enqueue(new TsConsoleActions.ActionGroup(
                    new TsConsoleActions.SetForground(color),
                    new TsConsoleActions.Write(message),
                    new TsConsoleActions.SetForground(currentColor)
                    ));
    }
    public static void WriteLineInColor(string message, ConsoleColor color)
    {
        WriteInColor(message + Environment.NewLine, color);
    }

    public static void Clear()
    {
        AddActions(new TsConsoleActions.Clear());
    }
}


namespace TsConsoleActions
{
    public abstract class TsConsoleAction
    {
        public abstract void Perform();
    }
    public class Write : TsConsoleAction
    {
        string text;

        public Write(string text)
        {
            this.text = text;
        }

        public override void Perform()
        {
            Console.Write(text);
        }
    }
    public class SetForground : TsConsoleAction
    {
        ConsoleColor color;

        public SetForground(ConsoleColor color)
        {
            this.color = color;
        }

        public override void Perform()
        {
            Console.ForegroundColor = color;
        }
    }
    public class SetBackground : TsConsoleAction
    {
        ConsoleColor color;

        public SetBackground(ConsoleColor color)
        {
            this.color = color;
        }

        public override void Perform()
        {
            Console.BackgroundColor = color;
        }
    }
    public class Clear : TsConsoleAction
    {
        ConsoleColor color;

        public Clear()
        {
        }

        public override void Perform()
        {
            Console.Clear();
        }
    }
    public class ActionGroup : TsConsoleAction
    {
        TsConsoleAction[] actions;

        public ActionGroup(params TsConsoleAction[] actions)
        {
            this.actions = actions;
        }

        public override void Perform()
        {
            for (int i = 0; i < actions.Length; i++)
                actions[i].Perform();
        }
    }

}

