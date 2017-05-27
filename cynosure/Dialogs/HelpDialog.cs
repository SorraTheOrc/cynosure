using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using cynosure.Model;
using Microsoft.Bot.Connector;

namespace cynosure.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<Standup>
    {
        internal List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("start standup", "Starts a new standup."));
            commands.Add(new Command("edit done", "Edit items listed as done in the current standup."));
            commands.Add(new Command("edit committed", "Edit items listed as committed in the current standup."));
            commands.Add(new Command("edit issues", "Edit items listed as blocking issues in the current standup."));
            commands.Add(new Command("standup summary", "Display a summary of the current standup."));
            commands.Add(new Command("help", "Get hints on what is possible with this bot."));
            return commands;
        }

        public async Task StartAsync(IDialogContext context)
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

            context.Done<object>(null);
        }
    }

    internal class Command
    {
        private string v1;
        private string v2;

        public Command(string trigger, string description)
        {
            this.Trigger = trigger;
            this.Description = description;
        }

        public string Trigger { get; set; }
        public string Description { get; set; }
    }
}