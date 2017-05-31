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
    public class HelpDialog : IDialog
    {
        private List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("start standup", "Starts a new standup or continues and existing one."));
            commands.Add(new Command("Add <item> to <group>.", "Add a single item to one of the available groups in the current standup."));
            commands.Add(new Command("Promote <item> from <group>.", "Promote a single item from one of the available groups to the next \"higher\" group in the current standup."));
            commands.Add(new Command("Promote <item> from <group>.", "Promote a single item from one of the available groups to the next \"higher\" group in the current standup."));
            commands.Add(new Command("Remove|Demote <item> from <group>.", "Demotes a single item from one of the available groups in the next \"lower\" group in the current standup."));
            commands.Add(new Command("Delete <item> from <group>.", "Deletes a single item from the current standup."));
            commands.Add(new Command("standup summary", "Display a summary of the current standup."));
            commands.Add(new Command("help", "Get hints on what is possible with this bot."));
            return commands;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await DisplayHelpCard(context);
            context.Done<object>(null);
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
    }

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