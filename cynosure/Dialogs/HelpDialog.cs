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
    public class HelpDialog : AbstractBaseDialog
    {
        override internal List<Command> Commands()
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

        override public async Task StartAsync(IDialogContext context)
        {
            DisplayHelpCard(context);
            context.Done<object>(null);
        }
        
        internal override string GetCurrentDialogType()
        {
            return "help";
        }
    }
}