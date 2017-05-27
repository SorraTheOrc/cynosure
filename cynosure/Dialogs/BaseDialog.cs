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
    public abstract class BaseDialog: IDialog<Standup>
    {
        protected Standup _standup;

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

        protected async Task SummaryReportAsync(IDialogContext context)
        {
            string summary = _standup.Summary();

            var text = "Your current standup report is:\n\n\n\n" + summary;
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );
            await context.PostAsync(text);
        }

        async protected Task DisplayHelpCard(IDialogContext context)
        {
            string title = "My main resonsibility is to run your standup for you.";
            string help = "My commands:\n\n\n\n";
            foreach (var command in Commands())
            {
                help += "'" + command.Trigger + "' : " + command.Description + "\n\n";
            }

            var reply = context.MakeMessage();
            reply.Speak = title + "\n\n" + help;
            reply.InputHint = InputHints.AcceptingInput;


            List<CardAction> buttons = new List<CardAction>();
            foreach (var command in Commands())
            {
                buttons.Add(new CardAction(ActionTypes.ImBack, command.Trigger, value: command.Trigger.ToLower()));
            }

            reply.Attachments = new List<Attachment>
            {
                new HeroCard(title)
                {
                    Buttons = buttons
                }.ToAttachment()
            };

            await context.PostAsync(reply);
        }

        public abstract Task StartAsync(IDialogContext context);

        internal abstract List<Command> Commands();
        
        internal class Command
        {
            public Command(string trigger, string description)
            {
                this.Trigger = trigger;
                this.Description = description;
            }

            public string Trigger { get; set; }
            public string Description { get; set; }
        }
    }
}