using cynosure.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace cynosure.Dialogs
{
    [Serializable]
    public abstract class AbstractBaseDialog : IDialog<Standup>
    {
        protected static bool IsLastInput(string input)
        {
            string[] lastWords = new string[] { "nothing", "nothing more", "nothing else", "none", "no", "no more", "finished", "done" };

            bool finished = false;
            foreach (string word in lastWords)
            {
                finished = finished || (input.ToLower() == word);
            }
            return finished;
        }

        protected static bool isAll(string input)
        {
            string[] allWords = new string[] { "all", "everything" };

            bool all = false;
            foreach (string word in allWords)
            {
                all = all || (input.ToLower() == word);
            }
            return all;
        }

        protected static bool IsHelp(string input)
        {
            string[] allWords = new string[] { "help", "what can i do", "what can you do for me?" };

            bool help = false;
            foreach (string word in allWords)
            {
                help = help || (input.ToLower() == word);
            }
            return help;

        }

        protected static bool IsStatus(string input)
        {
            string[] allWords = new string[] { "status", "current status", "where are we at?", "where are we up to?" };

            bool isStatus = false;
            foreach (string word in allWords)
            {
                isStatus = isStatus || (input.ToLower() == word);
            }
            return isStatus;

        }

        async protected Task DisplayHelpCard(IDialogContext context)
        {
            string title = "My main responsibility is to run your standup for you.";
            string help = "My commands:\n\n\n\n";
            foreach (var command in Commands())
            {
                help += "'" + command.Trigger + "' : " + command.Description + "\n\n";
            }

            var reply = context.MakeMessage();
            reply.Text = title + "\n\n" + help;
            reply.Speak = title + "\n\n" + help;
            reply.InputHint = InputHints.AcceptingInput;
            
            List<CardAction> buttons = new List<CardAction>();
            foreach (var command in Commands())
            {
                if (command.Type == Command.CommandType.Button)
                {
                    buttons.Add(new CardAction(ActionTypes.ImBack, command.Trigger, value: command.Trigger.ToLower()));
                }
            }

            await context.PostAsync(reply);
        }

        public abstract Task StartAsync(IDialogContext context);

        internal abstract List<Command> Commands();

        /**
         * Get the name of the item type this dialog works with.
         */
        internal abstract string GetCurrentDialogType();

        internal class Command
        {
            /**
             * Create a command that requires no additional information 
             * to be executed. Such commands can be represented as a 
             * button in the UI.
             */
            public Command(string trigger, string description)
            {
                this.Trigger = trigger;
                this.Description = description;
                this.Type = CommandType.Button;
            }
            
            public Command(string pattern, string description, CommandType type)
            {
                this.Trigger = pattern;
                this.Description = description;
                this.Type = type;
            }

        public enum CommandType { Button, Sentence };
            public string Trigger { get; set; }
            public string Description { get; set; }
            public CommandType? Type { get; set; }
        }
    }
}