using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using cynosure.Model;

namespace cynosure.Dialogs
{
    [Serializable]
    public class IssueItemsDialog : AbstractItemDialog
    {
        protected override string GetHeader(IDialogContext context)
        {
            return Standup.ItemsSummary("Items already recorded as blocking:", GetItems(context));
        }

        protected override List<string> GetItems(IDialogContext context)
        {
            return GetCurrentStandup(context).Issues;
        }

        protected override string GetPromptText(IDialogContext context)
        {
            string promptText;
            if (GetCurrentStandup(context).Issues.Any())
            {
                promptText = "What other blockers you are facing right now?";
            }
            else
            {
                promptText = "What blockers are you facing at the moment?";
            }
            return promptText;
        }

        override protected async Task ProcessDialogInput(IDialogContext context, string input)
        {
            Standup standup = GetCurrentStandup(context);
            standup.Issues.Add(input);
            context.UserData.SetValue(@"profile", standup);
            RequestInput(context);
        }

        internal override List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("Finished", "Finish editing issues"));
            commands.Add(new Command("Add <item> to needs.", "Add an item to the needs list.", Command.CommandType.Sentence));
            commands.Add(new Command("Remove <item> from needs.", "Add an item to the needs list.", Command.CommandType.Sentence));
            return commands;
        }
        
        internal override string GetCurrentDialogType()
        {
            return "blocking";
        }
    }
}